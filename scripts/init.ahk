; === HumanLike Typer for Windows ===
; Realistic human typing emulator with natural patterns
; 
; Copyright (c) 2025 HumanLike Typer Contributors
; Licensed under CC BY-NC-SA 4.0
; https://creativecommons.org/licenses/by-nc-sa/4.0/
; 
; Free to use, modify, and share for non-commercial purposes.
; Cannot be sold or used commercially.
; 
; Tray Icon: ○ when idle, ● while typing
; Right-click icon for menu: Type Clipboard, Settings, Reload
; Hotkey: Ctrl+Shift+V → type clipboard immediately
; Esc → cancel typing

;@Ahk2Exe-SetMainIcon ..\assets\humantyperlogo.ico
;@Ahk2Exe-SetCompanyName HumanLike Typer Contributors
;@Ahk2Exe-SetCopyright Copyright (c) 2025 - CC BY-NC-SA 4.0
;@Ahk2Exe-SetDescription Realistic human typing emulator
;@Ahk2Exe-SetVersion 1.1.1

#Requires AutoHotkey v2.0
#SingleInstance Force
SendMode("Input")  ; Use Input mode for fastest, most reliable sending
SetKeyDelay(10, 10)  ; Small delay for special keys to ensure processing

; === Default Settings ===
global DEFAULT_MIN_WPM := 90
global DEFAULT_MAX_WPM := 130
global DEFAULT_TYPO_RATE := 0.05  ; keep your requested default 5%
global DEFAULT_SPACE_PAUSE := 0.08

global minWPM := DEFAULT_MIN_WPM
global maxWPM := DEFAULT_MAX_WPM
global correctionChance := DEFAULT_TYPO_RATE
global wordPauseChance := DEFAULT_SPACE_PAUSE

global typingInProgress := false
global currentText := ""
global currentIndex := 0
global textLength := 0
global settingsGui := ""

; --- Typo pacing controls ---
global lastTypoIndex := -999
global MIN_TYPO_GAP_CHARS := 9      ; allow more frequent typos at 0.05
global MAX_TYPO_GAP_CHARS := 16
global nextAllowedTypoAt := 0

; === Initialize Tray Icon ===
TraySetIcon("Shell32.dll", 264)  ; Keyboard icon
A_IconTip := "HumanLike Typer"
TrayIdle()

; === Tray Menu ===
A_TrayMenu.Delete()
A_TrayMenu.Add("Type Clipboard", MenuTypeClipboard)
A_TrayMenu.Add()
A_TrayMenu.Add("Settings...", MenuSettings)
A_TrayMenu.Add()
A_TrayMenu.Add("Reload", MenuReload)
A_TrayMenu.Add("Exit", MenuExit)
A_TrayMenu.Default := "Type Clipboard"

; === QWERTY Neighbor Mapping ===
global NEIGHBORS := Map(
    "a", ["q","w","s","z"],
    "b", ["v","g","h","n"],
    "c", ["x","d","f","v"],
    "d", ["s","e","r","f","c","x"],
    "e", ["w","s","d","r"],
    "f", ["d","r","t","g","v","c"],
    "g", ["f","t","y","h","b","v"],
    "h", ["g","y","u","j","n","b"],
    "i", ["u","j","k","o"],
    "j", ["h","u","i","k","m","n"],
    "k", ["j","i","o","l","m"],
    "l", ["k","o","p"],
    "m", ["n","j","k"],
    "n", ["b","h","j","m"],
    "o", ["i","k","l","p"],
    "p", ["o","l"],
    "q", ["w","a"],
    "r", ["e","d","f","t"],
    "s", ["a","w","e","d","x","z"],
    "t", ["r","f","g","y"],
    "u", ["y","h","j","i"],
    "v", ["c","f","g","b"],
    "w", ["q","a","s","e"],
    "x", ["z","s","d","c"],
    "y", ["t","g","h","u"],
    "z", ["a","s","x"],
    "1", ["2","q"],
    "2", ["1","3","q","w"],
    "3", ["2","4","w","e"],
    "4", ["3","5","e","r"],
    "5", ["4","6","r","t"],
    "6", ["5","7","t","y"],
    "7", ["6","8","y","u"],
    "8", ["7","9","u","i"],
    "9", ["8","0","i","o"],
    "0", ["9","o","p"]
)

