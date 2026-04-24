namespace HumanType.Native;

public sealed class SettingsForm : Form
{
    private const int WmSysCommand = 0x0112;
    private const int ScClose = 0xF060;
    private const int SidebarCardWidth = 296;
    private const int LeftColumnX = 28;
    private const int ColumnSpacing = 32;
    private const int SliderWidth = 340;

    private readonly BrandedSlider minWpmInput = new();
    private readonly BrandedSlider maxWpmInput = new();
    private readonly BrandedSlider typoRateInput = new();
    private readonly BrandedSlider pauseChanceInput = new();
    private readonly BrandedSlider pauseMinInput = new();
    private readonly BrandedSlider pauseMaxInput = new();
    private readonly Label minWpmValueLabel = new();
    private readonly Label maxWpmValueLabel = new();
    private readonly Label typoRateValueLabel = new();
    private readonly Label pauseChanceValueLabel = new();
    private readonly Label pauseMinValueLabel = new();
    private readonly Label pauseMaxValueLabel = new();
    private readonly Label hotkeyValueLabel = new();
    private readonly Label statusValueLabel = new();
    private readonly Label footerLabel = new();
    private readonly Label updateStatusLabel = new();
    private readonly RoundedPanel updateInfoCard = new();
    private readonly Label currentVersionLabel = new();
    private readonly Label latestVersionLabel = new();
    private readonly Label lastCheckedLabel = new();
    private readonly Label lastUpdatedLabel = new();
    private readonly Panel overlayBackdrop = new() { Visible = false };
    private readonly RoundedPanel overlayCard = new();
    private readonly Panel overlayHeader = new();
    private readonly Panel overlayBody = new();
    private readonly Panel overlayFooter = new();
    private readonly Label overlayTitleLabel = new();
    private readonly Label overlaySubtitleLabel = new();
    private readonly RichTextBox overlayNotesBox = new();
    private readonly ListBox overlayReleaseList = new();
    private readonly BrandedButton overlayCloseButton = new();
    private readonly BrandedButton overlayPrimaryButton = new();
    private readonly System.Windows.Forms.Timer overlayFadeTimer = new() { Interval = 10 };
    private int overlayTargetAlpha;
    private int overlayCurrentAlpha;
    private readonly System.Windows.Forms.Timer saveDebounceTimer = new() { Interval = 150 };
    private readonly System.Windows.Forms.Timer flashTimer = new() { Interval = 80 };
    private int _flashStep;
    private readonly BrandedToggle randomPausesToggle = new();
    private readonly BrandedOptionSelector hotkeyModifierInput = new();
    private readonly ComboBox hotkeyKeyInput = new();

