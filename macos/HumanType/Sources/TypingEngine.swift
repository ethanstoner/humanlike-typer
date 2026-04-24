import ApplicationServices
import AppKit
import Foundation

@MainActor
final class TypingEngine {
    private(set) var isTyping = false

    private var typingTask: Task<Void, Never>?
    private var burstWPM: Int?
    private var burstCharsLeft = 0
    private var nextAllowedTypoAt = 0

    private let defaultWordPauseChance = 0.08

    private let smartPunctuationMap: [Character: String] = [
        "\u{2018}": "'",
        "\u{2019}": "'",
        "\u{201C}": "\"",
        "\u{201D}": "\"",
        "\u{2013}": "-",
        "\u{2014}": "--",
        "\u{2026}": "...",
        "\u{00A0}": " "
    ]

    private let neighbors: [Character: [Character]] = [
        "a": ["q", "w", "s", "z"],
        "b": ["v", "g", "h", "n"],
        "c": ["x", "d", "f", "v"],
        "d": ["s", "e", "r", "f", "c", "x"],
        "e": ["w", "s", "d", "r"],
        "f": ["d", "r", "t", "g", "v", "c"],
        "g": ["f", "t", "y", "h", "b", "v"],
        "h": ["g", "y", "u", "j", "n", "b"],
        "i": ["u", "j", "k", "o"],
        "j": ["h", "u", "i", "k", "m", "n"],
        "k": ["j", "i", "o", "l", "m"],
        "l": ["k", "o", "p"],
        "m": ["n", "j", "k"],
        "n": ["b", "h", "j", "m"],
        "o": ["i", "k", "l", "p"],
        "p": ["o", "l"],
        "q": ["w", "a"],
        "r": ["e", "d", "f", "t"],
        "s": ["a", "w", "e", "d", "x", "z"],
        "t": ["r", "f", "g", "y"],
        "u": ["y", "h", "j", "i"],
        "v": ["c", "f", "g", "b"],
        "w": ["q", "a", "s", "e"],
        "x": ["z", "s", "d", "c"],
        "y": ["t", "g", "h", "u"],
        "z": ["a", "s", "x"],
        "1": ["2", "q"],
        "2": ["1", "3", "q", "w"],
        "3": ["2", "4", "w", "e"],
        "4": ["3", "5", "e", "r"],
        "5": ["4", "6", "r", "t"],
        "6": ["5", "7", "t", "y"],
        "7": ["6", "8", "y", "u"],
        "8": ["7", "9", "u", "i"],
        "9": ["8", "0", "i", "o"],
        "0": ["9", "o", "p"]
    ]

    func typeClipboard(
        minWPM: Int,
        maxWPM: Int,
        typoRate: Double,
        stateDidChange: @escaping (Bool) -> Void,
        completion: @escaping (String) -> Void
    ) {
        guard !isTyping else {
            completion("Already typing")
            return
        }

        guard requestAccessibilityIfNeeded() else {
            completion("Accessibility access is required")
            return
        }

        guard let text = NSPasteboard.general.string(forType: .string), !text.isEmpty else {
            completion("Clipboard is empty")
            return
        }

        let sanitized = sanitize(text)
        guard !sanitized.isEmpty else {
            completion("Clipboard has no typable text")
            return
        }

        let boundedMin = max(10, min(minWPM, 260))
        let boundedMax = max(boundedMin, min(maxWPM, 260))
        let boundedTypoRate = min(max(typoRate, 0), 0.30)

        isTyping = true
        stateDidChange(true)
        burstWPM = nil
        burstCharsLeft = 0
        nextAllowedTypoAt = 0

        typingTask = Task { [weak self] in
            guard let self else { return }

            let result: String
            do {
                try await self.typeText(
                    sanitized,
                    minWPM: boundedMin,
                    maxWPM: boundedMax,
                    typoRate: boundedTypoRate
                )
                result = Task.isCancelled ? "Stopped" : "Finished typing"
            } catch is CancellationError {
                result = "Stopped"
            } catch {
                result = "Typing failed: \(error.localizedDescription)"
            }

            await MainActor.run {
                self.typingTask = nil
                self.isTyping = false
                stateDidChange(false)
                completion(result)
            }
        }
    }

    func stop() {
        typingTask?.cancel()
        typingTask = nil
        isTyping = false
    }

    private func requestAccessibilityIfNeeded() -> Bool {
        let options = [kAXTrustedCheckOptionPrompt.takeUnretainedValue() as String: true] as CFDictionary
        return AXIsProcessTrustedWithOptions(options)
    }

