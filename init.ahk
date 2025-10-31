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

;@Ahk2Exe-SetMainIcon humantyperlogo.ico
;@Ahk2Exe-SetCompanyName HumanLike Typer Contributors
;@Ahk2Exe-SetCopyright Copyright (c) 2025 - CC BY-NC-SA 4.0
;@Ahk2Exe-SetDescription Realistic human typing emulator
;@Ahk2Exe-SetVersion 1.1.1

#Requires AutoHotkey v2.0
#SingleInstance Force

; === Default Settings ===
global DEFAULT_MIN_WPM := 90
global DEFAULT_MAX_WPM := 130
global DEFAULT_TYPO_RATE := 0.05
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
    
    neighbors := NEIGHBORS[lower]
    if (neighbors.Length = 0)
        return ""
    
    pick := neighbors[Random(1, neighbors.Length)]
    
    ; Preserve case for letters
    if (RegExMatch(char, "[A-Z]"))
        pick := StrUpper(pick)
    
    return pick
}

; === Typo Simulation ===
MaybeDoTypo(curr, nextChar, &skipNext) {
    global correctionChance, typingInProgress
    skipNext := false
    
    ; Check if still typing
    if (!typingInProgress)
        return false
    
    ; Only typo on alphanumeric characters
    if (!RegExMatch(curr, "[a-zA-Z0-9]"))
        return false
    
    ; Check if we should make a typo
    if (Random(0.0, 1.0) >= correctionChance)
        return false
    
    ; 40% transpositions if next char is a letter
    doTransposition := (nextChar && RegExMatch(nextChar, "[a-zA-Z]") && Random(0.0, 1.0) < 0.4)
    
    if (doTransposition) {
        ; Type next char first, pause, then backspace and correct
        SendText(nextChar)
        Sleep(120)
        Send("{BS}")  ; Backspace
        Sleep(50)
        SendText(curr)
        skipNext := true  ; We already typed the next char
        return true
    } else {
        ; Adjacent-key mistake
        neigh := RandomNeighbor(curr)
        if (neigh != "") {
            SendText(neigh)
            Sleep(150)
            Send("{BS}")  ; Backspace
            Sleep(50)
            SendText(curr)
            return true
        }
    }
    
    return false
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
    
    ; Check for typo
    skipNext := false
    if (!MaybeDoTypo(c, nextChar, &skipNext)) {
        SendText(c)
    }
    
    if (skipNext)
        currentIndex += 2
    else
        currentIndex += 1
    
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

~Esc:: {  ; Tilde allows ESC to pass through naturally
    global typingInProgress
    if (typingInProgress) {
        StopTyping()
    }
}

