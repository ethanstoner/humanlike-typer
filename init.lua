-- === HumanLike Typer ===
-- Realistic human typing emulator with natural patterns
-- 
-- Copyright (c) 2025 HumanLike Typer Contributors
-- Licensed under CC BY-NC-SA 4.0
-- https://creativecommons.org/licenses/by-nc-sa/4.0/
-- 
-- Free to use, modify, and share for non-commercial purposes.
-- Cannot be sold or used commercially.
-- 
-- Menubar: ○ when idle, ● while typing
-- Click icon for menu: Type Clipboard, Settings, Reload
-- Hotkey: Ctrl+Alt+Cmd+V → type clipboard immediately
-- Esc → cancel typing

local DEFAULT_MIN_WPM, DEFAULT_MAX_WPM = 90, 130
local DEFAULT_TYPO_RATE = 0.05
local DEFAULT_SPACE_PAUSE = 0.08

-- Ensure webview framework is initialized (for all setups)
local webview = require("hs.webview")

local minWPM, maxWPM = DEFAULT_MIN_WPM, DEFAULT_MAX_WPM
local correctionChance = DEFAULT_TYPO_RATE
local wordPauseChance = DEFAULT_SPACE_PAUSE

-- Subtle alerts (kept small and out of the way)
hs.alert.defaultStyle.textSize = 14
hs.alert.defaultStyle.fillColor = {white=0.08, alpha=0.88}
hs.alert.defaultStyle.strokeColor = {white=1, alpha=0.08}
hs.alert.defaultStyle.textColor = {white=1, alpha=0.95}
hs.alert.defaultStyle.atScreenEdge = 3
hs.alert.defaultStyle.radius = 6
hs.alert.defaultStyle.fadeInDuration = 0.05
hs.alert.defaultStyle.fadeOutDuration = 0.10
hs.alert.defaultStyle.textFont = "Menlo"

-- Key press helpers (US layout)

local shifted = {
["!"]={"1","shift"}, ["@"]={"2","shift"}, ["#"]={"3","shift"}, ["$"]={"4","shift"},
["%"]={"5","shift"}, ["^"]={"6","shift"}, ["&"]={"7","shift"}, ["*"]={"8","shift"},
["("]={"9","shift"}, [")"]={"0","shift"}, ["_"]={"-","shift"}, ["+"]={"=","shift"},
["{"]={"[","shift"}, ["}"]={"]","shift"}, ["|"]={"\\","shift"}, [":"]={";","shift"},
["\""]={"'","shift"}, ["<"]={",","shift"}, [">"]={".","shift"}, ["?"]={"/","shift"}
}

local plain = {
[" "]="space", ["\t"]="tab", ["\n"]="return", ["\r"]="return",
[","]=",", ["."]=".", [";"]=";", ["'"]="'", ["-"]="-", ["="]="=",
["["]="[", ["]"]="]", ["\\"]="\\" , ["/"]="/"
}

local function pressKeyChar(c)
if c:match("%l") then
hs.eventtap.keyStroke({}, c, 0) ; return true
elseif c:match("%u") then
hs.eventtap.keyStroke({"shift"}, c:lower(), 0) ; return true
end
if c:match("%d") then
hs.eventtap.keyStroke({}, c, 0) ; return true
end
if plain[c] then
hs.eventtap.keyStroke({}, plain[c], 0) ; return true
end
if shifted[c] then
local key, mod = shifted[c][1], shifted[c][2]
hs.eventtap.keyStroke({mod}, key, 0) ; return true
end
return false
end

local function typeChar(c)
local usedReal = pressKeyChar(c)
if not usedReal then
hs.eventtap.keyStrokes(c)
end
end

-- === Text sanitization: allow only standard ASCII characters ===
local function sanitizeText(s)
  if not s then return "" end
  -- Common smart punctuation → ASCII
  -- ’    “ ”    – —    …     NBSP
  s = s:gsub("’", "'")
       :gsub("“", '"'):gsub("”", '"')
       :gsub("–", "-"):gsub("—", "--")
       :gsub("…", "...")
       :gsub("\194\160", " ") -- NBSP
  -- Remove all remaining non-ASCII (keep newline and tab)
  -- Strip control chars except \n and \t
  s = s:gsub("[\000-\008\011\012\014-\031]", "")
  -- Strip any rune outside printable ASCII range
  s = s:gsub("[^\n\t\032-\126]", "")
  return s
end

