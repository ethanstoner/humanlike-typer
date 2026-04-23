namespace HumanType.Native;

public sealed class ReleaseHistoryDialog : Form
{
    private readonly RichTextBox notesBox = new();
    private readonly ListBox releaseList = new();
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
        Size = new Size(820, 600);
        BackColor = Color.FromArgb(10, 13, 21);
        ForeColor = Color.FromArgb(237, 240, 255);
        Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
        Text = "Release History";

        var titleLabel = new Label
        {
            Text = "Release History",
            Font = new Font("Segoe UI Semibold", 18f, FontStyle.Bold),
            ForeColor = Color.FromArgb(246, 248, 255),
            AutoSize = true,
            Location = new Point(28, 24)
        };

        var subtitleLabel = new Label
        {
            Text = "Select a release to view its notes.",
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.FromArgb(159, 168, 199),
            AutoSize = true,
            Location = new Point(30, 68)
        };

        releaseList.BackColor = Color.FromArgb(16, 20, 31);
        releaseList.ForeColor = Color.FromArgb(233, 237, 255);
        releaseList.BorderStyle = BorderStyle.FixedSingle;
        releaseList.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        releaseList.Location = new Point(30, 112);
        releaseList.Size = new Size(210, 390);
        releaseList.SelectedIndexChanged += (_, _) => ShowSelectedRelease();

        notesBox.ReadOnly = true;
        notesBox.DetectUrls = true;
        notesBox.ScrollBars = RichTextBoxScrollBars.Vertical;
        notesBox.BorderStyle = BorderStyle.FixedSingle;
        notesBox.BackColor = Color.FromArgb(16, 20, 31);
        notesBox.ForeColor = Color.FromArgb(233, 237, 255);
        notesBox.Font = new Font("Segoe UI", 10f);
        notesBox.Location = new Point(256, 112);
        notesBox.Size = new Size(520, 390);
        notesBox.TabStop = false;

        foreach (var release in releases)
        {
            releaseList.Items.Add($"{release.Version}  {release.Name}");
        }

        Controls.Add(titleLabel);
        Controls.Add(subtitleLabel);
        Controls.Add(releaseList);
        Controls.Add(notesBox);

        if (releaseList.Items.Count > 0)
        {
            releaseList.SelectedIndex = 0;
        }
        else
        {
            ReleaseNotesDialog.RenderReleaseNotes(notesBox, "No GitHub releases were found.");
        }
    }

    private void ShowSelectedRelease()
    {
        if (releaseList.SelectedIndex < 0 || releaseList.SelectedIndex >= releases.Count)
        {
            return;
        }

        var release = releases[releaseList.SelectedIndex];
        ReleaseNotesDialog.RenderReleaseNotes(notesBox, $"# {release.Name}\n\n{release.Notes}");
    }
}