    private func typeText(_ text: String, minWPM: Int, maxWPM: Int, typoRate: Double) async throws {
        var index = text.startIndex
        var previousCharacter: Character?

        while index < text.endIndex {
            try Task.checkCancellation()

            let current = text[index]
            let nextIndex = text.index(after: index)
            let nextCharacter = nextIndex < text.endIndex ? text[nextIndex] : nil

            if let plan = maybeBuildTypoPlan(
                in: text,
                at: index,
                previousCharacter: previousCharacter,
                typoRate: typoRate
            ) {
                try await sendChunk(plan.typed, previousCharacter: previousCharacter, minWPM: minWPM, maxWPM: maxWPM, speedScale: 0.96)
                try await sleep(plan.noticeDelay)
                try await backspace(plan.typed.count, delay: plan.backspaceDelay)
                try await sendChunk(plan.correct, previousCharacter: previousCharacter, minWPM: minWPM, maxWPM: maxWPM, speedScale: 1.08)

                nextAllowedTypoAt = plan.absoluteIndex + Int.random(in: 9...16)
                previousCharacter = plan.correct.last
                index = plan.resumeIndex
                continue
            }

            try sendCharacter(current)
            if nextIndex < text.endIndex {
                let delay = charDelay(
                    current: current,
                    previous: previousCharacter,
                    next: nextCharacter,
                    minWPM: minWPM,
                    maxWPM: maxWPM,
                    speedScale: 1.0
                )
                try await sleep(delay)
            }

            previousCharacter = current
            index = nextIndex
        }
    }

    private func sendChunk(
        _ chunk: String,
        previousCharacter: Character?,
        minWPM: Int,
        maxWPM: Int,
        speedScale: Double
    ) async throws {
        var previous = previousCharacter
        let characters = Array(chunk)

        for (offset, character) in characters.enumerated() {
            try Task.checkCancellation()
            try sendCharacter(character)

            if offset < characters.count - 1 {
                let next = characters[offset + 1]
                let delay = charDelay(
                    current: character,
                    previous: previous,
                    next: next,
                    minWPM: minWPM,
                    maxWPM: maxWPM,
                    speedScale: speedScale
                )
                try await sleep(delay)
            }

            previous = character
        }
    }

    private func backspace(_ count: Int, delay: Duration) async throws {
        for _ in 0..<count {
            try Task.checkCancellation()
            guard let down = CGEvent(keyboardEventSource: nil, virtualKey: 51, keyDown: true),
                  let up = CGEvent(keyboardEventSource: nil, virtualKey: 51, keyDown: false) else {
                throw TypingError.eventCreationFailed
            }

            down.post(tap: .cghidEventTap)
            up.post(tap: .cghidEventTap)
            try await sleep(delay)
        }
    }

    private func sendCharacter(_ character: Character) throws {
        switch character {
        case "\n", "\r":
            try postKeyCode(36)
        case "\t":
            try postKeyCode(48)
        default:
            let scalarString = String(character)
            guard let down = CGEvent(keyboardEventSource: nil, virtualKey: 0, keyDown: true),
                  let up = CGEvent(keyboardEventSource: nil, virtualKey: 0, keyDown: false) else {
                throw TypingError.eventCreationFailed
            }

            down.keyboardSetUnicodeString(stringLength: scalarString.utf16.count, unicodeString: Array(scalarString.utf16))
            up.keyboardSetUnicodeString(stringLength: scalarString.utf16.count, unicodeString: Array(scalarString.utf16))
            down.post(tap: .cghidEventTap)
            up.post(tap: .cghidEventTap)
        }
    }

    private func postKeyCode(_ keyCode: CGKeyCode) throws {
        guard let down = CGEvent(keyboardEventSource: nil, virtualKey: keyCode, keyDown: true),
              let up = CGEvent(keyboardEventSource: nil, virtualKey: keyCode, keyDown: false) else {
            throw TypingError.eventCreationFailed
        }

        down.post(tap: .cghidEventTap)
        up.post(tap: .cghidEventTap)
    }

    private func sanitize(_ text: String) -> String {
        let normalized = text.applyingTransform(.toLatin, reverse: false) ?? text
        let folded = normalized.folding(options: [.diacriticInsensitive, .widthInsensitive], locale: .current)

        var result = ""
        for character in folded {
            if let replacement = smartPunctuationMap[character] {
                result.append(replacement)
                continue
            }

            guard let scalar = character.unicodeScalars.first, character.unicodeScalars.count == 1 else {
                continue
            }

            switch scalar.value {
            case 9, 10, 13, 32...126:
                result.append(character)
            default:
                continue
            }
        }

        return result
    }

