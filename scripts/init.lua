------------------------------------------------------------------------------------------------
-- === HumanLike Typer ===
------------------------------------------------------------------------------------------------
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
local burstWPM, burstCharsLeft = nil, 0
local nextAllowedTypoAt = 0
local SCRIPT_UPDATE_URL = "https://raw.githubusercontent.com/ethanstoner/humanlike-typer/main/scripts/init.lua"
local typingTimer, typingInProgress, mbar = nil, false, nil

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

local function currentBurstWPM()
if not burstWPM or burstCharsLeft <= 0 then
burstWPM = math.random(minWPM, maxWPM)
burstCharsLeft = math.random(4, 9)
end

burstCharsLeft = burstCharsLeft - 1
return burstWPM
end

local function charDelay(curr, prev, nextch, speedScale)
local wpm = currentBurstWPM()
local cps = (wpm * 5) / 60
local delay = (1 / cps) * (math.random(82, 118) / 100) * (speedScale or 1)

if curr == " " then
if math.random() < wordPauseChance then
delay = delay + ((math.random(40, 140)) / 1000)
end
elseif curr == "," or curr == ";" or curr == ":" then
delay = delay + ((math.random(45, 120)) / 1000)
elseif curr == "." or curr == "!" or curr == "?" then
delay = delay + ((math.random(110, 280)) / 1000)
elseif prev == " " and curr:match("%a") and math.random() < 0.18 then
delay = delay + ((math.random(25, 80)) / 1000)
end

return delay
end

local function sendChunkHuman(chunk, prev, speedScale)
for offset = 1, #chunk do
if not typingInProgress then return end
local curr = chunk:sub(offset, offset)
local nextch = (offset < #chunk) and chunk:sub(offset + 1, offset + 1) or nil
typeChar(curr)
if offset < #chunk then
hs.timer.usleep(charDelay(curr, prev, nextch, speedScale) * 1000 * 1000)
end
prev = curr
end
end

local function wordEndIndex(text, startIndex)
local finish = startIndex
while finish <= #text and text:sub(finish, finish):match("%a") do
finish = finish + 1
end
return finish - 1
end

local function buildTypoPlan(text, index)
local curr = text:sub(index, index)
local prev = (index > 1) and text:sub(index - 1, index - 1) or ""
local nextch = (index < #text) and text:sub(index + 1, index + 1) or ""

if not curr:match("%a") or not prev:match("%a") or not nextch:match("%a") then
return nil
end

local wordEnd = wordEndIndex(text, index)
local remaining = wordEnd - index + 1
if remaining < 2 then return nil end

local carry = math.min(math.random(1, 3), remaining - 1)
local doTransposition = remaining >= 2 and math.random() < 0.35

if doTransposition then
local chunkLen = math.min(2 + carry, remaining)
local correct = text:sub(index, index + chunkLen - 1)
local typed = text:sub(index + 1, index + 1) .. curr
if chunkLen > 2 then
typed = typed .. text:sub(index + 2, index + chunkLen - 1)
end
return {
typed = typed,
correct = correct,
advance = chunkLen,
notice = (math.random(120, 420)) / 1000,
backspace = (math.random(40, 85)) / 1000,
}
end

local neigh = randomNeighbor(curr)
if not neigh then return nil end

local chunkLen = math.min(1 + carry, remaining)
local correct = text:sub(index, index + chunkLen - 1)
local typed = neigh
if chunkLen > 1 then
typed = typed .. text:sub(index + 1, index + chunkLen - 1)
end

return {
typed = typed,
correct = correct,
advance = chunkLen,
notice = (math.random(140, 460)) / 1000,
backspace = (math.random(45, 95)) / 1000,
}
end

local function maybeDoTypo(text, index, prev)
if index < nextAllowedTypoAt then return 0 end
if math.random() >= correctionChance then return 0 end
local plan = buildTypoPlan(text, index)
if not plan then return 0 end

sendChunkHuman(plan.typed, prev, 0.96)
if not typingInProgress then return 0 end

hs.timer.usleep(plan.notice * 1000 * 1000)
for _ = 1, #plan.typed do
if not typingInProgress then return 0 end
hs.eventtap.keyStroke({}, "delete", 0)
hs.timer.usleep(plan.backspace * 1000 * 1000)
end

sendChunkHuman(plan.correct, prev, 1.08)
nextAllowedTypoAt = index + math.random(9, 16)
return plan.advance
end

-- Human typing engine (non-blocking)

typingTimer = nil
typingInProgress = false
mbar = nil

local function stopTyping()
if typingTimer then typingTimer:stop(); typingTimer = nil end
typingInProgress = false
if mbar then mbar:setTitle("○") end
end

local function updateThisScript()
local configPath = hs.configdir .. "/init.lua"
local backupPath = hs.configdir .. "/init.lua.bak"

hs.http.asyncGet(SCRIPT_UPDATE_URL, nil, function(status, body)
if status ~= 200 or not body or #body == 0 then
hs.alert.show("Update failed")
return
end

local ok, err = pcall(function()
local existing = io.open(configPath, "r")
if existing then
local current = existing:read("*a")
existing:close()
local backup = assert(io.open(backupPath, "w"))
backup:write(current)
backup:close()
end

local file = assert(io.open(configPath, "w"))
file:write(body)
file:close()
end)

if not ok then
hs.alert.show("Update failed")
hs.printf("[HT] Update failed: %s", tostring(err))
return
end

hs.alert.show("Updated. Reloading...")
hs.reload()
end)
end

function typeHuman(text)
if typingInProgress or not text or #text == 0 then return end
typingInProgress = true
text = sanitizeText(text)
if mbar then mbar:setTitle("●") end
math.randomseed(os.time())
burstWPM, burstCharsLeft = nil, 0
nextAllowedTypoAt = 0
local i, prev = 1, ""
local function step()
if i > #text then stopTyping(); return end
local c = text:sub(i,i)
local nextch = (i < #text) and text:sub(i+1,i+1) or nil
local delay = charDelay(c, prev, nextch, 1.0)
local consumed = maybeDoTypo(text, i, prev)
if consumed > 0 then
prev = text:sub(i + consumed - 1, i + consumed - 1)
i = i + consumed
else
typeChar(c)
prev = c
i = i + 1
end
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
  { title = "Update Script", fn = function()
    hs.printf("[HT] Menu: Update clicked")
    updateThisScript()
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