; === Text Sanitization ===
SanitizeText(text) {
    if (!text)
        return ""
    
    ; Convert smart punctuation to ASCII using character codes
    text := StrReplace(text, Chr(0x2018), "'")  ; Left single quote
    text := StrReplace(text, Chr(0x2019), "'")  ; Right single quote/apostrophe
    text := StrReplace(text, Chr(0x201C), '"')  ; Left double quote
    text := StrReplace(text, Chr(0x201D), '"')  ; Right double quote
    text := StrReplace(text, Chr(0x2013), "-")  ; En-dash
    text := StrReplace(text, Chr(0x2014), "--") ; Em-dash
    text := StrReplace(text, Chr(0x2026), "...") ; Ellipsis
    text := StrReplace(text, Chr(0xA0), " ") ; NBSP
    
    ; Remove control characters except newline, tab, carriage return
    result := ""
    Loop Parse, text {
        code := Ord(A_LoopField)
        ; Keep printable ASCII (32-126), newline (10), tab (9), carriage return (13)
        if (code = 9 || code = 10 || code = 13 || (code >= 32 && code <= 126))
            result .= A_LoopField
    }
    
    return result
}

; === Random Neighbor for Typos ===
RandomNeighbor(char) {
    global NEIGHBORS
    lower := StrLower(char)
    if (!NEIGHBORS.Has(lower))
        return ""
    
    ; Safely get the neighbors array - use .Get() to ensure proper type
    neighbors := NEIGHBORS.Get(lower)
    
    ; Type check: ensure neighbors is an Array, not a String
    if (Type(neighbors) != "Array") {
        ; If somehow it's a string, return empty
        return ""
    }
    
    if (neighbors.Length = 0)
        return ""
    
    pick := neighbors[Random(1, neighbors.Length)]
    
    ; Preserve case for letters
    if (RegExMatch(char, "[A-Z]"))
        pick := StrUpper(pick)
    
    return pick
}

; === Helper: Character classification ===
IsLetter(c) {
    return RegExMatch(c, "^[A-Za-z]$")
}

IsDigit(c) {
    return RegExMatch(c, "^[0-9]$")
}

IsSpace(c) {
    return (c = " " || c = "`t" || c = "`r" || c = "`n")
}

IsPunct(c) {
    return RegExMatch(c, "^[.,!?;:)]}-]$")
}

; Inside-word if both neighbors are letters
IsInsideWord(prev, next) {
    return (IsLetter(prev) && IsLetter(next))
}

; === Helper: Send Backspace reliably ===
SendBackspaceAPI() {
    ; Use direct Windows API for maximum reliability
    ; VK_BACK = 0x08, send key down then up
    DllCall("keybd_event", "UChar", 0x08, "UChar", 0, "UInt", 0, "Ptr", 0)  ; Key down
    Sleep(20)  ; Hold key down slightly longer for better compatibility
    DllCall("keybd_event", "UChar", 0x08, "UChar", 0, "UInt", 0x0002, "Ptr", 0)  ; Key up
    Sleep(10)  ; Brief pause after release to ensure processing
}

