using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HumanType.Native;

public sealed class TypingEngine
{
    private readonly Random random = new();
    private CancellationTokenSource? activeRun;
    private bool pauseRequested;
    private TypingSession? pausedSession;
    private int burstWpm;
    private int burstCharsLeft;
    private int nextAllowedTypoAt;
    private int postErrorSlowCharsLeft;
    private int resumeSlowCharsLeft;

    private static readonly Dictionary<char, char[]> Neighbors = new()
    {
        ['a'] = ['q', 'w', 's', 'z'],
        ['b'] = ['v', 'g', 'h', 'n'],
        ['c'] = ['x', 'd', 'f', 'v'],
        ['d'] = ['s', 'e', 'r', 'f', 'c', 'x'],
        ['e'] = ['w', 's', 'd', 'r'],
        ['f'] = ['d', 'r', 't', 'g', 'v', 'c'],
        ['g'] = ['f', 't', 'y', 'h', 'b', 'v'],
        ['h'] = ['g', 'y', 'u', 'j', 'n', 'b'],
        ['i'] = ['u', 'j', 'k', 'o'],
        ['j'] = ['h', 'u', 'i', 'k', 'm', 'n'],
        ['k'] = ['j', 'i', 'o', 'l', 'm'],
        ['l'] = ['k', 'o', 'p'],
        ['m'] = ['n', 'j', 'k'],
        ['n'] = ['b', 'h', 'j', 'm'],
        ['o'] = ['i', 'k', 'l', 'p'],
        ['p'] = ['o', 'l'],
        ['q'] = ['w', 'a'],
        ['r'] = ['e', 'd', 'f', 't'],
        ['s'] = ['a', 'w', 'e', 'd', 'x', 'z'],
        ['t'] = ['r', 'f', 'g', 'y'],
        ['u'] = ['y', 'h', 'j', 'i'],
        ['v'] = ['c', 'f', 'g', 'b'],
        ['w'] = ['q', 'a', 's', 'e'],
        ['x'] = ['z', 's', 'd', 'c'],
        ['y'] = ['t', 'g', 'h', 'u'],
        ['z'] = ['a', 's', 'x'],
        ['1'] = ['2', 'q'],
        ['2'] = ['1', '3', 'q', 'w'],
        ['3'] = ['2', '4', 'w', 'e'],
        ['4'] = ['3', '5', 'e', 'r'],
        ['5'] = ['4', '6', 'r', 't'],
        ['6'] = ['5', '7', 't', 'y'],
        ['7'] = ['6', '8', 'y', 'u'],
        ['8'] = ['7', '9', 'u', 'i'],
        ['9'] = ['8', '0', 'i', 'o'],
        ['0'] = ['9', 'o', 'p']
    };

    private static readonly Dictionary<char, KeyProfile> KeyProfiles = BuildKeyProfiles();

    public bool IsTyping => activeRun is not null;
    public bool IsPaused => pausedSession is not null;

    public Task StartTypingAsync(string text, AppSettings settings, IntPtr targetWindow, CancellationToken cancellationToken = default)
    {
        Stop();
        var sanitized = SanitizeText(text);
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return Task.CompletedTask;
        }

        var normalizedSettings = CloneSettings(settings);
        normalizedSettings.Normalize();
        activeRun = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        pauseRequested = false;
        pausedSession = null;
        burstWpm = 0;
        burstCharsLeft = 0;
        nextAllowedTypoAt = 0;
        postErrorSlowCharsLeft = 0;

