namespace HumanType.Native;

public sealed class SettingsForm : Form
{
    private const int WmSysCommand = 0x0112;
    private const int ScClose = 0xF060;
    private const int ContentCardWidth = 884;
    private const int SidebarCardWidth = 288;
    private const int LeftColumnX = 28;
    private const int RightColumnX = 486;

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
    private readonly Panel updateInfoCard = new();
    private readonly Label currentVersionLabel = new();
    private readonly Label latestVersionLabel = new();
    private readonly Label lastCheckedLabel = new();
    private readonly Label lastUpdatedLabel = new();
    private readonly Panel overlayBackdrop = new();
    private readonly Panel overlayCard = new();
    private readonly Panel overlayHeader = new();
    private readonly Panel overlayBody = new();
    private readonly Panel overlayFooter = new();
    private readonly Label overlayTitleLabel = new();
    private readonly Label overlaySubtitleLabel = new();
    private readonly RichTextBox overlayNotesBox = new();
    private readonly ListBox overlayReleaseList = new();
    private readonly Button overlayCloseButton = new();
    private readonly Button overlayPrimaryButton = new();
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
    private bool syncingWpmInputs;
    private bool syncingPauseInputs;

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
        MinimumSize = new Size(1280, 920);
        ClientSize = new Size(1360, 960);
        DoubleBuffered = true;
        ShowInTaskbar = true;
        Icon = LoadAppIcon();
        Text = "HumanType";
        MinimizeBox = true;
        MaximizeBox = false;
        BackColor = Color.FromArgb(8, 11, 18);
        ForeColor = Color.FromArgb(237, 240, 255);
        Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);

        var chrome = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(10, 13, 21)
        };
        Controls.Add(chrome);

        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = chrome.BackColor,
            Padding = new Padding(24)
        };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 354));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        chrome.Controls.Add(shell);

        shell.Controls.Add(BuildSidebar(hotkeyText), 0, 0);
        shell.Controls.Add(BuildContent(settings), 1, 0);
        BuildOverlay(chrome);
        Resize += (_, _) => LayoutOverlayCard();

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
        overlayNotesBox.Location = new Point(28, 18);
        overlayNotesBox.Size = new Size(804, 422);
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

        overlayNotesBox.Location = new Point(272, 18);
        overlayNotesBox.Size = new Size(560, 422);
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
        overlayBackdrop.Visible = false;
    }

    public void ShowAsPrimaryWindow()
    {
        ShowInTaskbar = true;
        if (!Visible)
        {
            Show();
        }

        WindowState = FormWindowState.Normal;
        BringToFront();
        Activate();
    }

    public void HideToTray()
    {
        if (!Visible)
        {
            ShowInTaskbar = false;
            return;
        }

        Hide();
        ShowInTaskbar = false;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmSysCommand && ((int)m.WParam & 0xFFF0) == ScClose)
        {
            if (overlayBackdrop.Visible)
            {
                HideOverlay();
                return;
            }

            HideToTray();
            return;
        }

        base.WndProc(ref m);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }

        base.OnFormClosing(e);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (overlayBackdrop.Visible && keyData == Keys.Escape)
        {
            HideOverlay();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private Control BuildSidebar(string hotkeyText)
    {
        var sidebar = CreateCardPanel();
        sidebar.Dock = DockStyle.Fill;
        sidebar.Padding = new Padding(22);

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
            Size = new Size(SidebarCardWidth, 168),
            Margin = new Padding(0, 0, 0, 14)
        };

        var accent = new Panel
        {
            BackColor = Color.FromArgb(99, 113, 255),
            Location = new Point(0, 4),
            Size = new Size(54, 4)
        };

        var kicker = new Label
        {
            Text = "HUMANTYPE",
            Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(118, 126, 255),
            AutoSize = true,
            Location = new Point(0, 20)
        };

        var title = new Label
        {
            Text = "Settings apply instantly.",
            Font = new Font("Segoe UI Semibold", 20f, FontStyle.Bold),
            ForeColor = Color.FromArgb(246, 248, 255),
            MaximumSize = new Size(262, 0),
            AutoSize = true,
            Location = new Point(0, 48)
        };

        var body = new Label
        {
            Text = "Tune speed, pauses, and hotkeys live. You should not need to save and restart a typing run just to test a setting.",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(160, 170, 204),
            MaximumSize = new Size(262, 0),
            AutoSize = true,
            Location = new Point(0, 110)
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
        return scrollHost;
    }

    private void BuildOverlay(Control host)
    {
        overlayBackdrop.Location = new Point(0, 0);
        overlayBackdrop.Size = host.ClientSize;
        overlayBackdrop.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        overlayBackdrop.BackColor = Color.FromArgb(168, 4, 6, 12);
        overlayBackdrop.Visible = false;

        overlayCard.Size = new Size(860, 640);
        overlayCard.BackColor = Color.FromArgb(12, 16, 25);
        overlayCard.Padding = new Padding(1);

        overlayHeader.Dock = DockStyle.Top;
        overlayHeader.Height = 100;
        overlayHeader.BackColor = Color.FromArgb(14, 18, 28);

        overlayTitleLabel.Font = new Font("Segoe UI Semibold", 18f, FontStyle.Bold);
        overlayTitleLabel.ForeColor = Color.FromArgb(246, 248, 255);
        overlayTitleLabel.AutoSize = true;
        overlayTitleLabel.Location = new Point(28, 22);

        overlaySubtitleLabel.Font = new Font("Segoe UI", 10f);
        overlaySubtitleLabel.ForeColor = Color.FromArgb(159, 168, 199);
        overlaySubtitleLabel.MaximumSize = new Size(780, 0);
        overlaySubtitleLabel.AutoSize = true;
        overlaySubtitleLabel.Location = new Point(30, 58);

        overlayBody.Dock = DockStyle.Fill;
        overlayBody.BackColor = Color.FromArgb(10, 13, 21);

        overlayReleaseList.BackColor = Color.FromArgb(16, 20, 31);
        overlayReleaseList.ForeColor = Color.FromArgb(233, 237, 255);
        overlayReleaseList.BorderStyle = BorderStyle.FixedSingle;
        overlayReleaseList.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        overlayReleaseList.Location = new Point(28, 18);
        overlayReleaseList.Size = new Size(212, 422);
        overlayReleaseList.SelectedIndexChanged += (_, _) => ShowSelectedOverlayRelease();

        overlayNotesBox.ReadOnly = true;
        overlayNotesBox.DetectUrls = true;
        overlayNotesBox.ScrollBars = RichTextBoxScrollBars.Vertical;
        overlayNotesBox.BorderStyle = BorderStyle.FixedSingle;
        overlayNotesBox.BackColor = Color.FromArgb(16, 20, 31);
        overlayNotesBox.ForeColor = Color.FromArgb(233, 237, 255);
        overlayNotesBox.Font = new Font("Segoe UI", 10f);
        overlayNotesBox.TabStop = false;

        overlayFooter.Dock = DockStyle.Bottom;
        overlayFooter.Height = 76;
        overlayFooter.BackColor = Color.FromArgb(10, 13, 21);

        var footerDivider = new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = Color.FromArgb(30, 37, 57)
        };

        overlayCloseButton.Text = "Close";
        overlayCloseButton.Size = new Size(132, 42);
        overlayCloseButton.BackColor = Color.FromArgb(24, 30, 46);
        overlayCloseButton.ForeColor = Color.FromArgb(233, 237, 255);
        overlayCloseButton.FlatStyle = FlatStyle.Flat;
        overlayCloseButton.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        overlayCloseButton.FlatAppearance.BorderColor = Color.FromArgb(42, 49, 71);
        overlayCloseButton.FlatAppearance.BorderSize = 1;
        overlayCloseButton.Location = new Point(580, 18);
        overlayCloseButton.Click += (_, _) => HideOverlay();

        overlayPrimaryButton.Size = new Size(160, 42);
        overlayPrimaryButton.BackColor = Color.FromArgb(99, 113, 255);
        overlayPrimaryButton.ForeColor = Color.White;
        overlayPrimaryButton.FlatStyle = FlatStyle.Flat;
        overlayPrimaryButton.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        overlayPrimaryButton.FlatAppearance.BorderSize = 0;
        overlayPrimaryButton.Location = new Point(692, 18);
        overlayPrimaryButton.Click += (_, _) => overlayPrimaryAction?.Invoke();

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
        LayoutOverlayCard();
    }

    private void LayoutOverlayCard()
    {
        overlayCard.Location = new Point(
            Math.Max(24, (overlayBackdrop.ClientSize.Width - overlayCard.Width) / 2),
            Math.Max(24, (overlayBackdrop.ClientSize.Height - overlayCard.Height) / 2));
    }

    private void ShowOverlay()
    {
        overlayBackdrop.Visible = true;
        overlayBackdrop.BringToFront();
        LayoutOverlayCard();
        overlayCloseButton.Focus();
    }

    private Control BuildHeroCard()
    {
        var card = CreateCardPanel();
        card.Size = new Size(ContentCardWidth, 154);

        var eyebrow = new Label
        {
            Text = "CONTROL PANEL",
            Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(118, 126, 255),
            AutoSize = true,
            Location = new Point(28, 24)
        };

        var title = new Label
        {
            Text = "Change settings live while you work.",
            Font = new Font("Segoe UI Semibold", 24f, FontStyle.Bold),
            ForeColor = Color.FromArgb(246, 248, 255),
            AutoSize = true,
            Location = new Point(28, 46)
        };

        var body = new Label
        {
            Text = "WPM, typo behavior, random pauses, and hotkeys apply immediately. Start a new run to test from the beginning, or pause and resume to continue with updated settings.",
            Font = new Font("Segoe UI", 10.5f),
            ForeColor = Color.FromArgb(159, 168, 199),
            MaximumSize = new Size(798, 0),
            AutoSize = true,
            Location = new Point(28, 90)
        };

        card.Controls.Add(eyebrow);
        card.Controls.Add(title);
        card.Controls.Add(body);
        return card;
    }

    private Control BuildQuickActionsCard()
    {
        var card = CreateCardPanel();
        card.Size = new Size(ContentCardWidth, 132);

        var title = CreateSectionTitle("Quick Actions");
        title.Location = new Point(28, 20);

        var typeButton = CreatePrimaryButton("Type Clipboard", async (_, _) =>
        {
            SetStatusText("Typing...");
            await typeClipboardAction();
        });
        typeButton.Location = new Point(LeftColumnX, 62);

        var stopButton = CreateSecondaryButton("Stop Typing", (_, _) =>
        {
            stopTypingAction();
            SetStatusText("Stopped");
        });
        stopButton.Location = new Point(222, 62);

        var trayButton = CreateSecondaryButton("Hide To Tray", (_, _) => HideToTray());
        trayButton.Location = new Point(416, 62);

        card.Controls.Add(title);
        card.Controls.Add(typeButton);
        card.Controls.Add(stopButton);
        card.Controls.Add(trayButton);
        return card;
    }

    private Control BuildSpeedCard(AppSettings settings)
    {
        var card = CreateCardPanel();
        card.Size = new Size(ContentCardWidth, 242);

        var title = CreateSectionTitle("Typing Speed");
        title.Location = new Point(28, 20);

        ConfigureSlider(minWpmInput, "Minimum WPM", settings.MinWpm, 10, 260, 1, " WPM");
        ConfigureSlider(maxWpmInput, "Maximum WPM", settings.MaxWpm, 10, 260, 1, " WPM");
        minWpmInput.Location = new Point(LeftColumnX, 62);
        maxWpmInput.Location = new Point(RightColumnX, 62);
        minWpmInput.ValueChanged += (_, _) => HandleMinWpmChanged();
        maxWpmInput.ValueChanged += (_, _) => HandleMaxWpmChanged();

        StyleValueLabel(minWpmValueLabel);
        StyleValueLabel(maxWpmValueLabel);
        minWpmValueLabel.Location = new Point(LeftColumnX, 172);
        maxWpmValueLabel.Location = new Point(RightColumnX, 172);

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
        card.Size = new Size(ContentCardWidth, 396);

        var title = CreateSectionTitle("Typing Behavior");
        title.Location = new Point(28, 20);

        ConfigureSlider(typoRateInput, "Typo Rate", settings.TypoRate * 100d, 0, 30, 0.25, "%");
        ConfigureSlider(pauseChanceInput, "Pause Frequency", settings.PauseChance * 100d, 0, 100, 1, "%");
        ConfigureSlider(pauseMinInput, "Pause Minimum", settings.PauseMinMs, 0, 1200, 10, " ms");
        ConfigureSlider(pauseMaxInput, "Pause Maximum", settings.PauseMaxMs, 0, 2000, 10, " ms");

        typoRateInput.Location = new Point(LeftColumnX, 62);
        pauseChanceInput.Location = new Point(RightColumnX, 62);
        pauseMinInput.Location = new Point(LeftColumnX, 198);
        pauseMaxInput.Location = new Point(RightColumnX, 198);

        typoRateInput.ValueChanged += (_, _) => ApplySettingsFromInputs();
        pauseChanceInput.ValueChanged += (_, _) => ApplySettingsFromInputs();
        pauseMinInput.ValueChanged += (_, _) => HandlePauseMinChanged();
        pauseMaxInput.ValueChanged += (_, _) => HandlePauseMaxChanged();

        StyleValueLabel(typoRateValueLabel);
        StyleValueLabel(pauseChanceValueLabel);
        StyleValueLabel(pauseMinValueLabel);
        StyleValueLabel(pauseMaxValueLabel);
        typoRateValueLabel.Location = new Point(LeftColumnX, 172);
        pauseChanceValueLabel.Location = new Point(RightColumnX, 172);
        pauseMinValueLabel.Location = new Point(LeftColumnX, 316);
        pauseMaxValueLabel.Location = new Point(RightColumnX, 316);

        var note = new Label
        {
            Text = "Random pauses can be disabled entirely, or tuned with both a frequency and a time range. Typo rate stays independent from pause behavior.",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.FromArgb(159, 168, 199),
            MaximumSize = new Size(816, 0),
            AutoSize = true,
            Location = new Point(LeftColumnX, 346)
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
        card.Size = new Size(ContentCardWidth, 304);

        var title = CreateSectionTitle("Automation");
        title.Location = new Point(28, 20);

        randomPausesToggle.Checked = settings.RandomPausesEnabled;
        randomPausesToggle.Location = new Point(LeftColumnX, 60);
        randomPausesToggle.CheckedChanged += (_, _) => ApplySettingsFromInputs();

        var toggleLabel = new Label
        {
            Text = "Enable random pauses",
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            ForeColor = Color.FromArgb(233, 237, 255),
            AutoSize = true,
            Location = new Point(LeftColumnX + 86, 66)
        };

        var modifierLabel = new Label
        {
            Text = "Hotkey Modifiers",
            Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(142, 152, 184),
            AutoSize = true,
            Location = new Point(LeftColumnX, 118)
        };

        hotkeyModifierInput.SetOptions(["Ctrl+Alt", "Ctrl+Shift", "Alt+Shift", "Ctrl+Alt+Shift"]);
        hotkeyModifierInput.SelectedValue = settings.HotkeyModifiers;
        hotkeyModifierInput.Size = new Size(360, 78);
        hotkeyModifierInput.Location = new Point(LeftColumnX, 142);
        hotkeyModifierInput.SelectedValueChanged += (_, _) => ApplySettingsFromInputs();

        var keyLabel = new Label
        {
            Text = "Hotkey Key",
            Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(142, 152, 184),
            AutoSize = true,
            Location = new Point(LeftColumnX + 408, 118)
        };

        StyleComboBox(hotkeyKeyInput);
        hotkeyKeyInput.Items.AddRange(BuildHotkeyKeyOptions());
        hotkeyKeyInput.SelectedItem = hotkeyKeyInput.Items.Contains(settings.HotkeyKey) ? settings.HotkeyKey : "V";
        hotkeyKeyInput.Size = new Size(180, 34);
        hotkeyKeyInput.Location = new Point(LeftColumnX + 408, 142);
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

    private Control BuildSidebarUpdatesCard()
    {
        updateInfoCard.BackColor = Color.FromArgb(16, 20, 31);
        updateInfoCard.Size = new Size(SidebarCardWidth, 308);
        updateInfoCard.Margin = new Padding(0, 0, 0, 0);
        updateInfoCard.Controls.Clear();

        var title = CreateInfoTitleLabel("UPDATES");
        title.Location = new Point(16, 14);

        var summaryLabel = new Label
        {
            Text = "Automatic installs run when a newer GitHub release is available.",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(159, 168, 199),
            MaximumSize = new Size(252, 0),
            AutoSize = true,
            Location = new Point(16, 38)
        };

        var checkButton = CreatePrimaryButton("Check Updates", async (_, _) =>
        {
            SetUpdateStatusText("Checking GitHub...");
            await checkUpdatesAction();
        });
        checkButton.Size = new Size(256, 38);
        checkButton.Location = new Point(16, 82);

        var notesButton = CreateSecondaryButton("Release Notes", (_, _) => showReleaseNotesAction());
        notesButton.Size = new Size(256, 38);
        notesButton.Location = new Point(16, 128);

        var historyButton = CreateSecondaryButton("History", (_, _) => showReleaseHistoryAction());
        historyButton.Size = new Size(256, 38);
        historyButton.Location = new Point(16, 174);

        updateStatusLabel.Text = "Updates are checked automatically in the background.";
        updateStatusLabel.Font = new Font("Segoe UI", 9f);
        updateStatusLabel.ForeColor = Color.FromArgb(150, 159, 190);
        updateStatusLabel.AutoSize = true;
        updateStatusLabel.MaximumSize = new Size(252, 0);
        updateStatusLabel.Location = new Point(16, 220);

        var detailsDivider = new Panel
        {
            BackColor = Color.FromArgb(30, 37, 57),
            Location = new Point(16, 238),
            Size = new Size(256, 1)
        };

        StyleUpdateDetailLabel(currentVersionLabel);
        StyleUpdateDetailLabel(latestVersionLabel);
        StyleUpdateDetailLabel(lastCheckedLabel);
        StyleUpdateDetailLabel(lastUpdatedLabel);
        currentVersionLabel.Location = new Point(16, 248);
        latestVersionLabel.Location = new Point(16, 268);
        lastCheckedLabel.Location = new Point(16, 288);
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

    private Control BuildSidebarSummaryCard(string hotkeyText)
    {
        var card = new Panel
        {
            BackColor = Color.FromArgb(16, 20, 31),
            Size = new Size(SidebarCardWidth, 156),
            Margin = new Padding(0, 0, 0, 14)
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

    private Control BuildFooterBar()
    {
        var card = CreateCardPanel();
        card.Size = new Size(ContentCardWidth, 112);

        var closeButton = CreateSecondaryButton("Hide Window", (_, _) => HideToTray());
        closeButton.Location = new Point(LeftColumnX, 30);

        footerLabel.Text = "Changes apply automatically and are stored locally.";
        footerLabel.Font = new Font("Segoe UI", 9.5f);
        footerLabel.ForeColor = Color.FromArgb(150, 159, 190);
        footerLabel.AutoSize = true;
        footerLabel.Location = new Point(220, 40);

        card.Controls.Add(closeButton);
        card.Controls.Add(footerLabel);
        return card;
    }

    private Panel CreateInfoCard(string title, string value, Label? existingValueLabel, int height)
    {
        var card = new Panel
        {
            BackColor = Color.FromArgb(16, 20, 31),
            Size = new Size(SidebarCardWidth, height),
            Margin = new Padding(0, 0, 0, 16)
        };

        var titleLabel = CreateInfoTitleLabel(title.ToUpperInvariant());
        var valueLabel = existingValueLabel ?? new Label();
        valueLabel.Text = value;
        valueLabel.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        valueLabel.ForeColor = Color.FromArgb(240, 243, 255);
        valueLabel.MaximumSize = new Size(252, 0);
        valueLabel.AutoSize = true;
        valueLabel.Location = new Point(16, 40);

        card.Controls.Add(titleLabel);
        card.Controls.Add(valueLabel);
        return card;
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

    private static Panel CreateCardPanel()
    {
        return new Panel
        {
            BackColor = Color.FromArgb(12, 16, 25),
            Margin = new Padding(0, 0, 0, 18)
        };
    }

    private static Label CreateSectionTitle(string text)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI Semibold", 15f, FontStyle.Bold),
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
        input.Size = new Size(300, 84);
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
        label.MaximumSize = new Size(252, 0);
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
        for (var c = 'A'; c <= 'Z'; c++)
        {
            options.Add(c.ToString());
        }

        for (var i = 1; i <= 12; i++)
        {
            options.Add($"F{i}");
        }

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

    private static Button CreatePrimaryButton(string text, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            Size = new Size(156, 42),
            BackColor = Color.FromArgb(99, 113, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            TabStop = false
        };

        button.FlatAppearance.BorderSize = 0;
        button.Click += onClick;
        return button;
    }

    private static Button CreateSecondaryButton(string text, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            Size = new Size(144, 42),
            BackColor = Color.FromArgb(24, 30, 46),
            ForeColor = Color.FromArgb(233, 237, 255),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
            TabStop = false
        };

        button.FlatAppearance.BorderColor = Color.FromArgb(42, 49, 71);
        button.FlatAppearance.BorderSize = 1;
        button.Click += onClick;
        return button;
    }

    private void UpdateValueLabels()
    {
        minWpmValueLabel.Text = $"Burst floor: {minWpmInput.Value:0} WPM";
        maxWpmValueLabel.Text = $"Burst ceiling: {maxWpmInput.Value:0} WPM";
        typoRateValueLabel.Text = $"Temporary typo frequency: {typoRateInput.Value:0.00}%";
        pauseChanceValueLabel.Text = $"Random pause frequency: {pauseChanceInput.Value:0}%";
        pauseMinValueLabel.Text = $"Minimum pause: {pauseMinInput.Value:0} ms";
        pauseMaxValueLabel.Text = $"Maximum pause: {pauseMaxInput.Value:0} ms";
        footerLabel.Text = "Changes apply automatically and are stored locally.";
    }

    private void ShowSelectedOverlayRelease()
    {
        if (!overlayReleaseList.Visible)
        {
            return;
        }

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
        if (overlayReleaseList.SelectedIndex < 0 || overlayReleaseList.SelectedIndex >= overlayReleases.Count)
        {
            return;
        }

        UpdateService.OpenUrl(overlayReleases[overlayReleaseList.SelectedIndex].ReleasePageUrl);
    }

    private void HandleMinWpmChanged()
    {
        if (syncingWpmInputs)
        {
            UpdateValueLabels();
            return;
        }

        syncingWpmInputs = true;
        if (minWpmInput.Value > maxWpmInput.Value)
        {
            maxWpmInput.Value = minWpmInput.Value;
        }

        syncingWpmInputs = false;
        ApplySettingsFromInputs();
    }

    private void HandleMaxWpmChanged()
    {
        if (syncingWpmInputs)
        {
            UpdateValueLabels();
            return;
        }

        syncingWpmInputs = true;
        if (maxWpmInput.Value < minWpmInput.Value)
        {
            minWpmInput.Value = maxWpmInput.Value;
        }

        syncingWpmInputs = false;
        ApplySettingsFromInputs();
    }

    private void HandlePauseMinChanged()
    {
        if (syncingPauseInputs)
        {
            UpdateValueLabels();
            return;
        }

        syncingPauseInputs = true;
        if (pauseMinInput.Value > pauseMaxInput.Value)
        {
            pauseMaxInput.Value = pauseMinInput.Value;
        }

        syncingPauseInputs = false;
        ApplySettingsFromInputs();
    }

    private void HandlePauseMaxChanged()
    {
        if (syncingPauseInputs)
        {
            UpdateValueLabels();
            return;
        }

        syncingPauseInputs = true;
        if (pauseMaxInput.Value < pauseMinInput.Value)
        {
            pauseMinInput.Value = pauseMaxInput.Value;
        }

        syncingPauseInputs = false;
        ApplySettingsFromInputs();
    }

    private void ApplySettingsFromInputs()
    {
        var settings = BuildSettings();
        saveSettingsAction(settings);
        UpdateHotkeyText($"{settings.HotkeyModifiers}+{settings.HotkeyKey}");
        UpdateValueLabels();
        footerLabel.Text = "Applied automatically.";
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
        {
            if (File.Exists(candidate))
            {
                return new Icon(candidate);
            }
        }

        return null;
    }
}