; === Typo Simulation (Context-Aware) ===
MaybeDoTypo(curr, nextChar, &skipNext, currentWPM := 100, index := 1, prevChar := "", afterChar := "") {
    global correctionChance, typingInProgress
    global lastTypoIndex, nextAllowedTypoAt, MIN_TYPO_GAP_CHARS, MAX_TYPO_GAP_CHARS
    
    skipNext := false
    
    if (!typingInProgress) {
        return false
    }
    
    ; Only typo on letters, mid-word
    if !(RegExMatch(curr, "^[A-Za-z]$")) {
        return false
    }
    
    if !(RegExMatch(prevChar, "^[A-Za-z]$") && RegExMatch(nextChar, "^[A-Za-z]$")) {
        return false
    }
    
    ; Enforce spacing so they don't cluster
    if (index < nextAllowedTypoAt) {
        return false
    }
    
    ; Probability gate (your 0.05 default is honored here)
    if (Random(0.0, 1.0) >= correctionChance) {
        return false
    }
    
    ; Adaptive delays: long enough for backspace to register at high WPM
    baseDelay := 200 + (currentWPM - 90) * 3
    if (baseDelay < 170) {
        baseDelay := 170
    }
    backspaceDelay := 160 + (currentWPM - 90) * 2.5
    if (backspaceDelay < 140) {
        backspaceDelay := 140
    }
    
    ; 35% transpositions; otherwise neighbor mis-hit
    doTransposition := (RegExMatch(nextChar, "^[A-Za-z]$") && Random(0.0, 1.0) < 0.35)
    
    if (doTransposition) {
        SendText(nextChar)
        Sleep(baseDelay)
        if (!typingInProgress) {
            return false
        }
        SendBackspaceAPI()
        Sleep(backspaceDelay)
        if (!typingInProgress) {
            return false
        }
        SendText(curr)
        skipNext := true
    } else {
        neigh := RandomNeighbor(curr)
        if (neigh = "") {
            return false
        }
        SendText(neigh)
        Sleep(baseDelay + 20)
        if (!typingInProgress) {
            return false
        }
        SendBackspaceAPI()
        Sleep(backspaceDelay)
        if (!typingInProgress) {
            return false
        }
        SendText(curr)
    }
    
    ; Push the next allowed typo forward
    lastTypoIndex := index
    gap := Random(MIN_TYPO_GAP_CHARS, MAX_TYPO_GAP_CHARS)
    nextAllowedTypoAt := index + gap
    return true
}

; === Main Typing Function ===
TypeHuman(text) {
    global typingInProgress, currentText, currentIndex, textLength
    
    ; Prevent multiple instances
    if (typingInProgress) {
        ToolTip("Already typing! Press ESC to stop.")
        SetTimer(() => ToolTip(), -1000)
        return
    }
    
    if (!text || StrLen(text) = 0)
        return
    
    text := SanitizeText(text)
    if (StrLen(text) = 0)
        return
    
    ; Initialize global state
    typingInProgress := true
    currentText := text
    currentIndex := 1
    textLength := StrLen(text)
    TrayTyping()
    
    ; Reset typo pacing when a new run starts
    global lastTypoIndex, nextAllowedTypoAt
    lastTypoIndex := -999
    nextAllowedTypoAt := 0  ; IMPORTANT: allow typos immediately if eligible
    
    ; Start typing
    TypeStep()
}