-- === Human-like typo helpers ===
local NEIGHBORS = {
a={"q","w","s","z"}, b={"v","g","h","n"}, c={"x","d","f","v"},
d={"s","e","r","f","c","x"}, e={"w","s","d","r"}, f={"d","r","t","g","v","c"},
g={"f","t","y","h","b","v"}, h={"g","y","u","j","n","b"}, i={"u","j","k","o"},
j={"h","u","i","k","m","n"}, k={"j","i","o","l","m"}, l={"k","o","p"},
m={"n","j","k"}, n={"b","h","j","m"}, o={"i","k","l","p"}, p={"o","l"},
q={"w","a"}, r={"e","d","f","t"}, s={"a","w","e","d","x","z"}, t={"r","f","g","y"},
u={"y","h","j","i"}, v={"c","f","g","b"}, w={"q","a","s","e"}, x={"z","s","d","c"},
y={"t","g","h","u"}, z={"a","s","x"},
["1"]={"2","q"},["2"]={"1","3","q","w"},["3"]={"2","4","w","e"},["4"]={"3","5","e","r"},
["5"]={"4","6","r","t"},["6"]={"5","7","t","y"},["7"]={"6","8","y","u"},
["8"]={"7","9","u","i"},["9"]={"8","0","i","o"},["0"]={"9","o","p"}
}

