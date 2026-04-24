import Foundation

@MainActor
final class AppModel: ObservableObject {
    @Published var minWPM: Int = 90 {
        didSet { normalizeSettings() }
    }
    @Published var maxWPM: Int = 130 {
        didSet { normalizeSettings() }
    }
    @Published var typoRate: Double = 0.05 {
        didSet { normalizeSettings() }
    }
    @Published var isTyping = false
    @Published var statusText: String = "Idle"
    @Published var updateStatus: String = "Not checked"

    let engine = TypingEngine()
    let updater = UpdateChecker()

    var menuBarSymbolName: String {
        isTyping ? "keyboard.badge.ellipsis" : "keyboard"
    }

    func typeClipboard() {
        normalizeSettings()
        statusText = "Typing..."
        engine.typeClipboard(
            minWPM: minWPM,
            maxWPM: maxWPM,
            typoRate: typoRate,
            stateDidChange: { [weak self] isTyping in
                self?.isTyping = isTyping
            }
        ) { [weak self] result in
            Task { @MainActor in
                self?.isTyping = false
                self?.statusText = result
            }
        }
    }

    func stopTyping() {
        engine.stop()
        isTyping = false
        statusText = "Stopped"
    }

    func checkForUpdates() {
        updater.checkForUpdates { [weak self] status in
            Task { @MainActor in
                self?.updateStatus = status
            }
        }
    }

    private func normalizeSettings() {
        let boundedMin = max(10, min(minWPM, 260))
        let boundedMax = max(10, min(maxWPM, 260))

        if minWPM != boundedMin {
            minWPM = boundedMin
            return
        }

        if maxWPM != boundedMax {
            maxWPM = boundedMax
            return
        }

        if minWPM > maxWPM {
            maxWPM = minWPM
            return
        }

        let boundedTypoRate = min(max(typoRate, 0), 0.30)
        if typoRate != boundedTypoRate {
            typoRate = boundedTypoRate
        }
    }
}