    private func maybeBuildTypoPlan(
        in text: String,
        at index: String.Index,
        previousCharacter: Character?,
        typoRate: Double
    ) -> TypoPlan? {
        let absoluteIndex = text.distance(from: text.startIndex, to: index) + 1
        guard absoluteIndex >= nextAllowedTypoAt else { return nil }
        guard Double.random(in: 0...1) < typoRate else { return nil }

        let current = text[index]
        let nextIndex = text.index(after: index)
        guard nextIndex < text.endIndex else { return nil }
        let next = text[nextIndex]
        guard isLetter(current), isLetter(previousCharacter), isLetter(next) else { return nil }

        let wordEnd = wordEndIndex(in: text, start: index)
        let remaining = text.distance(from: index, to: wordEnd) + 1
        guard remaining >= 2 else { return nil }

        let carry = min(Int.random(in: 1...3), remaining - 1)
        let useTransposition = remaining >= 2 && Double.random(in: 0...1) < 0.35

        if useTransposition {
            let chunkLength = min(2 + carry, remaining)
            let correct = substring(text, from: index, length: chunkLength)
            let secondIndex = text.index(after: index)
            var typed = String(text[secondIndex]) + String(current)
            if chunkLength > 2 {
                let tailStart = text.index(after: secondIndex)
                typed += substring(text, from: tailStart, length: chunkLength - 2)
            }

            return TypoPlan(
                typed: typed,
                correct: correct,
                resumeIndex: text.index(index, offsetBy: chunkLength),
                noticeDelay: .milliseconds(Int.random(in: 120...420)),
                backspaceDelay: .milliseconds(Int.random(in: 40...85)),
                absoluteIndex: absoluteIndex
            )
        }

        guard let neighbor = randomNeighbor(for: current) else { return nil }

        let chunkLength = min(1 + carry, remaining)
        let correct = substring(text, from: index, length: chunkLength)
        var typed = String(neighbor)
        if chunkLength > 1 {
            typed += substring(text, from: nextIndex, length: chunkLength - 1)
        }

        return TypoPlan(
            typed: typed,
            correct: correct,
            resumeIndex: text.index(index, offsetBy: chunkLength),
            noticeDelay: .milliseconds(Int.random(in: 140...460)),
            backspaceDelay: .milliseconds(Int.random(in: 45...95)),
            absoluteIndex: absoluteIndex
        )
    }

    private func wordEndIndex(in text: String, start: String.Index) -> String.Index {
        var index = start
        while index < text.endIndex, isLetter(text[index]) {
            index = text.index(after: index)
        }

        return text.index(before: index)
    }

    private func substring(_ text: String, from start: String.Index, length: Int) -> String {
        let end = text.index(start, offsetBy: length)
        return String(text[start..<end])
    }

    private func randomNeighbor(for character: Character) -> Character? {
        let lowercased = Character(String(character).lowercased())
        guard let options = neighbors[lowercased], let choice = options.randomElement() else {
            return nil
        }

        return String(character).uppercased() == String(character) ? Character(String(choice).uppercased()) : choice
    }

    private func currentBurstWPM(minWPM: Int, maxWPM: Int) -> Int {
        if burstWPM == nil || burstCharsLeft <= 0 {
            burstWPM = Int.random(in: minWPM...maxWPM)
            burstCharsLeft = Int.random(in: 4...9)
        }

        burstCharsLeft -= 1
        return burstWPM ?? minWPM
    }

    private func charDelay(
        current: Character,
        previous: Character?,
        next: Character?,
        minWPM: Int,
        maxWPM: Int,
        speedScale: Double
    ) -> Duration {
        let wpm = currentBurstWPM(minWPM: minWPM, maxWPM: maxWPM)
        let cps = Double(wpm * 5) / 60.0
        var delayMs = (1000.0 / cps) * Double.random(in: 0.82...1.18) * speedScale

        if current == " " {
            if Double.random(in: 0...1) < defaultWordPauseChance {
                delayMs += Double.random(in: 40...140)
            }
        } else if [",", ";", ":"].contains(current) {
            delayMs += Double.random(in: 45...120)
        } else if [".", "!", "?"].contains(current) {
            delayMs += Double.random(in: 110...280)
        } else if previous == " ", isLetter(current), next != nil, Double.random(in: 0...1) < 0.18 {
            delayMs += Double.random(in: 25...80)
        }

        return .milliseconds(max(1, Int(delayMs.rounded())))
    }

    private func isLetter(_ character: Character?) -> Bool {
        guard let character, let scalar = character.unicodeScalars.first, character.unicodeScalars.count == 1 else {
            return false
        }

        return CharacterSet.letters.contains(scalar)
    }

    private func sleep(_ duration: Duration) async throws {
        try await Task.sleep(for: duration)
    }
}

private struct TypoPlan {
    let typed: String
    let correct: String
    let resumeIndex: String.Index
    let noticeDelay: Duration
    let backspaceDelay: Duration
    let absoluteIndex: Int
}

private enum TypingError: LocalizedError {
    case eventCreationFailed

    var errorDescription: String? {
        switch self {
        case .eventCreationFailed:
            return "Unable to create keyboard events"
        }
    }
}