local function randomNeighbor(ch)
local lower = ch:lower()
local opts = NEIGHBORS[lower]
if not opts or #opts == 0 then return nil end
local pick = opts[math.random(1,#opts)]
-- preserve case for letters
if ch:match("%u") then pick = pick:upper() end
return pick
end

-- returns true if we executed a typo (and handled correction), false otherwise
local function maybeDoTypo(curr, nextch)
if not curr:match("[%a%d]") then return false end
if math.random() >= correctionChance then return false end

local doTransposition = (nextch and nextch:match("%a") and math.random() < 0.4)

if doTransposition then
-- Type next first, then correct with a backspace, then type curr
typeChar(nextch)
hs.timer.usleep(50 * 1000)
hs.eventtap.keyStroke({}, "delete", 0)   -- erase the wrong order
hs.timer.usleep(20 * 1000)
typeChar(curr)
return true
else
-- Adjacent-key mis-hit, then backspace and correct
local neigh = randomNeighbor(curr)
if neigh then
typeChar(neigh)
hs.timer.usleep(60 * 1000)
hs.eventtap.keyStroke({}, "delete", 0)
hs.timer.usleep(20 * 1000)
typeChar(curr)
return true
end
end

return false
end

-- Human typing engine (non-blocking)

local typingTimer = nil
local typingInProgress = false
local mbar

local function stopTyping()
if typingTimer then typingTimer:stop(); typingTimer = nil end
typingInProgress = false
if mbar then mbar:setTitle("○") end
end

function typeHuman(text)
if typingInProgress or not text or #text == 0 then return end
typingInProgress = true
text = sanitizeText(text)
if mbar then mbar:setTitle("●") end
math.randomseed(os.time())
local i, prev = 1, ""
local function step()
if i > #text then stopTyping(); return end
local c = text:sub(i,i)
local wpm = math.random(minWPM, maxWPM)
local cps = (wpm * 5) / 60
local delay = (1 / cps) * (math.random(90,110)/100)
if c == " " and math.random() < wordPauseChance then delay = delay + (0.06 * math.random(70,130)/100) end
if c == "." or c == "!" or c == "?" then delay = delay + 0.08 end
local nextch = (i < #text) and text:sub(i+1,i+1) or nil
if not maybeDoTypo(c, nextch) then
typeChar(c)
end
prev = c
i = i + 1
typingTimer = hs.timer.doAfter(delay, step)
end
step()
end

-- Settings panel functions

local settingsWV, ucc

local function applySettings(newMin, newMax, newTypo)
newMin = tonumber(newMin) or minWPM
newMax = tonumber(newMax) or maxWPM
newTypo = tonumber(newTypo) or correctionChance
if newMin < 10 then newMin = 10 end
if newMax > 260 then newMax = 260 end
if newMin > newMax then newMin, newMax = newMax, newMin end
if newTypo < 0 then newTypo = 0 end
if newTypo > 0.30 then newTypo = 0.30 end
minWPM, maxWPM, correctionChance = math.floor(newMin), math.floor(newMax), newTypo
end

local function resetDefaults()
minWPM, maxWPM = DEFAULT_MIN_WPM, DEFAULT_MAX_WPM
correctionChance = DEFAULT_TYPO_RATE
wordPauseChance = DEFAULT_SPACE_PAUSE
end

local function settingsHTML()
return string.format([[
  <html><head><meta charset="utf-8">
  <style>
    body{font-family:-apple-system,Helvetica,Arial;background:#111;color:#eee;margin:0}
    .wrap{padding:14px 16px;width:420px}
    h1{font-size:16px;margin:0 0 10px}
    label{display:block;font-size:12px;color:#bbb;margin-top:10px}
    input{width:100%%;padding:6px 8px;margin-top:4px;border-radius:6px;border:1px solid #333;background:#1a1a1a;color:#eee}
    .row{display:flex;gap:10px}.row>div{flex:1}
    .btns{display:flex;gap:10px;margin-top:14px}
    button{flex:1;padding:8px 10px;border-radius:8px;border:1px solid #3a3a3a;background:#222;color:#eee;cursor:pointer}
    button.primary{background:#2a5;border-color:#2a5;color:#021;font-weight:600}
    button.reset{background:#933;border-color:#a44}
    small{color:#888;display:block;margin-top:8px}
  </style></head>
  <body><div class="wrap">
    <h1>HumanLike Typer — Settings</h1>
    <div class="row">
      <div><label>Minimum WPM</label><input id="min" type="number" min="10" max="260" value="%d"></div>
      <div><label>Maximum WPM</label><input id="max" type="number" min="10" max="260" value="%d"></div>
    </div>
    <label>Typo Rate (0.00–0.30)</label>
    <input id="typo" type="number" step="0.01" min="0" max="0.30" value="%.2f">
    <div class="btns">
      <button class="primary" onclick="apply()">Save</button>
      <button class="reset" onclick="resetDefaults()">Reset Defaults</button>
      <button onclick="closeWin()">Close</button>
    </div>
    <small>Menu: ○/● icon → Type Clipboard • Settings • Reload</small>
  </div>
  <script>
    function send(o){ window.webkit.messageHandlers.cfg.postMessage(JSON.stringify(o)); }
    function apply(){ send({action:"apply", min:document.getElementById('min').value, max:document.getElementById('max').value, typo:document.getElementById('typo').value}); }
    function resetDefaults(){ send({action:"reset"}); }
    function closeWin(){ send({action:"close"}); }
  </script>
  </body></html>]], minWPM, maxWPM, correctionChance)
end

ucc = hs.webview.usercontent.new("cfg")
ucc:setCallback(function(_, messageName, userData)
  -- Some Hammerspoon builds send (name, body); others send only (body).
  -- Normalize to a JSON string payload.
  local payload = userData
  if type(payload) ~= "string" then
    payload = messageName
  end
  local ok, data = pcall(function() return hs.json.decode(payload) end)
  if not ok or type(data) ~= "table" then return end

  if data.action == "apply" then
    applySettings(data.min, data.max, data.typo)
  elseif data.action == "reset" then
    resetDefaults()
  elseif data.action == "close" then
    if settingsWV then settingsWV:delete(); settingsWV = nil end
    return
  end

  if settingsWV then settingsWV:html(settingsHTML()) end
end)

function showSettings()
  hs.printf("[HT] showSettings() called")
  
  -- Close existing window
  if settingsWV then 
    hs.printf("[HT] Closing existing settings window")
    settingsWV:delete()
    settingsWV = nil
  end
  
  -- Get screen dimensions
  local scr = hs.screen.mainScreen():frame()
  local w, h = 460, 260
  local x = scr.x + (scr.w - w) / 2
  local y = scr.y + (scr.h - h) / 2
  
  hs.printf("[HT] Creating settings window at x=%d, y=%d, w=%d, h=%d", x, y, w, h)
  
  -- Try to create the webview
  local ok, result = pcall(function()
    settingsWV = hs.webview.new({x=x, y=y, w=w, h=h})
    settingsWV:windowStyle({"titled", "closable"})
    settingsWV:userContentController(ucc)
    settingsWV:allowTextEntry(true)
    settingsWV:windowTitle("HumanLike Typer — Settings")
    settingsWV:html(settingsHTML())
    settingsWV:windowCallback(function(action)
      hs.printf("[HT] Window action: %s", action)
      if action == "closing" then
        if settingsWV then 
          settingsWV:delete()
          settingsWV = nil 
        end
      end
    end)
    settingsWV:show()
    settingsWV:bringToFront(true)
  end)
  
  if not ok then
    hs.printf("[HT] ERROR creating settings window: %s", tostring(result))
    hs.alert.show("Settings window failed to open. Check Console.")
  else
    hs.printf("[HT] Settings window created successfully")
  end
end

-- Menubar icon + menu

mbar = hs.menubar.new()
mbar:setTitle("○")

local menuItems = {
  { title = "Type Clipboard", fn = function()
    hs.printf("[HT] Menu: Type Clipboard clicked")
    local clip = hs.pasteboard.getContents() or ""
    clip = sanitizeText(clip)
    if #clip > 0 then typeHuman(clip) end
  end },
  { title = "-" },
  { title = "Settings…", fn = function() 
    hs.printf("[HT] Menu: Settings clicked")
    showSettings() 
  end },
  { title = "-" },
  { title = "Reload Config", fn = function() 
    hs.printf("[HT] Menu: Reload clicked")
    hs.reload() 
  end }
}

mbar:setMenu(menuItems)

-- Hotkeys

hs.hotkey.bind({"ctrl","alt","cmd"}, "V", function()
local clip = hs.pasteboard.getContents() or ""
clip = sanitizeText(clip)
if #clip == 0 then return end
typeHuman(clip)
end)

hs.hotkey.bind({}, "escape", function() stopTyping() end)