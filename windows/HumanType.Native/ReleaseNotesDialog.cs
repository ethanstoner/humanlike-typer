namespace HumanType.Native;

public sealed class ReleaseNotesDialog : Form
{
    public ReleaseNotesDialog(string title, string subtitle, string notes, string primaryText, Action primaryAction)
    {
        var hasPrimaryAction = !string.IsNullOrWhiteSpace(primaryText) &&
            !primaryText.Equals("Close", StringComparison.OrdinalIgnoreCase);

        AutoScaleMode = AutoScaleMode.None;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(680, 560);
        BackColor = Color.FromArgb(10, 13, 21);
        ForeColor = Color.FromArgb(237, 240, 255);
        Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
        Text = title;

        var headerBand = new Panel
        {
            BackColor = Color.FromArgb(12, 16, 25),
            Dock = DockStyle.Top,
            Height = 100
        };

        var titleLabel = new Label
        {
            Text = title,
            Font = new Font("Segoe UI Semibold", 18f, FontStyle.Bold),
            ForeColor = Color.FromArgb(246, 248, 255),
            AutoSize = true,
            Location = new Point(28, 20)
        };

        var subtitleLabel = new Label
        {
            Text = subtitle,
            Font = new Font("Segoe UI", 10f),
            ForeColor = Color.FromArgb(159, 168, 199),
            MaximumSize = new Size(612, 0),
            AutoSize = true,
            Location = new Point(30, 56)
        };

        var notesFrame = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 18, 28, 12),
            BackColor = Color.FromArgb(10, 13, 21)
        };

        var notesBox = new RichTextBox
        {
            ReadOnly = true,
            DetectUrls = true,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(16, 20, 31),
            ForeColor = Color.FromArgb(233, 237, 255),
            Font = new Font("Segoe UI", 10f),
            Dock = DockStyle.Fill,
            TabStop = false
        };
        RenderReleaseNotes(notesBox, notes);

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 74,
            BackColor = Color.FromArgb(10, 13, 21)
        };

        var closeButton = CreateButton("Close", Color.FromArgb(24, 30, 46), Color.FromArgb(233, 237, 255));
        closeButton.FlatAppearance.BorderColor = Color.FromArgb(42, 49, 71);
        closeButton.FlatAppearance.BorderSize = 1;
        closeButton.Location = hasPrimaryAction ? new Point(380, 16) : new Point(520, 16);
        closeButton.Click += (_, _) => Close();

        footer.Controls.Add(closeButton);

        if (hasPrimaryAction)
        {
            var primaryButton = CreateButton(primaryText, Color.FromArgb(99, 113, 255), Color.White);
            primaryButton.Location = new Point(520, 16);
            primaryButton.Click += (_, _) =>
            {
                primaryAction();
                Close();
            };
            footer.Controls.Add(primaryButton);
        }

        headerBand.Controls.Add(titleLabel);
        headerBand.Controls.Add(subtitleLabel);
        notesFrame.Controls.Add(notesBox);
        Controls.Add(notesFrame);
        Controls.Add(footer);
        Controls.Add(headerBand);
    }

    internal static void RenderReleaseNotes(RichTextBox notesBox, string notes)
    {
        notesBox.Clear();
        var normalizedNotes = string.IsNullOrWhiteSpace(notes)
            ? "No release notes were published for this version."
            : notes.Replace("\r\n", "\n").Trim();

        foreach (var rawLine in normalizedNotes.Split('\n'))
        {
            var line = rawLine.TrimEnd();
            if (line.Length == 0)
            {
                AppendText(notesBox, Environment.NewLine, notesBox.Font, Color.FromArgb(116, 126, 158));
                continue;
            }

            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                AppendText(notesBox, StripMarkdown(line[3..]) + Environment.NewLine, new Font("Segoe UI Semibold", 13f, FontStyle.Bold), Color.FromArgb(246, 248, 255));
                continue;
            }

            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                AppendText(notesBox, StripMarkdown(line[2..]) + Environment.NewLine, new Font("Segoe UI Semibold", 15f, FontStyle.Bold), Color.FromArgb(246, 248, 255));
                continue;
            }

            if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                AppendText(notesBox, "  - ", notesBox.Font, Color.FromArgb(118, 126, 255));
                AppendText(notesBox, StripMarkdown(line[2..]) + Environment.NewLine, notesBox.Font, Color.FromArgb(233, 237, 255));
                continue;
            }

            AppendText(notesBox, StripMarkdown(line) + Environment.NewLine, notesBox.Font, Color.FromArgb(204, 211, 236));
        }

        notesBox.SelectionStart = 0;
        notesBox.SelectionLength = 0;
    }

    internal static void AppendText(RichTextBox notesBox, string text, Font font, Color color)
    {
        notesBox.SelectionStart = notesBox.TextLength;
        notesBox.SelectionLength = 0;
        notesBox.SelectionFont = font;
        notesBox.SelectionColor = color;
        notesBox.AppendText(text);
    }

    internal static string StripMarkdown(string text)
    {
        return text
            .Replace("`", string.Empty)
            .Replace("**", string.Empty)
            .Replace("__", string.Empty)
            .Trim();
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