        return RunTypingAsync(new TypingSession(sanitized, normalizedSettings, targetWindow, 0, null, 0, 0, 0, 0), activeRun.Token);
    }

    public Task ResumeTypingAsync(CancellationToken cancellationToken = default)
    {
        if (activeRun is not null || pausedSession is null)
        {
            return Task.CompletedTask;
        }

        var session = pausedSession.Value;
        pausedSession = null;
        pauseRequested = false;
        burstWpm = session.BurstWpm;
        burstCharsLeft = session.BurstCharsLeft;
        nextAllowedTypoAt = session.NextAllowedTypoAt;
        postErrorSlowCharsLeft = session.PostErrorSlowCharsLeft;
        resumeSlowCharsLeft = random.Next(5, 10);
        activeRun = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        return RunTypingAsync(session, activeRun.Token);
    }

    public bool Pause()
    {
        if (activeRun is null)
        {
            return false;
        }

        pauseRequested = true;
        return true;
    }

    public void UpdatePausedSettings(AppSettings settings)
    {
        if (pausedSession is null)
        {
            return;
        }

        var normalizedSettings = CloneSettings(settings);
        normalizedSettings.Normalize();
        var session = pausedSession.Value;
        pausedSession = session with { Settings = normalizedSettings };
    }

    public void Stop()
    {
        pauseRequested = false;
        pausedSession = null;
        activeRun?.Cancel();
        activeRun?.Dispose();
        activeRun = null;
    }

    public static string SanitizeText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text
            .Replace('\u2018', '\'')
            .Replace('\u2019', '\'')
            .Replace('\u201C', '"')
            .Replace('\u201D', '"')
            .Replace('\u2013', '-')
            .Replace("\u2014", "--")
            .Replace("\u2026", "...")
            .Replace('\u00A0', ' ');
    }

    private async Task RunTypingAsync(TypingSession session, CancellationToken cancellationToken)
    {
        var index = session.Index;
        var previous = session.Previous;

        try
        {
            await WaitForModifierReleaseAsync(cancellationToken);

            if (session.TargetWindow != IntPtr.Zero)
            {
                ActivateTargetWindow(session.TargetWindow);
                await Task.Delay(220, cancellationToken);
            }

            if (session.Index > 0)
            {
                await Task.Delay(random.Next(180, 320), cancellationToken);
            }

            for (; index < session.Text.Length; )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (pauseRequested)
                {
                    SavePausedSession(session, index, previous);
                    return;
                }

                var current = session.Text[index];

                if (TryBuildErrorPlan(session.Text, index, session.Settings, out var plan))
                {
                    await SendChunkAsync(plan.Typed, previous, session.Settings, 0.96, cancellationToken);
                    await Task.Delay(plan.NoticeDelayMs, cancellationToken);
                    await SendBackspacesAsync(plan.Typed.Length, plan.BackspaceDelayMs, cancellationToken);
                    await SendChunkAsync(plan.Correct, previous, session.Settings, 1.08, cancellationToken);
                    previous = plan.Correct[^1];
                    index += plan.Advance;
                    nextAllowedTypoAt = index + random.Next(9, 17);
                    postErrorSlowCharsLeft = random.Next(2, 5);
                    continue;
                }

                SendCharacter(current);
                previous = current;
                index++;

                if (index < session.Text.Length)
                {
                    await Task.Delay(CharDelay(current, index > 1 ? session.Text[index - 2] : null, session.Text[index], session.Settings, 1.0), cancellationToken);
                }
            }
        }
        catch (OperationCanceledException) when (pauseRequested)
        {
            SavePausedSession(session, index, previous);
        }
        finally
        {
            if (activeRun is not null)
            {
                activeRun.Dispose();
                activeRun = null;
            }
        }
    }

    private void SavePausedSession(TypingSession session, int index, char? previous)
    {
        pausedSession = new TypingSession(
            session.Text,
            CloneSettings(session.Settings),
            session.TargetWindow,
            index,
            previous,
            burstWpm,
            burstCharsLeft,
            nextAllowedTypoAt,
            postErrorSlowCharsLeft);
        pauseRequested = false;
    }

    private static void ActivateTargetWindow(IntPtr targetWindow)
    {
        var foregroundWindow = NativeMethods.GetForegroundWindow();
        var currentThreadId = NativeMethods.GetCurrentThreadId();
        var targetThreadId = NativeMethods.GetWindowThreadProcessId(targetWindow, IntPtr.Zero);
        var foregroundThreadId = foregroundWindow == IntPtr.Zero
            ? 0
            : NativeMethods.GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);

        try
        {
            if (foregroundThreadId != 0)
            {
                NativeMethods.AttachThreadInput(currentThreadId, foregroundThreadId, true);
            }

            if (targetThreadId != 0 && targetThreadId != currentThreadId)
            {
                NativeMethods.AttachThreadInput(currentThreadId, targetThreadId, true);
            }

            if (NativeMethods.IsIconic(targetWindow))
            {
                NativeMethods.ShowWindow(targetWindow, NativeMethods.SwRestore);
            }

            NativeMethods.BringWindowToTop(targetWindow);
            NativeMethods.SetForegroundWindow(targetWindow);
            NativeMethods.SetActiveWindow(targetWindow);
            NativeMethods.SetFocus(targetWindow);
        }
        finally
        {
            if (foregroundThreadId != 0)
            {
                NativeMethods.AttachThreadInput(currentThreadId, foregroundThreadId, false);
            }

            if (targetThreadId != 0 && targetThreadId != currentThreadId)
            {
                NativeMethods.AttachThreadInput(currentThreadId, targetThreadId, false);
            }
        }
    }

    private async Task SendChunkAsync(string chunk, char? previous, AppSettings settings, double speedScale, CancellationToken cancellationToken)
    {
        for (var i = 0; i < chunk.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SendCharacter(chunk[i]);

            if (i < chunk.Length - 1)
            {
                var next = chunk[i + 1];
                await Task.Delay(CharDelay(chunk[i], previous, next, settings, speedScale), cancellationToken);
            }

            previous = chunk[i];
        }
    }

    private async Task SendBackspacesAsync(int count, int delayMs, CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SendVirtualKey(NativeMethods.VkBack);
            await Task.Delay(delayMs, cancellationToken);
        }
    }

    private bool TryBuildErrorPlan(string text, int index, AppSettings settings, out TypoPlan plan)
    {
        plan = default;

        if (index < nextAllowedTypoAt || random.NextDouble() >= settings.TypoRate)
        {
            return false;
        }

        if (index <= 0 || index >= text.Length - 1)
        {
            return false;
        }

        var current = text[index];
        var previous = text[index - 1];
        var next = text[index + 1];

        if (!char.IsLetter(current) || !char.IsLetter(previous) || !char.IsLetter(next))
        {
            return false;
        }

        var wordEnd = index;
        while (wordEnd < text.Length && char.IsLetter(text[wordEnd]))
        {
            wordEnd++;
        }

        var remaining = wordEnd - index;
        if (remaining < 2)
        {
            return false;
        }

        var carry = Math.Min(random.Next(1, 4), remaining - 1);
        var weightedModes = new List<string>();
        AddWeightedMode(weightedModes, "substitution", 9);
        AddWeightedMode(weightedModes, "transposition", remaining >= 2 ? 5 : 0);
        AddWeightedMode(weightedModes, "insertion", 3);
        AddWeightedMode(weightedModes, "omission", remaining >= 3 ? 3 : 0);

        if (weightedModes.Count == 0)
        {
            return false;
        }

        var mode = weightedModes[random.Next(weightedModes.Count)];

        if (mode == "transposition" && remaining >= 2)
        {
            var chunkLength = Math.Min(2 + carry, remaining);
            if (!PreferCrossHandTransposition(text, index))
            {
                chunkLength = Math.Min(2, chunkLength);
            }

            var correct = text.Substring(index, chunkLength);
            var typed = new StringBuilder()
                .Append(text[index + 1])
                .Append(current);

            if (chunkLength > 2)
            {
                typed.Append(text.Substring(index + 2, chunkLength - 2));
            }

            plan = new TypoPlan(typed.ToString(), correct, chunkLength, random.Next(120, 421), random.Next(40, 86));
            return true;
        }

        if (mode == "insertion")
        {
            var inserted = RandomNeighbor(current) ?? RandomNeighbor(next);
            if (inserted is null)
            {
                return false;
            }

            var chunkLength = Math.Min(1 + carry, remaining);
            var correct = text.Substring(index, chunkLength);
            var typed = new StringBuilder()
                .Append(inserted.Value)
                .Append(correct);

            plan = new TypoPlan(typed.ToString(), correct, chunkLength, random.Next(110, 240), random.Next(30, 70));
            return true;
        }

        if (mode == "omission" && remaining >= 3)
        {
            var chunkLength = Math.Min(2 + carry, remaining);
            var correct = text.Substring(index, chunkLength);
            var typed = correct.Remove(0, 1);
            if (typed.Length == 0)
            {
                return false;
            }

            plan = new TypoPlan(typed, correct, chunkLength, random.Next(170, 340), random.Next(35, 80));
            return true;
        }

        var neighbor = RandomNeighbor(current);
        if (neighbor is null)
        {
            return false;
        }

        var adjacentChunkLength = Math.Min(1 + carry, remaining);
        var adjacentCorrect = text.Substring(index, adjacentChunkLength);
        var adjacentTyped = new StringBuilder().Append(neighbor.Value);

        if (adjacentChunkLength > 1)
        {
            adjacentTyped.Append(text.Substring(index + 1, adjacentChunkLength - 1));
        }

        plan = new TypoPlan(adjacentTyped.ToString(), adjacentCorrect, adjacentChunkLength, random.Next(140, 461), random.Next(45, 96));
        return true;
    }

    private static void AddWeightedMode(List<string> weightedModes, string mode, int weight)
    {
        for (var i = 0; i < weight; i++)
        {
            weightedModes.Add(mode);
        }
    }

    private static bool PreferCrossHandTransposition(string text, int index)
    {
        if (index + 1 >= text.Length)
        {
            return false;
        }

        return TryGetKeyProfile(text[index], out var currentProfile) &&
               TryGetKeyProfile(text[index + 1], out var nextProfile) &&
               currentProfile.Hand != nextProfile.Hand;
    }

    private char? RandomNeighbor(char value)
    {
        var lower = char.ToLowerInvariant(value);
        if (!Neighbors.TryGetValue(lower, out var options) || options.Length == 0)
        {
            return null;
        }

        var picked = options[random.Next(options.Length)];
        return char.IsUpper(value) ? char.ToUpperInvariant(picked) : picked;
    }

    private int CurrentBurstWpm(AppSettings settings)
    {
        if (burstCharsLeft <= 0)
        {
            burstWpm = random.Next(settings.MinWpm, settings.MaxWpm + 1);
            burstCharsLeft = random.Next(4, 10);
        }

        burstCharsLeft--;
        return burstWpm;
    }

    private int CharDelay(char current, char? previous, char? next, AppSettings settings, double speedScale)
    {
        var wpm = CurrentBurstWpm(settings);
        var cps = (wpm * 6.4d) / 60d;
        var contextScale = Math.Clamp(155d / wpm, 0.42, 1.0);
        var delay = (1000d / cps) * (random.Next(88, 109) / 100d) * speedScale;

        if (previous is not null &&
            TryGetKeyProfile(previous.Value, out var previousProfile) &&
            TryGetKeyProfile(current, out var currentProfile))
        {
            if (previousProfile.Hand != currentProfile.Hand)
            {
                delay -= random.Next(18, 42) * 0.7;
            }
            else
            {
                delay += random.Next(10, 36) * contextScale;
            }

            if (previousProfile.Finger == currentProfile.Finger)
            {
                delay += random.Next(18, 44) * contextScale;
            }

            if (currentProfile.Row != 1)
            {
                delay += random.Next(8, 26) * contextScale;
            }

            if (currentProfile.Finger is Finger.LeftPinky or Finger.RightPinky)
            {
                delay += random.Next(12, 34) * contextScale;
            }
        }

        if (current == ' ')
        {
            if (settings.RandomPausesEnabled && random.NextDouble() < settings.PauseChance)
            {
                delay += random.Next(settings.PauseMinMs, settings.PauseMaxMs + 1) * contextScale;
            }
        }
        else if (",;:".Contains(current))
        {
            delay += random.Next(45, 121) * contextScale;
        }
        else if (".!?".Contains(current))
        {
            delay += random.Next(110, 281) * contextScale;
        }
        else if (previous == ' ' && char.IsLetter(current) && next is not null && random.NextDouble() < 0.18)
        {
            delay += random.Next(25, 81) * contextScale;
        }

        if ((previous is '.' or '!' or '?' or ':' or ';') && char.IsLetter(current))
        {
            delay += random.Next(70, 170) * contextScale;
        }

        if (previous == ' ' && char.IsLetter(current) && next is not null && char.IsLetter(next.Value) && random.NextDouble() < 0.10)
        {
            delay += random.Next(40, 110) * contextScale;
        }

        if (char.IsUpper(current) || char.IsDigit(current))
        {
            delay += random.Next(12, 30) * contextScale;
        }

        if ("\"'()[]{}".Contains(current))
        {
            delay += random.Next(18, 40) * contextScale;
        }

        if (postErrorSlowCharsLeft > 0)
        {
            delay += random.Next(35, 95) * Math.Max(contextScale, 0.55);
            postErrorSlowCharsLeft--;
        }

        if (resumeSlowCharsLeft > 0)
        {
            delay += random.Next(55, 130) * Math.Max(contextScale, 0.6);
            resumeSlowCharsLeft--;
        }

        return Math.Max(1, (int)Math.Round(delay));
    }

    private static void SendCharacter(char character)
    {
        if (character is '\r' or '\n')
        {
            SendKeys.SendWait("{ENTER}");
            return;
        }

        if (character == '\t')
        {
            SendKeys.SendWait("{TAB}");
            return;
        }

        SendKeys.SendWait(ToSendKeysText(character));
    }

    private static async Task WaitForModifierReleaseAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < 40; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsModifierDown(NativeMethods.VkControl) &&
                !IsModifierDown(NativeMethods.VkMenu) &&
                !IsModifierDown(NativeMethods.VkShift))
            {
                return;
            }

            await Task.Delay(25, cancellationToken);
        }
    }

    private static bool IsModifierDown(ushort keyCode)
    {
        return (NativeMethods.GetAsyncKeyState(keyCode) & 0x8000) != 0;
    }

    private static void SendInputs(NativeMethods.INPUT[] inputs, string description)
    {
        var sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException($"SendInput failed for {description} (Win32: {Marshal.GetLastWin32Error()}).");
        }
    }

    private static NativeMethods.INPUT CreateVirtualKeyInput(ushort keyCode, bool keyUp)
    {
        return new NativeMethods.INPUT
        {
            type = NativeMethods.InputKeyboard,
            U = new NativeMethods.InputUnion
            {
                ki = new NativeMethods.KEYBDINPUT
                {
                    wVk = keyCode,
                    dwFlags = keyUp ? NativeMethods.KeyEventKeyUp : 0
                }
            }
        };
    }

    private static void SendVirtualKey(ushort keyCode)
    {
        NativeMethods.keybd_event((byte)keyCode, 0, 0, IntPtr.Zero);
        Thread.Sleep(10);
        NativeMethods.keybd_event((byte)keyCode, 0, NativeMethods.KeyEventFKeyUp, IntPtr.Zero);
    }

    private static string ToSendKeysText(char character)
    {
        return character switch
        {
            '+' => "{+}",
            '^' => "{^}",
            '%' => "{%}",
            '~' => "{~}",
            '(' => "{(}",
            ')' => "{)}",
            '[' => "{[}",
            ']' => "{]}",
            '{' => "{{}",
            '}' => "{}}",
            _ => character.ToString()
        };
    }

    private static AppSettings CloneSettings(AppSettings settings)
    {
        return new AppSettings
        {
            MinWpm = settings.MinWpm,
            MaxWpm = settings.MaxWpm,
            TypoRate = settings.TypoRate,
            RandomPausesEnabled = settings.RandomPausesEnabled,
            PauseChance = settings.PauseChance,
            PauseMinMs = settings.PauseMinMs,
            PauseMaxMs = settings.PauseMaxMs,
            HotkeyModifiers = settings.HotkeyModifiers,
            HotkeyKey = settings.HotkeyKey
        };
    }

    private static Dictionary<char, KeyProfile> BuildKeyProfiles()
    {
        var profiles = new Dictionary<char, KeyProfile>();

        RegisterRow(profiles, "`1234567890-=", 0, new[]
        {
            Finger.LeftPinky, Finger.LeftPinky, Finger.LeftRing, Finger.LeftMiddle, Finger.LeftIndex,
            Finger.LeftIndex, Finger.RightIndex, Finger.RightIndex, Finger.RightMiddle, Finger.RightRing,
            Finger.RightPinky, Finger.RightPinky, Finger.RightPinky
        });
        RegisterRow(profiles, "qwertyuiop[]\\", 1, new[]
        {
            Finger.LeftPinky, Finger.LeftRing, Finger.LeftMiddle, Finger.LeftIndex, Finger.LeftIndex,
            Finger.RightIndex, Finger.RightIndex, Finger.RightIndex, Finger.RightMiddle, Finger.RightRing,
            Finger.RightPinky, Finger.RightPinky, Finger.RightPinky
        });
        RegisterRow(profiles, "asdfghjkl;'", 2, new[]
        {
            Finger.LeftPinky, Finger.LeftRing, Finger.LeftMiddle, Finger.LeftIndex, Finger.LeftIndex,
            Finger.RightIndex, Finger.RightIndex, Finger.RightIndex, Finger.RightRing, Finger.RightPinky,
            Finger.RightPinky
        });
        RegisterRow(profiles, "zxcvbnm,./", 3, new[]
        {
            Finger.LeftPinky, Finger.LeftRing, Finger.LeftMiddle, Finger.LeftIndex, Finger.LeftIndex,
            Finger.RightIndex, Finger.RightIndex, Finger.RightMiddle, Finger.RightRing, Finger.RightPinky
        });
        profiles[' '] = new KeyProfile(Hand.Thumbs, Finger.Thumb, 4);

        return profiles;
    }

    private static void RegisterRow(Dictionary<char, KeyProfile> profiles, string keys, int row, Finger[] fingers)
    {
        for (var i = 0; i < keys.Length && i < fingers.Length; i++)
        {
            var key = keys[i];
            var finger = fingers[i];
            profiles[key] = new KeyProfile(GetHand(finger), finger, row);
            if (char.IsLetter(key))
            {
                profiles[char.ToUpperInvariant(key)] = new KeyProfile(GetHand(finger), finger, row);
            }
        }
    }

    private static bool TryGetKeyProfile(char value, out KeyProfile profile)
    {
        return KeyProfiles.TryGetValue(value, out profile);
    }

    private static Hand GetHand(Finger finger)
    {
        return finger switch
        {
            Finger.LeftPinky or Finger.LeftRing or Finger.LeftMiddle or Finger.LeftIndex => Hand.Left,
            Finger.RightIndex or Finger.RightMiddle or Finger.RightRing or Finger.RightPinky => Hand.Right,
            _ => Hand.Thumbs
        };
    }

    private readonly record struct TypoPlan(string Typed, string Correct, int Advance, int NoticeDelayMs, int BackspaceDelayMs);
    private readonly record struct TypingSession(
        string Text,
        AppSettings Settings,
        IntPtr TargetWindow,
        int Index,
        char? Previous,
        int BurstWpm,
        int BurstCharsLeft,
        int NextAllowedTypoAt,
        int PostErrorSlowCharsLeft);
    private readonly record struct KeyProfile(Hand Hand, Finger Finger, int Row);

    private enum Hand
    {
        Left,
        Right,
        Thumbs
    }

    private enum Finger
    {
        LeftPinky,
        LeftRing,
        LeftMiddle,
        LeftIndex,
        RightIndex,
        RightMiddle,
        RightRing,
        RightPinky,
        Thumb
    }
}
