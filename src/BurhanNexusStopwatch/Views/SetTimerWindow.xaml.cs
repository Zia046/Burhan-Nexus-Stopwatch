using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BurhanNexusStopwatch.Services;

namespace BurhanNexusStopwatch.Views;

public partial class SetTimerWindow : Window
{
    private static readonly int[] PresetMinutes = { 1, 2, 3, 4, 5, 10, 15, 30, 60 };

    private readonly List<(Button Button, TimeSpan Duration)> _presets = new();
    private bool _suppressTextChanged;

    public TimeSpan SelectedDuration { get; private set; }
    public bool StartImmediately { get; private set; }

    public SetTimerWindow(TimeSpan current)
    {
        InitializeComponent();
        MaxHeight = SystemParameters.WorkArea.Height - 40;

        foreach (var minutes in PresetMinutes)
        {
            var duration = TimeSpan.FromMinutes(minutes);
            var button = new Button
            {
                Content = minutes == 60 ? "60 min" : $"{minutes} min",
                Style = (Style)FindResource("PresetChip"),
                ToolTip = minutes == 60 ? "1 hour" : $"{minutes} minute{(minutes == 1 ? "" : "s")}",
            };
            button.Click += (_, _) => SetFields(duration);
            button.MouseDoubleClick += (_, _) => { SetFields(duration); Accept(start: true); };
            _presets.Add((button, duration));
            PresetGrid.Children.Add(button);
        }

        foreach (var box in new[] { HoursBox, MinutesBox, SecondsBox })
        {
            box.PreviewTextInput += (_, e) => e.Handled = !e.Text.All(char.IsDigit);
            DataObject.AddPastingHandler(box, OnPaste);
            box.GotKeyboardFocus += (_, _) => box.SelectAll();
            box.PreviewMouseLeftButtonDown += (_, e) =>
            {
                if (!box.IsKeyboardFocusWithin) { e.Handled = true; box.Focus(); }
            };
            box.PreviewMouseWheel += (_, e) => { Step((string)box.Tag, e.Delta > 0 ? 1 : -1); e.Handled = true; };
            box.PreviewKeyDown += (_, e) =>
            {
                if (e.Key == Key.Up) { Step((string)box.Tag, 1); e.Handled = true; }
                else if (e.Key == Key.Down) { Step((string)box.Tag, -1); e.Handled = true; }
            };
            box.TextChanged += (_, _) => { if (!_suppressTextChanged) OnFieldsEdited(); };
            box.LostKeyboardFocus += (_, _) => Normalize();
        }

        SetFields(current);
        Loaded += (_, _) => MinutesBox.Focus();
    }

    private static void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        var text = e.DataObject.GetData(DataFormats.UnicodeText) as string;
        if (string.IsNullOrEmpty(text) || !text.All(char.IsDigit)) e.CancelCommand();
    }

    private static int Read(TextBox box) => int.TryParse(box.Text, out var v) ? v : 0;

    private TimeSpan ReadFields()
    {
        // Allows inputs like 90 minutes; the total is normalised afterwards.
        long total = Read(HoursBox) * 3600L + Read(MinutesBox) * 60L + Read(SecondsBox);
        return TimeSpan.FromSeconds(total);
    }

    private void SetFields(TimeSpan d)
    {
        if (d > CountdownEngine.MaxDuration) d = CountdownEngine.MaxDuration;
        if (d < TimeSpan.Zero) d = TimeSpan.Zero;
        _suppressTextChanged = true;
        HoursBox.Text = ((int)d.TotalHours).ToString("00");
        MinutesBox.Text = d.Minutes.ToString("00");
        SecondsBox.Text = d.Seconds.ToString("00");
        _suppressTextChanged = false;
        OnFieldsEdited();
    }

    private void Normalize() => SetFields(ReadFields());

    private void OnFieldsEdited()
    {
        ErrorText.Text = string.Empty;
        var d = ReadFields();
        foreach (var (button, duration) in _presets)
            button.Tag = duration == d ? "Selected" : null;
    }

    private void Step(string unit, int direction)
    {
        var d = ReadFields();
        var step = unit.StartsWith('H') ? TimeSpan.FromHours(1)
                 : unit.StartsWith('M') ? TimeSpan.FromMinutes(1)
                 : TimeSpan.FromSeconds(1);
        d += direction > 0 ? step : -step;
        if (d < TimeSpan.Zero) d = TimeSpan.Zero;
        SetFields(d);
    }

    private void Spin_Click(object sender, RoutedEventArgs e)
    {
        var tag = (string)((Button)sender).Tag;
        Step(tag, tag.EndsWith('+') ? 1 : -1);
    }

    private void Accept(bool start)
    {
        var d = ReadFields();
        if (d <= TimeSpan.Zero)
        {
            ErrorText.Text = "Please set a time greater than zero.";
            return;
        }
        if (d > CountdownEngine.MaxDuration)
        {
            ErrorText.Text = "Maximum timer is 99:59:59.";
            return;
        }
        SelectedDuration = d;
        StartImmediately = start;
        DialogResult = true;
    }

    private void Set_Click(object sender, RoutedEventArgs e) => Accept(start: false);
    private void SetAndStart_Click(object sender, RoutedEventArgs e) => Accept(start: true);
    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
