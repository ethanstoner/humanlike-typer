namespace HumanType.Native;

public sealed class BrandedOptionSelector : FlowLayoutPanel
{
    private readonly List<Button> optionButtons = new();
    private string selectedValue = string.Empty;

    public event EventHandler? SelectedValueChanged;

    public string SelectedValue
    {
        get => selectedValue;
        set
        {
            if (selectedValue == value)
            {
                return;
            }

            selectedValue = value;
            UpdateButtonStates();
            SelectedValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public BrandedOptionSelector()
    {
        AutoSize = false;
        Size = new Size(220, 84);
        WrapContents = true;
        FlowDirection = FlowDirection.LeftToRight;
        BackColor = Color.Transparent;
        Margin = Padding.Empty;
        Padding = Padding.Empty;
    }

    public void SetOptions(IEnumerable<string> options)
    {
        SuspendLayout();
        Controls.Clear();
        optionButtons.Clear();

        foreach (var option in options)
        {
            var button = CreateOptionButton(option);
            optionButtons.Add(button);
            Controls.Add(button);
        }

        UpdateButtonStates();
        ResumeLayout();
    }

    private Button CreateOptionButton(string option)
    {
        var button = new Button
        {
            Text = option,
            AutoSize = false,
            Size = new Size(Math.Max(78, TextRenderer.MeasureText(option, new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)).Width + 28), 34),
            Margin = new Padding(0, 0, 10, 10),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
            TabStop = false,
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 1;
        button.Click += (_, _) => SelectedValue = option;
        return button;
    }

    private void UpdateButtonStates()
    {
        foreach (var button in optionButtons)
        {
            var selected = string.Equals(button.Text, selectedValue, StringComparison.OrdinalIgnoreCase);
            button.BackColor = selected ? Color.FromArgb(99, 113, 255) : Color.FromArgb(24, 30, 46);
            button.ForeColor = selected ? Color.White : Color.FromArgb(233, 237, 255);
            button.FlatAppearance.BorderColor = selected ? Color.FromArgb(128, 139, 255) : Color.FromArgb(42, 49, 71);
        }
    }
}
