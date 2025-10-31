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
global typingTimer := ""
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
    
    ; Convert smart punctuation to ASCII
    text := StrReplace(text, "'", "'")  ; Smart apostrophe
    text := StrReplace(text, "'", "'")  ; Smart single quotes
    text := StrReplace(text, """, '"')  ; Smart double quote left
    text := StrReplace(text, """, '"')  ; Smart double quote right
    text := StrReplace(text, "–", "-")  ; En-dash
    text := StrReplace(text, "—", "--") ; Em-dash
    text := StrReplace(text, "…", "...") ; Ellipsis
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
    skipNext := false
    
    ; Only typo on alphanumeric characters
    if (!RegExMatch(curr, "[a-zA-Z0-9]"))
        return false
    
    ; Check if we should make a typo
    if (Random(0.0, 1.0) >= correctionChance)
        return false
    
    ; 40% transpositions if next char is a letter
    doTransposition := (nextChar && RegExMatch(nextChar, "[a-zA-Z]") && Random(0.0, 1.0) < 0.4)
    
    if (doTransposition) {
        ; Type next char first, then backspace and correct
        SendText(nextChar)
        Sleep(50)
        Send("{Backspace}")
        Sleep(20)
        SendText(curr)
        skipNext := true  ; We already typed the next char
        return true
    } else {
        ; Adjacent-key mistake
        neigh := RandomNeighbor(curr)
        if (neigh != "") {
            SendText(neigh)
            Sleep(60)
            Send("{Backspace}")
            Sleep(20)
            SendText(curr)
            return true
        }
    }
    
    return false
}

; === Main Typing Function ===
TypeHuman(text) {
    global typingInProgress, typingTimer
    
    if (typingInProgress || !text || StrLen(text) = 0)
        return
    
    text := SanitizeText(text)
    if (StrLen(text) = 0)
        return
    
    typingInProgress := true
    TrayTyping()
    
    ; Use a closure to maintain state
    i := 1
    textLen := StrLen(text)
    
    TypeStep() {
        global typingInProgress, minWPM, maxWPM, wordPauseChance
        
        if (i > textLen) {
            StopTyping()
            return
        }
        
        c := SubStr(text, i, 1)
        nextChar := (i < textLen) ? SubStr(text, i + 1, 1) : ""
        
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
            i += 2
        else
            i += 1
        
        ; Schedule next character
        SetTimer(TypeStep, Integer(delay))
    }
    
    TypeStep()
}

; === Stop Typing ===
StopTyping() {
    global typingInProgress
    typingInProgress := false
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
    
    ; Create new GUI
    settingsGui := Gui("+AlwaysOnTop", "HumanLike Typer - Settings")
    settingsGui.SetFont("s10", "Segoe UI")
    settingsGui.BackColor := "0x1a1a1a"
    
    ; Min WPM
    settingsGui.SetFont("s9 c0xbbbbbb", "Segoe UI")
    settingsGui.Add("Text", "x20 y20 w150", "Minimum WPM (10-260):")
    settingsGui.SetFont("s10 c0xeeeeee", "Segoe UI")
    minWpmEdit := settingsGui.Add("Edit", "x20 y45 w150 Number", minWPM)
    
    ; Max WPM
    settingsGui.SetFont("s9 c0xbbbbbb", "Segoe UI")
    settingsGui.Add("Text", "x190 y20 w150", "Maximum WPM (10-260):")
    settingsGui.SetFont("s10 c0xeeeeee", "Segoe UI")
    maxWpmEdit := settingsGui.Add("Edit", "x190 y45 w150 Number", maxWPM)
    
    ; Typo Rate
    settingsGui.SetFont("s9 c0xbbbbbb", "Segoe UI")
    settingsGui.Add("Text", "x20 y85 w320", "Typo Rate (0.00 - 0.30):")
    settingsGui.SetFont("s10 c0xeeeeee", "Segoe UI")
    typoEdit := settingsGui.Add("Edit", "x20 y110 w320", Format("{:.2f}", correctionChance))
    
    ; Buttons
    saveBtn := settingsGui.Add("Button", "x20 y150 w100 h35 Default", "Save")
    saveBtn.OnEvent("Click", (*) => SaveSettings(minWpmEdit, maxWpmEdit, typoEdit))
    
    resetBtn := settingsGui.Add("Button", "x130 y150 w100 h35", "Reset Defaults")
    resetBtn.OnEvent("Click", (*) => ResetDefaults(minWpmEdit, maxWpmEdit, typoEdit))
    
    closeBtn := settingsGui.Add("Button", "x240 y150 w100 h35", "Close")
    closeBtn.OnEvent("Click", (*) => settingsGui.Destroy())
    
    ; Info text
    settingsGui.SetFont("s8 c0x888888", "Segoe UI")
    settingsGui.Add("Text", "x20 y195 w320", "Hotkey: Ctrl+Shift+V to type clipboard`nESC to stop typing")
    
    settingsGui.OnEvent("Close", (*) => settingsGui.Destroy())
    settingsGui.Show("w360 h230")
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

Esc:: {
    if (typingInProgress)
        StopTyping()
    else
        Send("{Esc}")  ; Pass through if not typing
}