    private readonly Func<Task> typeClipboardAction;
    private readonly Action stopTypingAction;
    private readonly Action<AppSettings> saveSettingsAction;
    private readonly Func<Task> checkUpdatesAction;
    private readonly Action showReleaseNotesAction;
    private readonly Action showReleaseHistoryAction;
    private readonly string currentVersion;
    private IReadOnlyList<ReleaseNoteItem> overlayReleases = [];
    private Action? overlayPrimaryAction;
    private bool allowClose;
    private bool syncingWpmInputs;
    private bool syncingPauseInputs;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED — eliminate all resize flicker
            return cp;
        }
    }

    public SettingsForm(
        AppSettings settings,
        string hotkeyText,
        Func<Task> typeClipboardAction,
        Action stopTypingAction,
        Action<AppSettings> saveSettingsAction,
        Func<Task> checkUpdatesAction,
        Action showReleaseNotesAction,
        Action showReleaseHistoryAction,
        string currentVersion)
    {
        this.typeClipboardAction = typeClipboardAction;
        this.stopTypingAction = stopTypingAction;
        this.saveSettingsAction = saveSettingsAction;
        this.checkUpdatesAction = checkUpdatesAction;
        this.showReleaseNotesAction = showReleaseNotesAction;
        this.showReleaseHistoryAction = showReleaseHistoryAction;
        this.currentVersion = currentVersion;

        AutoScaleMode = AutoScaleMode.None;
        FormBorderStyle = FormBorderStyle.Sizable;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1140, 860);
        ClientSize = new Size(1280, 920);
        DoubleBuffered = true;
        ShowInTaskbar = true;
        Icon = LoadAppIcon();
        Text = "HumanType";
        MinimizeBox = true;
        MaximizeBox = true;
        BackColor = Color.FromArgb(8, 11, 18);
        ForeColor = Color.FromArgb(237, 240, 255);
        Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);

        var chrome = new DoubleBufferedPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(10, 13, 21)
        };
        Controls.Add(chrome);

        var shell = new DoubleBufferedTableLayout
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = chrome.BackColor,
            Padding = new Padding(20, 20, 20, 20)
        };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        chrome.Controls.Add(shell);

        shell.Controls.Add(BuildSidebar(hotkeyText), 0, 0);
        shell.Controls.Add(BuildContent(settings), 1, 0);
        BuildOverlay(chrome);
        Resize += (_, _) => LayoutOverlayCard();

        saveDebounceTimer.Tick += (_, _) => { saveDebounceTimer.Stop(); ExecuteSave(); };
        flashTimer.Tick += OnFlashTick;

        UpdateValueLabels();
    }

    public void SetStatusText(string status)
    {
        statusValueLabel.Text = status;
    }

    public void UpdateHotkeyText(string hotkeyText)
    {
        hotkeyValueLabel.Text = hotkeyText;
    }

    public void SetUpdateStatusText(string status)
    {
        updateStatusLabel.Text = status;
    }

    public void SetUpdateDetails(string latestVersion, string lastChecked, string lastUpdated)
    {
        currentVersionLabel.Text = $"Installed: {currentVersion}";
        latestVersionLabel.Text = string.IsNullOrWhiteSpace(latestVersion) ? "Latest: not checked yet" : $"Latest: {latestVersion}";
        lastCheckedLabel.Text = string.IsNullOrWhiteSpace(lastChecked) ? "Last checked: never" : $"Last checked: {lastChecked}";
        lastUpdatedLabel.Text = string.IsNullOrWhiteSpace(lastUpdated) ? "Installed on: unknown" : $"Installed on: {lastUpdated}";
    }

    public void ShowReleaseOverlay(string title, string subtitle, string notes, string primaryText, Action primaryAction)
    {
        overlayReleases = [];
        overlayPrimaryAction = primaryAction;
        overlayTitleLabel.Text = title;
        overlaySubtitleLabel.Text = subtitle;
        overlayReleaseList.Visible = false;
        overlayNotesBox.Location = new Point(0, 0);
        overlayNotesBox.Size = overlayBody.ClientSize - new Size(56, 32);
        overlayPrimaryButton.Text = string.IsNullOrWhiteSpace(primaryText) ? "View Release" : primaryText;
        overlayPrimaryButton.Visible = true;
        overlayPrimaryButton.Enabled = true;
        ReleaseNotesDialog.RenderReleaseNotes(overlayNotesBox, notes);
        ShowOverlay();
    }

    public void ShowReleaseHistoryOverlay(IReadOnlyList<ReleaseNoteItem> releases)
    {
        overlayReleases = releases;
        overlayPrimaryAction = OpenSelectedOverlayRelease;
        overlayTitleLabel.Text = "Release History";
        overlaySubtitleLabel.Text = "Select a release to view its notes.";
        overlayReleaseList.Visible = true;
        overlayReleaseList.Items.Clear();
        foreach (var release in releases)
        {
            overlayReleaseList.Items.Add($"{release.Version}  {release.Name}");
        }

        var bodySize = overlayBody.ClientSize;
        var listWidth = 228;
        var gap = 16;
        overlayReleaseList.Size = new Size(listWidth, bodySize.Height - 32);
        overlayNotesBox.Location = new Point(listWidth + gap, 0);
        overlayNotesBox.Size = new Size(bodySize.Width - 56 - listWidth - gap, bodySize.Height - 32);
        overlayPrimaryButton.Text = "View Release";
        overlayPrimaryButton.Visible = true;
        overlayPrimaryButton.Enabled = releases.Count > 0;

        if (releases.Count > 0)
        {
            overlayReleaseList.SelectedIndex = 0;
            ShowSelectedOverlayRelease();
        }
        else
        {
            ReleaseNotesDialog.RenderReleaseNotes(overlayNotesBox, "No GitHub releases were found.");
        }

        ShowOverlay();
    }

    public void HideOverlay()
    {
        overlayCard.Visible = false;
        overlayTargetAlpha = 0;
        overlayFadeTimer.Start();
    }

    public void PrepareForExit()
    {
        allowClose = true;
        overlayFadeTimer.Stop();
        overlayBackdrop.Visible = false;
    }

    public void ShowAsPrimaryWindow()
    {
        ShowInTaskbar = true;
        if (!Visible) Show();
        WindowState = FormWindowState.Normal;
        BringToFront();
        Activate();
    }

    public void HideToTray()
    {
        if (!Visible) { ShowInTaskbar = false; return; }
        Hide();
        ShowInTaskbar = false;
    }

    protected override void WndProc(ref Message m)
    {
        if (allowClose) { base.WndProc(ref m); return; }
        if (m.Msg == WmSysCommand && ((int)m.WParam & 0xFFF0) == ScClose)
        {
            if (overlayBackdrop.Visible) { HideOverlay(); return; }
            HideToTray(); return;
        }
        base.WndProc(ref m);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (allowClose) { base.OnFormClosing(e); return; }
        if (e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; HideToTray(); return; }
        base.OnFormClosing(e);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (overlayBackdrop.Visible && keyData == Keys.Escape) { HideOverlay(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    // ── Sidebar ──────────────────────────────────────────────────

    private Control BuildSidebar(string hotkeyText)
    {
        var sidebar = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22),
            CornerRadius = 12,
            PanelBorderColor = Color.FromArgb(22, 28, 44)
        };

        var stack = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = false,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        var intro = new Panel
        {
            BackColor = Color.Transparent,
            Size = new Size(SidebarCardWidth, 196),
            Margin = new Padding(0, 0, 0, 14)
        };

        var accent = new Panel
        {
            BackColor = Color.FromArgb(99, 113, 255),
            Location = new Point(0, 4),
            Size = new Size(48, 3)
        };

        var kicker = new Label
        {
            Text = "HUMANTYPE",
            Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(118, 126, 255),
            AutoSize = true,
            Location = new Point(0, 18)
        };

        var title = new Label
        {
            Text = "Settings apply instantly.",
            Font = new Font("Segoe UI Semibold", 18f, FontStyle.Bold),
            ForeColor = Color.FromArgb(246, 248, 255),
            MaximumSize = new Size(SidebarCardWidth - 8, 0),
            AutoSize = true,
            Location = new Point(0, 44)
        };

        var body = new Label
        {
            Text = "Tune speed, pauses, and hotkeys live.\nNo need to restart a typing run to test changes.",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(160, 170, 204),
            MaximumSize = new Size(SidebarCardWidth - 8, 0),
            AutoSize = true,
            Location = new Point(0, 96)
        };

        intro.Controls.Add(accent);
        intro.Controls.Add(kicker);
        intro.Controls.Add(title);
        intro.Controls.Add(body);

        stack.Controls.Add(intro);
        stack.Controls.Add(BuildSidebarSummaryCard(hotkeyText));
        stack.Controls.Add(BuildSidebarUpdatesCard());
        sidebar.Controls.Add(stack);
        return sidebar;
    }

    private Control BuildSidebarSummaryCard(string hotkeyText)
    {
        var card = new RoundedPanel
        {
            BackColor = Color.FromArgb(16, 20, 31),
            Size = new Size(SidebarCardWidth, 150),
            Margin = new Padding(0, 0, 0, 14),
            CornerRadius = 8,
            PanelBorderColor = Color.FromArgb(24, 30, 48)
        };

        var titleLabel = CreateInfoTitleLabel("SESSION");
        var hotkeyTitle = CreateSidebarFieldLabel("Hotkey");
        hotkeyTitle.Location = new Point(16, 40);
        hotkeyValueLabel.Text = hotkeyText;
        StyleSidebarValueLabel(hotkeyValueLabel);
        hotkeyValueLabel.Location = new Point(16, 58);

        var statusTitle = CreateSidebarFieldLabel("Status");
        statusTitle.Location = new Point(16, 90);
        statusValueLabel.Text = "Idle";
        StyleSidebarValueLabel(statusValueLabel);
        statusValueLabel.Location = new Point(16, 108);

        card.Controls.Add(titleLabel);
        card.Controls.Add(hotkeyTitle);
        card.Controls.Add(hotkeyValueLabel);
        card.Controls.Add(statusTitle);
        card.Controls.Add(statusValueLabel);
        return card;
    }

    private Control BuildSidebarUpdatesCard()
    {
        var innerWidth = SidebarCardWidth - 32;
        updateInfoCard.BackColor = Color.FromArgb(16, 20, 31);
        updateInfoCard.Size = new Size(SidebarCardWidth, 320);
        updateInfoCard.Margin = Padding.Empty;
        updateInfoCard.CornerRadius = 8;
        updateInfoCard.PanelBorderColor = Color.FromArgb(24, 30, 48);
        updateInfoCard.Controls.Clear();

        var title = CreateInfoTitleLabel("UPDATES");
        title.Location = new Point(16, 14);

        var summaryLabel = new Label
        {
            Text = "Auto-installs when a newer GitHub release is available.",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(159, 168, 199),
            MaximumSize = new Size(innerWidth, 0),
            AutoSize = true,
            Location = new Point(16, 38)
        };

        var checkButton = CreatePrimaryButton("Check Updates");
        checkButton.Clicked += async (_, _) => { SetUpdateStatusText("Checking GitHub..."); await checkUpdatesAction(); };
        checkButton.Size = new Size(innerWidth, 36);
        checkButton.Location = new Point(16, 82);

        var notesButton = CreateSecondaryButton("Release Notes");
        notesButton.Clicked += (_, _) => showReleaseNotesAction();
        notesButton.Size = new Size(innerWidth, 36);
        notesButton.Location = new Point(16, 124);

        var historyButton = CreateSecondaryButton("History");
        historyButton.Clicked += (_, _) => showReleaseHistoryAction();
        historyButton.Size = new Size(innerWidth, 36);
        historyButton.Location = new Point(16, 166);

        updateStatusLabel.Text = "Updates are checked automatically in the background.";
        updateStatusLabel.Font = new Font("Segoe UI", 8.75f);
        updateStatusLabel.ForeColor = Color.FromArgb(150, 159, 190);
        updateStatusLabel.AutoSize = true;
        updateStatusLabel.MaximumSize = new Size(innerWidth, 0);
        updateStatusLabel.Location = new Point(16, 212);

        var detailsDivider = new Panel
        {
            BackColor = Color.FromArgb(30, 37, 57),
            Location = new Point(16, 248),
            Size = new Size(innerWidth, 1)
        };

        StyleUpdateDetailLabel(currentVersionLabel);
        StyleUpdateDetailLabel(latestVersionLabel);
        StyleUpdateDetailLabel(lastCheckedLabel);
        StyleUpdateDetailLabel(lastUpdatedLabel);
        currentVersionLabel.Location = new Point(16, 258);
        latestVersionLabel.Location = new Point(16, 276);
        lastCheckedLabel.Location = new Point(16, 294);
        lastUpdatedLabel.Visible = false;
        SetUpdateDetails(string.Empty, string.Empty, string.Empty);

        updateInfoCard.Controls.Add(title);
        updateInfoCard.Controls.Add(summaryLabel);
        updateInfoCard.Controls.Add(checkButton);
        updateInfoCard.Controls.Add(notesButton);
        updateInfoCard.Controls.Add(historyButton);
        updateInfoCard.Controls.Add(updateStatusLabel);
        updateInfoCard.Controls.Add(detailsDivider);
        updateInfoCard.Controls.Add(currentVersionLabel);
        updateInfoCard.Controls.Add(latestVersionLabel);
        updateInfoCard.Controls.Add(lastCheckedLabel);
        return updateInfoCard;
    }

    // ── Content ──────────────────────────────────────────────────

    private Control BuildContent(AppSettings settings)
    {
        var content = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.FromArgb(10, 13, 21),
            Padding = new Padding(20, 0, 18, 0),
            Margin = Padding.Empty
        };

        content.Controls.Add(BuildHeroCard());
        content.Controls.Add(BuildQuickActionsCard());
        content.Controls.Add(BuildSpeedCard(settings));
        content.Controls.Add(BuildBehaviorCard(settings));
        content.Controls.Add(BuildAutomationCard(settings));
        content.Controls.Add(BuildFooterBar());

        var scrollHost = new BrandedScrollPanel
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            BackColor = Color.FromArgb(10, 13, 21)
        };
        scrollHost.SetContent(content);

        scrollHost.Resize += (_, _) =>
        {
            var cardWidth = Math.Max(600, scrollHost.ClientSize.Width - 58);
            foreach (Control card in content.Controls)
                card.Width = cardWidth;
            RelayoutCardContents(cardWidth);
        };

        return scrollHost;
    }

    private void RelayoutCardContents(int cardWidth)
    {
        var rightColumnX = LeftColumnX + SliderWidth + ColumnSpacing;
        minWpmInput.Location = new Point(LeftColumnX, minWpmInput.Location.Y);
        maxWpmInput.Location = new Point(rightColumnX, maxWpmInput.Location.Y);
        minWpmValueLabel.Location = new Point(LeftColumnX, minWpmValueLabel.Location.Y);
        maxWpmValueLabel.Location = new Point(rightColumnX, maxWpmValueLabel.Location.Y);

        typoRateInput.Location = new Point(LeftColumnX, typoRateInput.Location.Y);
        pauseChanceInput.Location = new Point(rightColumnX, pauseChanceInput.Location.Y);
        pauseMinInput.Location = new Point(LeftColumnX, pauseMinInput.Location.Y);
        pauseMaxInput.Location = new Point(rightColumnX, pauseMaxInput.Location.Y);
        typoRateValueLabel.Location = new Point(LeftColumnX, typoRateValueLabel.Location.Y);
        pauseChanceValueLabel.Location = new Point(rightColumnX, pauseChanceValueLabel.Location.Y);
        pauseMinValueLabel.Location = new Point(LeftColumnX, pauseMinValueLabel.Location.Y);
        pauseMaxValueLabel.Location = new Point(rightColumnX, pauseMaxValueLabel.Location.Y);

        hotkeyKeyInput.Location = new Point(rightColumnX, hotkeyKeyInput.Location.Y);
    }

    private Control BuildHeroCard()
    {
        var card = CreateCardPanel();
        card.Size = new Size(800, 148);

        var eyebrow = new Label
        {
            Text = "CONTROL PANEL",
            Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(118, 126, 255),
            AutoSize = true,
            Location = new Point(28, 22)
        };

        var title = new Label
        {
            Text = "Change settings live while you work.",
            Font = new Font("Segoe UI Semibold", 22f, FontStyle.Bold),
            ForeColor = Color.FromArgb(246, 248, 255),
            AutoSize = true,
            Location = new Point(28, 44)
        };

        var body = new Label
        {
            Text = "WPM, typo behavior, random pauses, and hotkeys apply immediately.\nPause and resume to continue with updated settings.",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.FromArgb(159, 168, 199),
            MaximumSize = new Size(720, 0),
            AutoSize = true,
            Location = new Point(28, 86)
        };

        card.Controls.Add(eyebrow);
        card.Controls.Add(title);
        card.Controls.Add(body);
        return card;
    }

    private Control BuildQuickActionsCard()
    {
        var card = CreateCardPanel();
        card.Size = new Size(800, 118);

        var title = CreateSectionTitle("Quick Actions");
        title.Location = new Point(28, 20);

        var typeButton = CreatePrimaryButton("Type Clipboard");
        typeButton.Clicked += async (_, _) => { SetStatusText("Typing..."); await typeClipboardAction(); };
        typeButton.Location = new Point(LeftColumnX, 58);

        var stopButton = CreateSecondaryButton("Stop Typing");
        stopButton.Clicked += (_, _) => { stopTypingAction(); SetStatusText("Stopped"); };
        stopButton.Location = new Point(LeftColumnX + 170, 58);

        var trayButton = CreateSecondaryButton("Hide To Tray");
        trayButton.Clicked += (_, _) => HideToTray();
        trayButton.Location = new Point(LeftColumnX + 328, 58);

        card.Controls.Add(title);
        card.Controls.Add(typeButton);
        card.Controls.Add(stopButton);
        card.Controls.Add(trayButton);
        return card;
    }

    private Control BuildSpeedCard(AppSettings settings)
    {
        var card = CreateCardPanel();
        card.Size = new Size(800, 216);

        var title = CreateSectionTitle("Typing Speed");
        title.Location = new Point(28, 20);

        var rightCol = LeftColumnX + SliderWidth + ColumnSpacing;
        ConfigureSlider(minWpmInput, "Minimum WPM", settings.MinWpm, 10, 260, 1, " WPM");
        ConfigureSlider(maxWpmInput, "Maximum WPM", settings.MaxWpm, 10, 260, 1, " WPM");
        minWpmInput.Location = new Point(LeftColumnX, 56);
        maxWpmInput.Location = new Point(rightCol, 56);
        minWpmInput.ValueChanged += (_, _) => HandleMinWpmChanged();
        maxWpmInput.ValueChanged += (_, _) => HandleMaxWpmChanged();

        StyleValueLabel(minWpmValueLabel);
        StyleValueLabel(maxWpmValueLabel);
        minWpmValueLabel.Location = new Point(LeftColumnX, 158);
        maxWpmValueLabel.Location = new Point(rightCol, 158);

        card.Controls.Add(title);
        card.Controls.Add(minWpmInput);
        card.Controls.Add(maxWpmInput);
        card.Controls.Add(minWpmValueLabel);
        card.Controls.Add(maxWpmValueLabel);
        return card;
    }

    private Control BuildBehaviorCard(AppSettings settings)
    {
        var card = CreateCardPanel();
        card.Size = new Size(800, 370);

        var title = CreateSectionTitle("Typing Behavior");
        title.Location = new Point(28, 20);

        var rightCol = LeftColumnX + SliderWidth + ColumnSpacing;
        ConfigureSlider(typoRateInput, "Typo Rate", settings.TypoRate * 100d, 0, 30, 0.25, "%");
        ConfigureSlider(pauseChanceInput, "Pause Frequency", settings.PauseChance * 100d, 0, 100, 1, "%");
        ConfigureSlider(pauseMinInput, "Pause Minimum", settings.PauseMinMs, 0, 1200, 10, " ms");
        ConfigureSlider(pauseMaxInput, "Pause Maximum", settings.PauseMaxMs, 0, 2000, 10, " ms");

        typoRateInput.Location = new Point(LeftColumnX, 56);
        pauseChanceInput.Location = new Point(rightCol, 56);
        pauseMinInput.Location = new Point(LeftColumnX, 186);
        pauseMaxInput.Location = new Point(rightCol, 186);

        typoRateInput.ValueChanged += (_, _) => ApplySettingsFromInputs();
        pauseChanceInput.ValueChanged += (_, _) => ApplySettingsFromInputs();
        pauseMinInput.ValueChanged += (_, _) => HandlePauseMinChanged();
        pauseMaxInput.ValueChanged += (_, _) => HandlePauseMaxChanged();

        StyleValueLabel(typoRateValueLabel);
        StyleValueLabel(pauseChanceValueLabel);
        StyleValueLabel(pauseMinValueLabel);
        StyleValueLabel(pauseMaxValueLabel);
        typoRateValueLabel.Location = new Point(LeftColumnX, 158);
        pauseChanceValueLabel.Location = new Point(rightCol, 158);
        pauseMinValueLabel.Location = new Point(LeftColumnX, 290);
        pauseMaxValueLabel.Location = new Point(rightCol, 290);

        var note = new Label
        {
            Text = "Random pauses can be disabled entirely, or tuned with frequency and time range. Typo rate is independent from pause behavior.",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(140, 150, 185),
            MaximumSize = new Size(700, 0),
            AutoSize = true,
            Location = new Point(LeftColumnX, 326)
        };

        card.Controls.Add(title);
        card.Controls.Add(typoRateInput);
        card.Controls.Add(pauseChanceInput);
        card.Controls.Add(pauseMinInput);
        card.Controls.Add(pauseMaxInput);
        card.Controls.Add(typoRateValueLabel);
        card.Controls.Add(pauseChanceValueLabel);
        card.Controls.Add(pauseMinValueLabel);
        card.Controls.Add(pauseMaxValueLabel);
        card.Controls.Add(note);
        return card;
    }

    private Control BuildAutomationCard(AppSettings settings)
    {
        var card = CreateCardPanel();
        card.Size = new Size(800, 270);

        var title = CreateSectionTitle("Automation");
        title.Location = new Point(28, 20);

        randomPausesToggle.Checked = settings.RandomPausesEnabled;
        randomPausesToggle.Location = new Point(LeftColumnX, 56);
        randomPausesToggle.CheckedChanged += (_, _) => ApplySettingsFromInputs();

        var toggleLabel = new Label
        {
            Text = "Enable random pauses",
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            ForeColor = Color.FromArgb(233, 237, 255),
            AutoSize = true,
            Location = new Point(LeftColumnX + 86, 62)
        };

        var rightCol = LeftColumnX + SliderWidth + ColumnSpacing;

        var modifierLabel = new Label
        {
            Text = "Hotkey Modifiers",
            Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(142, 152, 184),
            AutoSize = true,
            Location = new Point(LeftColumnX, 106)
        };

        hotkeyModifierInput.SetOptions(["Ctrl+Alt", "Ctrl+Shift", "Alt+Shift", "Ctrl+Alt+Shift"]);
        hotkeyModifierInput.SelectedValue = settings.HotkeyModifiers;
        hotkeyModifierInput.Size = new Size(SliderWidth, 78);
        hotkeyModifierInput.Location = new Point(LeftColumnX, 128);
        hotkeyModifierInput.SelectedValueChanged += (_, _) => ApplySettingsFromInputs();

        var keyLabel = new Label
        {
            Text = "Hotkey Key",
            Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(142, 152, 184),
            AutoSize = true,
            Location = new Point(rightCol, 106)
        };

        StyleComboBox(hotkeyKeyInput);
        hotkeyKeyInput.Items.AddRange(BuildHotkeyKeyOptions());
        hotkeyKeyInput.SelectedItem = hotkeyKeyInput.Items.Contains(settings.HotkeyKey) ? settings.HotkeyKey : "V";
        hotkeyKeyInput.Size = new Size(180, 34);
        hotkeyKeyInput.Location = new Point(rightCol, 128);
        hotkeyKeyInput.SelectedIndexChanged += (_, _) => ApplySettingsFromInputs();

        card.Controls.Add(title);
        card.Controls.Add(randomPausesToggle);
        card.Controls.Add(toggleLabel);
        card.Controls.Add(modifierLabel);
        card.Controls.Add(hotkeyModifierInput);
        card.Controls.Add(keyLabel);
        card.Controls.Add(hotkeyKeyInput);
        return card;
    }

    private Control BuildFooterBar()
    {
        var card = CreateCardPanel();
        card.Size = new Size(800, 84);

        var closeButton = CreateSecondaryButton("Hide Window");
        closeButton.Clicked += (_, _) => HideToTray();
        closeButton.Location = new Point(LeftColumnX, 22);

        footerLabel.Text = "Changes apply automatically and are stored locally.";
        footerLabel.Font = new Font("Segoe UI", 9.5f);
        footerLabel.ForeColor = Color.FromArgb(150, 159, 190);
        footerLabel.AutoSize = true;
        footerLabel.Location = new Point(LeftColumnX + 170, 32);

        card.Controls.Add(closeButton);
        card.Controls.Add(footerLabel);
        return card;
    }

    // ── Overlay ────────────────────────────��─────────────────────

    private void BuildOverlay(Control host)
    {
        overlayBackdrop.Location = new Point(0, 0);
        overlayBackdrop.Size = host.ClientSize;
        overlayBackdrop.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        overlayBackdrop.BackColor = Color.FromArgb(0, 0, 0, 0);
        overlayBackdrop.Visible = false;

        overlayCard.Size = new Size(900, 600);
        overlayCard.BackColor = Color.FromArgb(14, 18, 28);
        overlayCard.CornerRadius = 14;
        overlayCard.PanelBorderColor = Color.FromArgb(38, 46, 68);
        overlayCard.PanelBorderWidth = 1;

        overlayHeader.Dock = DockStyle.Top;
        overlayHeader.Height = 90;
        overlayHeader.BackColor = Color.FromArgb(14, 18, 28);

        overlayTitleLabel.Font = new Font("Segoe UI Semibold", 16f, FontStyle.Bold);
        overlayTitleLabel.ForeColor = Color.FromArgb(246, 248, 255);
        overlayTitleLabel.AutoSize = true;
        overlayTitleLabel.Location = new Point(28, 18);

        overlaySubtitleLabel.Font = new Font("Segoe UI", 9.75f);
        overlaySubtitleLabel.ForeColor = Color.FromArgb(159, 168, 199);
        overlaySubtitleLabel.MaximumSize = new Size(820, 0);
        overlaySubtitleLabel.AutoSize = true;
        overlaySubtitleLabel.Location = new Point(30, 50);

        overlayBody.Dock = DockStyle.Fill;
        overlayBody.BackColor = Color.FromArgb(10, 13, 21);
        overlayBody.Padding = new Padding(28, 14, 28, 14);

        overlayReleaseList.BackColor = Color.FromArgb(16, 20, 31);
        overlayReleaseList.ForeColor = Color.FromArgb(233, 237, 255);
        overlayReleaseList.BorderStyle = BorderStyle.None;
        overlayReleaseList.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        overlayReleaseList.Location = new Point(0, 0);
        overlayReleaseList.Size = new Size(228, 400);
        overlayReleaseList.SelectedIndexChanged += (_, _) => ShowSelectedOverlayRelease();

        overlayNotesBox.ReadOnly = true;
        overlayNotesBox.DetectUrls = true;
        overlayNotesBox.ScrollBars = RichTextBoxScrollBars.Vertical;
        overlayNotesBox.BorderStyle = BorderStyle.None;
        overlayNotesBox.BackColor = Color.FromArgb(16, 20, 31);
        overlayNotesBox.ForeColor = Color.FromArgb(233, 237, 255);
        overlayNotesBox.Font = new Font("Segoe UI", 10f);
        overlayNotesBox.TabStop = false;

        overlayFooter.Dock = DockStyle.Bottom;
        overlayFooter.Height = 64;
        overlayFooter.BackColor = Color.FromArgb(14, 18, 28);

        var footerDivider = new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = Color.FromArgb(30, 37, 57)
        };

        overlayCloseButton.Text = "Close";
        overlayCloseButton.Size = new Size(110, 38);
        overlayCloseButton.Clicked += (_, _) => HideOverlay();

        overlayPrimaryButton.Text = "View Release";
        overlayPrimaryButton.Size = new Size(140, 38);
        overlayPrimaryButton.IsPrimary = true;
        overlayPrimaryButton.Clicked += (_, _) => overlayPrimaryAction?.Invoke();

        LayoutOverlayFooterButtons();

        overlayHeader.Controls.Add(overlayTitleLabel);
        overlayHeader.Controls.Add(overlaySubtitleLabel);
        overlayBody.Controls.Add(overlayReleaseList);
        overlayBody.Controls.Add(overlayNotesBox);
        overlayFooter.Controls.Add(footerDivider);
        overlayFooter.Controls.Add(overlayCloseButton);
        overlayFooter.Controls.Add(overlayPrimaryButton);
        overlayCard.Controls.Add(overlayBody);
        overlayCard.Controls.Add(overlayFooter);
        overlayCard.Controls.Add(overlayHeader);
        overlayBackdrop.Controls.Add(overlayCard);
        host.Controls.Add(overlayBackdrop);
        overlayBackdrop.SendToBack();

        overlayFadeTimer.Tick += OnOverlayFadeTick;
        LayoutOverlayCard();
    }

    private void LayoutOverlayFooterButtons()
    {
        var footerWidth = overlayCard.Width - 4;
        overlayPrimaryButton.Location = new Point(footerWidth - overlayPrimaryButton.Width - 24, 13);
        overlayCloseButton.Location = new Point(overlayPrimaryButton.Left - overlayCloseButton.Width - 10, 13);
    }

    private void LayoutOverlayCard()
    {
        overlayCard.Location = new Point(
            Math.Max(24, (overlayBackdrop.ClientSize.Width - overlayCard.Width) / 2),
            Math.Max(24, (overlayBackdrop.ClientSize.Height - overlayCard.Height) / 2));
        LayoutOverlayFooterButtons();
    }

    private void ShowOverlay()
    {
        overlayCurrentAlpha = 0;
        overlayTargetAlpha = 170;
        overlayBackdrop.BackColor = Color.FromArgb(0, 0, 0, 0);
        overlayCard.Visible = false;
        overlayBackdrop.Visible = true;
        overlayBackdrop.BringToFront();
        LayoutOverlayCard();
        overlayFadeTimer.Start();
    }

    private void OnOverlayFadeTick(object? sender, EventArgs e)
    {
        if (overlayCurrentAlpha < overlayTargetAlpha)
        {
            overlayCurrentAlpha = Math.Min(overlayCurrentAlpha + 34, overlayTargetAlpha);
            overlayBackdrop.BackColor = Color.FromArgb(overlayCurrentAlpha, 2, 4, 10);

            if (!overlayCard.Visible && overlayCurrentAlpha >= 40)
            {
                overlayCard.Visible = true;
                overlayCloseButton.Focus();
            }

            if (overlayCurrentAlpha >= overlayTargetAlpha)
                overlayFadeTimer.Stop();
        }
        else if (overlayCurrentAlpha > overlayTargetAlpha)
        {
            overlayCurrentAlpha = Math.Max(overlayCurrentAlpha - 50, 0);
            overlayBackdrop.BackColor = Color.FromArgb(overlayCurrentAlpha, 2, 4, 10);

            if (overlayCurrentAlpha <= 0)
            {
                overlayFadeTimer.Stop();
                overlayBackdrop.Visible = false;
            }
        }
        else
        {
            overlayFadeTimer.Stop();
        }
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static RoundedPanel CreateCardPanel()
    {
        return new RoundedPanel
        {
            BackColor = Color.FromArgb(12, 16, 25),
            Margin = new Padding(0, 0, 0, 14),
            CornerRadius = 10,
            PanelBorderColor = Color.FromArgb(22, 28, 44)
        };
    }

    private static Label CreateSectionTitle(string text)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI Semibold", 14f, FontStyle.Bold),
            ForeColor = Color.FromArgb(246, 248, 255),
            AutoSize = true
        };
    }

    private static void ConfigureSlider(BrandedSlider input, string title, double value, double minimum, double maximum, double step, string suffix)
    {
        input.Title = title;
        input.Minimum = minimum;
        input.Maximum = maximum;
        input.Step = step;
        input.Suffix = suffix;
        input.Value = value;
        input.Size = new Size(SliderWidth, 82);
    }

    private static void StyleValueLabel(Label label)
    {
        label.Font = new Font("Segoe UI", 9.5f);
        label.ForeColor = Color.FromArgb(142, 152, 184);
        label.AutoSize = true;
    }

    private static void StyleUpdateDetailLabel(Label label)
    {
        label.Font = new Font("Segoe UI", 8.75f);
        label.ForeColor = Color.FromArgb(142, 152, 184);
        label.AutoSize = true;
        label.MaximumSize = new Size(SidebarCardWidth - 32, 0);
    }

    private static Label CreateInfoTitleLabel(string text)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(110, 121, 160),
            AutoSize = true,
            Location = new Point(16, 14)
        };
    }

    private static Label CreateSidebarFieldLabel(string text)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(110, 121, 160),
            AutoSize = true
        };
    }

    private static void StyleSidebarValueLabel(Label label)
    {
        label.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        label.ForeColor = Color.FromArgb(240, 243, 255);
        label.AutoSize = true;
        label.MaximumSize = new Size(252, 0);
    }

    private static string[] BuildHotkeyKeyOptions()
    {
        var options = new List<string>();
        for (var c = 'A'; c <= 'Z'; c++) options.Add(c.ToString());
        for (var i = 1; i <= 12; i++) options.Add($"F{i}");
        return options.ToArray();
    }

    private static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.BackColor = Color.FromArgb(24, 30, 46);
        comboBox.ForeColor = Color.FromArgb(233, 237, 255);
        comboBox.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        comboBox.IntegralHeight = false;
    }

    private static BrandedButton CreatePrimaryButton(string text)
    {
        return new BrandedButton
        {
            Text = text,
            Size = new Size(156, 40),
            IsPrimary = true
        };
    }

    private static BrandedButton CreateSecondaryButton(string text)
    {
        return new BrandedButton
        {
            Text = text,
            Size = new Size(144, 40)
        };
    }

    // ── State ────────────────────────────────────────────────────

    private void UpdateValueLabels()
    {
        minWpmValueLabel.Text = $"Burst floor: {minWpmInput.Value:0} WPM";
        maxWpmValueLabel.Text = $"Burst ceiling: {maxWpmInput.Value:0} WPM";
        typoRateValueLabel.Text = $"Temporary typo frequency: {typoRateInput.Value:0.00}%";
        pauseChanceValueLabel.Text = $"Random pause frequency: {pauseChanceInput.Value:0}%";
        pauseMinValueLabel.Text = $"Minimum pause: {pauseMinInput.Value:0} ms";
        pauseMaxValueLabel.Text = $"Maximum pause: {pauseMaxInput.Value:0} ms";
    }

    private void ShowSelectedOverlayRelease()
    {
        if (!overlayReleaseList.Visible) return;
        if (overlayReleaseList.SelectedIndex < 0 || overlayReleaseList.SelectedIndex >= overlayReleases.Count)
        {
            overlaySubtitleLabel.Text = "Select a release to view its notes.";
            overlayPrimaryButton.Enabled = false;
            return;
        }
        var release = overlayReleases[overlayReleaseList.SelectedIndex];
        overlaySubtitleLabel.Text = $"Viewing {release.Version}. Open the full GitHub release page if you need release assets.";
        ReleaseNotesDialog.RenderReleaseNotes(overlayNotesBox, $"# {release.Name}\n\n{release.Notes}");
        overlayPrimaryButton.Enabled = !string.IsNullOrWhiteSpace(release.ReleasePageUrl);
    }

    private void OpenSelectedOverlayRelease()
    {
        if (overlayReleaseList.SelectedIndex < 0 || overlayReleaseList.SelectedIndex >= overlayReleases.Count) return;
        UpdateService.OpenUrl(overlayReleases[overlayReleaseList.SelectedIndex].ReleasePageUrl);
    }

    private void HandleMinWpmChanged()
    {
        if (syncingWpmInputs) { UpdateValueLabels(); return; }
        syncingWpmInputs = true;
        if (minWpmInput.Value > maxWpmInput.Value) maxWpmInput.Value = minWpmInput.Value;
        syncingWpmInputs = false;
        ApplySettingsFromInputs();
    }

    private void HandleMaxWpmChanged()
    {
        if (syncingWpmInputs) { UpdateValueLabels(); return; }
        syncingWpmInputs = true;
        if (maxWpmInput.Value < minWpmInput.Value) minWpmInput.Value = maxWpmInput.Value;
        syncingWpmInputs = false;
        ApplySettingsFromInputs();
    }

    private void HandlePauseMinChanged()
    {
        if (syncingPauseInputs) { UpdateValueLabels(); return; }
        syncingPauseInputs = true;
        if (pauseMinInput.Value > pauseMaxInput.Value) pauseMaxInput.Value = pauseMinInput.Value;
        syncingPauseInputs = false;
        ApplySettingsFromInputs();
    }

    private void HandlePauseMaxChanged()
    {
        if (syncingPauseInputs) { UpdateValueLabels(); return; }
        syncingPauseInputs = true;
        if (pauseMaxInput.Value < pauseMinInput.Value) pauseMinInput.Value = pauseMaxInput.Value;
        syncingPauseInputs = false;
        ApplySettingsFromInputs();
    }

    private void ApplySettingsFromInputs()
    {
        UpdateValueLabels();
        footerLabel.Text = "Saving...";
        footerLabel.ForeColor = Color.FromArgb(150, 159, 190);
        saveDebounceTimer.Stop();
        saveDebounceTimer.Start();
    }

    private void ExecuteSave()
    {
        var settings = BuildSettings();
        saveSettingsAction(settings);
        UpdateHotkeyText($"{settings.HotkeyModifiers}+{settings.HotkeyKey}");
        footerLabel.Text = "Applied.";
        _flashStep = 0;
        flashTimer.Stop();
        flashTimer.Start();
    }

    private void OnFlashTick(object? sender, EventArgs e)
    {
        switch (_flashStep)
        {
            case 0:
                footerLabel.ForeColor = Color.FromArgb(118, 126, 255);
                break;
            case 1:
                footerLabel.ForeColor = Color.FromArgb(160, 168, 220);
                break;
            case 2:
                footerLabel.ForeColor = Color.FromArgb(150, 159, 190);
                flashTimer.Stop();
                break;
        }
        _flashStep++;
    }

    private AppSettings BuildSettings()
    {
        var settings = new AppSettings
        {
            MinWpm = (int)Math.Round(minWpmInput.Value),
            MaxWpm = (int)Math.Round(maxWpmInput.Value),
            TypoRate = typoRateInput.Value / 100d,
            RandomPausesEnabled = randomPausesToggle.Checked,
            PauseChance = pauseChanceInput.Value / 100d,
            PauseMinMs = (int)Math.Round(pauseMinInput.Value),
            PauseMaxMs = (int)Math.Round(pauseMaxInput.Value),
            HotkeyModifiers = string.IsNullOrWhiteSpace(hotkeyModifierInput.SelectedValue) ? "Ctrl+Alt" : hotkeyModifierInput.SelectedValue,
            HotkeyKey = hotkeyKeyInput.SelectedItem?.ToString() ?? "V"
        };
        settings.Normalize();
        return settings;
    }

    private static Icon? LoadAppIcon()
    {
        var exeDirectory = AppContext.BaseDirectory;
        var candidatePaths = new[]
        {
            Path.Combine(exeDirectory, "HumanType.ico"),
            Path.GetFullPath(Path.Combine(exeDirectory, "..", "..", "..", "assets", "HumanType.ico"))
        };
        foreach (var candidate in candidatePaths)
            if (File.Exists(candidate))
                return new Icon(candidate);
        return null;
    }

    // ── Double-buffered helper containers ────────────────────────

    private sealed class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }
    }

    private sealed class DoubleBufferedTableLayout : TableLayoutPanel
    {
        public DoubleBufferedTableLayout()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }
    }
}