; === Typing Step Function ===
TypeStep() {
    global typingInProgress, currentText, currentIndex, textLength, minWPM, maxWPM, wordPauseChance
    
    ; Check if we should stop
    if (!typingInProgress || currentIndex > textLength) {
        StopTyping()
        return
    }
    
    c := SubStr(currentText, currentIndex, 1)
    nextChar := (currentIndex < textLength) ? SubStr(currentText, currentIndex + 1, 1) : ""
    
    ; Calculate delay based on WPM
    wpm := Random(minWPM, maxWPM)
    cps := (wpm * 5) / 60
    delay := (1000 / cps) * (Random(90, 110) / 100)
    
    ; Add pauses after spaces and punctuation
    if (c = " " && Random(0.0, 1.0) < wordPauseChance)
        delay += (60 * Random(70, 130) / 100)
    if (InStr(".!?", c))
        delay += 80
    
    ; Optional: slightly tone down space pauses at high WPM (keeps rhythm crisp)
    if (wpm >= 120 && c = " ")
        delay *= 0.9
    
    ; Get context characters for typo detection
    prevChar := (currentIndex > 1) ? SubStr(currentText, currentIndex - 1, 1) : ""
    afterChar := (currentIndex + 1 <= textLength) ? SubStr(currentText, currentIndex + 1, 1) : ""
    
    ; Check for typo - pass current WPM, index, and context for adaptive delays
    skipNext := false
    if (!MaybeDoTypo(c, nextChar, &skipNext, wpm, currentIndex, prevChar, afterChar)) {
        SendText(c)
    }
    
    if (skipNext) {
        currentIndex += 2
    } else {
        currentIndex += 1
    }
    
    ; Schedule next character
    SetTimer(TypeStep, Integer(delay))
}

; === Stop Typing ===
StopTyping() {
    global typingInProgress, currentText, currentIndex, textLength
    
    ; Stop the timer
    SetTimer(TypeStep, 0)
    
    ; Reset state
    typingInProgress := false
    currentText := ""
    currentIndex := 0
    textLength := 0
    
    TrayIdle()
}

; === Tray Icon States ===
TrayIdle() {
    A_IconTip := "HumanLike Typer - Idle"
    TraySetIcon("Shell32.dll", 264)
}

TrayTyping() {
    A_IconTip := "HumanLike Typer - Typing..."
    TraySetIcon("Shell32.dll", 265)
}

; === Menu Handlers ===
MenuTypeClipboard(*) {
    clip := A_Clipboard
    if (clip)
        TypeHuman(clip)
}

MenuSettings(*) {
    ShowSettings()
}

MenuReload(*) {
    Reload
}

MenuExit(*) {
    ExitApp
}

; === Settings GUI ===
ShowSettings() {
    global settingsGui, minWPM, maxWPM, correctionChance
    
    ; Close existing window if open
    if (settingsGui) {
        try settingsGui.Destroy()
        settingsGui := ""
    }
    
    ; Create modern GUI with rounded borders
    settingsGui := Gui("+AlwaysOnTop -Caption +Border", "HumanLike Typer")
    settingsGui.BackColor := "0x0F0F0F"
    settingsGui.MarginX := 0
    settingsGui.MarginY := 0
    
    ; === HEADER ===
    settingsGui.SetFont("s12 Bold c0xFFFFFF", "Segoe UI")
    settingsGui.Add("Text", "x0 y0 w440 h50 Background0x1a1a1a Center 0x200", "⚙️ Settings")
    
    ; === CONTENT AREA ===
    ; Min WPM Section
    settingsGui.SetFont("s9 c0x999999", "Segoe UI")
    settingsGui.Add("Text", "x30 y70 w180", "MINIMUM WPM")
    settingsGui.SetFont("s10 c0xE0E0E0", "Segoe UI")
    minWpmEdit := settingsGui.Add("Edit", "x30 y92 w180 h32 Background0x1E1E1E c0xFFFFFF", minWPM)
    
    ; Max WPM Section
    settingsGui.SetFont("s9 c0x999999", "Segoe UI")
    settingsGui.Add("Text", "x230 y70 w180", "MAXIMUM WPM")
    settingsGui.SetFont("s10 c0xE0E0E0", "Segoe UI")
    maxWpmEdit := settingsGui.Add("Edit", "x230 y92 w180 h32 Background0x1E1E1E c0xFFFFFF", maxWPM)
    
    ; Range hint
    settingsGui.SetFont("s8 c0x666666", "Segoe UI")
    settingsGui.Add("Text", "x30 y128 w380 Center", "Valid range: 10 - 260 WPM")
    
    ; Typo Rate Section
    settingsGui.SetFont("s9 c0x999999", "Segoe UI")
    settingsGui.Add("Text", "x30 y160 w380", "TYPO FREQUENCY")
    settingsGui.SetFont("s10 c0xE0E0E0", "Segoe UI")
    typoEdit := settingsGui.Add("Edit", "x30 y182 w380 h32 Background0x1E1E1E c0xFFFFFF", Format("{:.2f}", correctionChance))
    
    ; Typo hint
    settingsGui.SetFont("s8 c0x666666", "Segoe UI")
    settingsGui.Add("Text", "x30 y218 w380 Center", "0.00 = No typos  •  0.30 = Maximum typos")
    
    ; === DIVIDER LINE ===
    settingsGui.Add("Text", "x30 y250 w380 h1 Background0x2A2A2A")
    
    ; === INFO SECTION ===
    settingsGui.SetFont("s8 c0x888888", "Segoe UI")
    settingsGui.Add("Text", "x30 y265 w380 Center", "⌨️ Hotkey: Ctrl+Shift+V  •  ESC to stop typing")
    
    ; === BUTTONS ===
    settingsGui.SetFont("s10 c0xFFFFFF Bold", "Segoe UI")
    
    ; Save Button (Primary - Green)
    saveBtn := settingsGui.Add("Button", "x30 y300 w120 h40", "✓ Save")
    saveBtn.OnEvent("Click", (*) => SaveSettings(minWpmEdit, maxWpmEdit, typoEdit))
    
    ; Reset Button (Secondary - Orange)
    resetBtn := settingsGui.Add("Button", "x165 y300 w120 h40", "↺ Reset")
    resetBtn.OnEvent("Click", (*) => ResetDefaults(minWpmEdit, maxWpmEdit, typoEdit))
    
    ; Close Button (Tertiary)
    closeBtn := settingsGui.Add("Button", "x300 y300 w110 h40", "✕ Close")
    closeBtn.OnEvent("Click", (*) => settingsGui.Destroy())
    
    settingsGui.OnEvent("Close", (*) => settingsGui.Destroy())
    settingsGui.Show("w440 h360")
}

