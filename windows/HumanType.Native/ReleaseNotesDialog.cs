namespace HumanType.Native;

public sealed class ReleaseNotesDialog : Form
{
    public ReleaseNotesDialog(string title, string subtitle, string notes, string primaryText, Action primaryAction)
    {
        AutoScaleMode = AutoScaleMode.None;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        Size = new Size(680, 560);
        BackColor = Color.FromArgb(10, 13, 21);
        ForeColor = Color.FromArgb(237, 240, 255);
        Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
        Text = title;

        var titleLabel = new Label
        {
            Text = title,
            Font = new Font("Segoe UI Semibold", 18f, FontStyle.Bold),
            ForeColor = Color.FromArgb(246, 248, 255),
            AutoSize = true,
            Location = new Point(28, 24)
        };

        var subtitleLabel = new Label
        {
            Text = subtitle,
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.FromArgb(159, 168, 199),
            MaximumSize = new Size(600, 0),
            AutoSize = true,
            Location = new Point(30, 68)
        };

        var notesBox = new TextBox
        {
            Text = string.IsNullOrWhiteSpace(notes) ? "No release notes were published for this version." : notes,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(16, 20, 31),
            ForeColor = Color.FromArgb(233, 237, 255),
            Font = new Font("Segoe UI", 10f),
            Location = new Point(30, 112),
            Size = new Size(604, 334)
        };

        var primaryButton = CreateButton(primaryText, Color.FromArgb(99, 113, 255), Color.White);
        primaryButton.Location = new Point(352, 468);
        primaryButton.Click += (_, _) =>
        {
            primaryAction();
            Close();
        };

        var closeButton = CreateButton("Close", Color.FromArgb(24, 30, 46), Color.FromArgb(233, 237, 255));
        closeButton.Location = new Point(500, 468);
        closeButton.Click += (_, _) => Close();

        Controls.Add(titleLabel);
        Controls.Add(subtitleLabel);
        Controls.Add(notesBox);
        Controls.Add(primaryButton);
        Controls.Add(closeButton);
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
