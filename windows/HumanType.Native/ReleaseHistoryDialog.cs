namespace HumanType.Native;

public sealed class ReleaseHistoryDialog : Form
{
    private readonly RichTextBox notesBox = new();
    private readonly ListBox releaseList = new();
    private readonly Label subtitleLabel = new();
    private readonly Button viewReleaseButton = new();
    private readonly IReadOnlyList<ReleaseNoteItem> releases;

    public ReleaseHistoryDialog(IReadOnlyList<ReleaseNoteItem> releases)
    {
        this.releases = releases;

        AutoScaleMode = AutoScaleMode.None;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(820, 600);
        BackColor = Color.FromArgb(10, 13, 21);
        ForeColor = Color.FromArgb(237, 240, 255);
        Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
        Text = "Release History";

        var headerBand = new Panel
        {
            BackColor = Color.FromArgb(12, 16, 25),
            Dock = DockStyle.Top,
            Height = 100
        };

        var titleLabel = new Label
        {
            Text = "Release History",
            Font = new Font("Segoe UI Semibold", 18f, FontStyle.Bold),
            ForeColor = Color.FromArgb(246, 248, 255),
            AutoSize = true,
            Location = new Point(28, 20)
        };

        subtitleLabel.Text = "Select a release to view its notes.";
        subtitleLabel.Font = new Font("Segoe UI", 10f);
        subtitleLabel.ForeColor = Color.FromArgb(159, 168, 199);
        subtitleLabel.AutoSize = true;
        subtitleLabel.MaximumSize = new Size(752, 0);
        subtitleLabel.Location = new Point(30, 56);

        var content = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 18, 28, 12),
            BackColor = Color.FromArgb(10, 13, 21)
        };

        releaseList.BackColor = Color.FromArgb(16, 20, 31);
        releaseList.ForeColor = Color.FromArgb(233, 237, 255);
        releaseList.BorderStyle = BorderStyle.FixedSingle;
        releaseList.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        releaseList.Location = new Point(0, 0);
        releaseList.Size = new Size(220, 396);
        releaseList.SelectedIndexChanged += (_, _) => ShowSelectedRelease();

        notesBox.ReadOnly = true;
        notesBox.DetectUrls = true;
        notesBox.ScrollBars = RichTextBoxScrollBars.Vertical;
        notesBox.BorderStyle = BorderStyle.FixedSingle;
        notesBox.BackColor = Color.FromArgb(16, 20, 31);
        notesBox.ForeColor = Color.FromArgb(233, 237, 255);
        notesBox.Font = new Font("Segoe UI", 10f);
        notesBox.Location = new Point(236, 0);
        notesBox.Size = new Size(528, 396);
        notesBox.TabStop = false;

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 74,
            BackColor = Color.FromArgb(10, 13, 21)
        };

        var closeButton = CreateButton("Close", Color.FromArgb(24, 30, 46), Color.FromArgb(233, 237, 255));
        closeButton.FlatAppearance.BorderColor = Color.FromArgb(42, 49, 71);
        closeButton.FlatAppearance.BorderSize = 1;
        closeButton.Location = new Point(520, 16);
        closeButton.Click += (_, _) => Close();

        viewReleaseButton.Text = "View Release";
        viewReleaseButton.Size = new Size(132, 42);
        viewReleaseButton.BackColor = Color.FromArgb(99, 113, 255);
        viewReleaseButton.ForeColor = Color.White;
        viewReleaseButton.FlatStyle = FlatStyle.Flat;
        viewReleaseButton.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        viewReleaseButton.FlatAppearance.BorderSize = 0;
        viewReleaseButton.Location = new Point(660, 16);
        viewReleaseButton.Click += (_, _) => OpenSelectedRelease();

        foreach (var release in releases)
        {
            releaseList.Items.Add($"{release.Version}  {release.Name}");
        }

        headerBand.Controls.Add(titleLabel);
        headerBand.Controls.Add(subtitleLabel);
        content.Controls.Add(releaseList);
        content.Controls.Add(notesBox);
        footer.Controls.Add(closeButton);
        footer.Controls.Add(viewReleaseButton);
        Controls.Add(content);
        Controls.Add(footer);
        Controls.Add(headerBand);

        if (releaseList.Items.Count > 0)
        {
            releaseList.SelectedIndex = 0;
        }
        else
        {
            ReleaseNotesDialog.RenderReleaseNotes(notesBox, "No GitHub releases were found.");
            viewReleaseButton.Enabled = false;
        }
    }

    private void ShowSelectedRelease()
    {
        if (releaseList.SelectedIndex < 0 || releaseList.SelectedIndex >= releases.Count)
        {
            subtitleLabel.Text = "Select a release to view its notes.";
            viewReleaseButton.Enabled = false;
            return;
        }

        var release = releases[releaseList.SelectedIndex];
        subtitleLabel.Text = $"Viewing {release.Version}. Open the full GitHub release page if you need release assets.";
        ReleaseNotesDialog.RenderReleaseNotes(notesBox, $"# {release.Name}\n\n{release.Notes}");
        viewReleaseButton.Enabled = !string.IsNullOrWhiteSpace(release.ReleasePageUrl);
    }

    private void OpenSelectedRelease()
    {
        if (releaseList.SelectedIndex < 0 || releaseList.SelectedIndex >= releases.Count)
        {
            return;
        }

        UpdateService.OpenUrl(releases[releaseList.SelectedIndex].ReleasePageUrl);
    }

    private static Button CreateButton(string text, Color backColor, Color foreColor)
    {
        var button = new Button
        {
            Text = text,
            Size = new Size(132, 42),
            BackColor = backColor,
            ForeColor = foreColor,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold)
        };

        button.FlatAppearance.BorderSize = 0;
        return button;
    }
}