SaveSettings(minEdit, maxEdit, typoEdit) {
    global minWPM, maxWPM, correctionChance, settingsGui
    
    newMin := Integer(minEdit.Value)
    newMax := Integer(maxEdit.Value)
    newTypo := Float(typoEdit.Value)
    
    ; Validate
    if (newMin < 10)
        newMin := 10
    if (newMax > 260)
        newMax := 260
    if (newMin > newMax) {
        temp := newMin
        newMin := newMax
        newMax := temp
    }
    if (newTypo < 0)
        newTypo := 0
    if (newTypo > 0.30)
        newTypo := 0.30
    
    minWPM := newMin
    maxWPM := newMax
    correctionChance := newTypo
    
    ToolTip("Settings saved!")
    SetTimer(() => ToolTip(), -1500)
    
    if (settingsGui)
        settingsGui.Destroy()
}

ResetDefaults(minEdit, maxEdit, typoEdit) {
    global DEFAULT_MIN_WPM, DEFAULT_MAX_WPM, DEFAULT_TYPO_RATE
    
    minEdit.Value := DEFAULT_MIN_WPM
    maxEdit.Value := DEFAULT_MAX_WPM
    typoEdit.Value := Format("{:.2f}", DEFAULT_TYPO_RATE)
    
    ToolTip("Reset to defaults")
    SetTimer(() => ToolTip(), -1500)
}

; === Hotkeys ===
^+v:: {  ; Ctrl+Shift+V
    clip := A_Clipboard
    if (clip)
        TypeHuman(clip)
}

Esc:: {  ; ESC to cancel typing (removed tilde for more reliable cancellation)
    global typingInProgress
    if (typingInProgress) {
        StopTyping()
        ; Prevent ESC from being passed to the application when canceling
        return
    }
    ; If not typing, allow ESC to pass through normally
    Send("{Esc}")
}

