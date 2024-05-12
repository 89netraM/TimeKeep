using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.Media;

namespace TimeKeep.App.Controls;

public partial class DateTimePicker : UserControl
{
    public static readonly StyledProperty<DateTimeOffset?> DateTimeProperty =
        AvaloniaProperty.Register<DateTimePicker, DateTimeOffset?>(nameof(DateTime), defaultBindingMode: BindingMode.TwoWay);

    public DateTimeOffset? DateTime
    {
        get => GetValue(DateTimeProperty);
        set
        {
            if (IsEnabled)
            {
                SetValue(DateTimeProperty, value);
            }
        }
    }

    public DateTimePicker()
    {
        InitializeComponent();

        SetMonoFont();
    }

    private void SetMonoFont()
    {
        var monoFont = FontManager.Current.SystemFonts
            .FirstOrDefault(f => f.Name.Contains("mono", StringComparison.InvariantCultureIgnoreCase));
        if (monoFont is null)
        {
            return;
        }
        DateBox.FontFamily = monoFont;
        TimeBox.FontFamily = monoFont;
    }

    private void OnDateBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (DateBox is { MaskCompleted: true, Text: not null })
        {
            DateTime = new DateTimeOffset(System.DateTime.Parse(DateBox.Text) + (DateTime?.DateTime.TimeOfDay ?? TimeSpan.Zero), DateTime?.Offset ?? DateTimeOffset.Now.Offset);
        }
    }

    private void OnDatePickerConfirmed(object? sender, EventArgs e)
    {
        DateTime = new DateTimeOffset(DatePicker.Date.DateTime + (DateTime?.DateTime.TimeOfDay ?? TimeSpan.Zero), DateTime?.Offset ?? DateTimeOffset.Now.Offset);
        DateButton.Flyout?.Hide();
    }

    private void OnDatePickerDismissed(object? sender, EventArgs e)
    {
        DateButton.Flyout?.Hide();
    }

    private void OnTimeBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (TimeBox is { MaskCompleted: true, Text: not null })
        {
            DateTime = new DateTimeOffset((DateTime?.Date ?? System.DateTime.Now) + TimeSpan.Parse(TimeBox.Text), DateTime?.Offset ?? DateTimeOffset.Now.Offset);
        }
    }

    private void OnTimePickerConfirmed(object? sender, EventArgs e)
    {
        DateTime = new DateTimeOffset((DateTime?.Date ?? System.DateTime.Now) + TimePicker.Time, DateTime?.Offset ?? DateTimeOffset.Now.Offset);
        TimeButton.Flyout?.Hide();
    }

    private void OnTimePickerDismissed(object? sender, EventArgs e)
    {
        TimeButton.Flyout?.Hide();
    }

    private void OnRefreshTimeTapped(object? sender, TappedEventArgs e)
    {
        DateTime = DateTimeOffset.Now;
    }
}
