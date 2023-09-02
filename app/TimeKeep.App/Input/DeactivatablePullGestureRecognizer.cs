using Avalonia;
using Avalonia.Input;

namespace TimeKeep.App.Input;

public class DeactivatablePullGestureRecognizer : PullGestureRecognizer
{
    public static readonly StyledProperty<bool> IsEnabledProperty =
        AvaloniaProperty.Register<DeactivatablePullGestureRecognizer, bool>(nameof(IsEnabled), defaultValue: true);

    public bool IsEnabled
    {
        get => GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    public DeactivatablePullGestureRecognizer() { }

    public DeactivatablePullGestureRecognizer(PullDirection direction) : base(direction) { }

    protected override void PointerPressed(PointerPressedEventArgs e)
    {
        if (IsEnabled)
        {
            base.PointerPressed(e);
        }
    }
}
