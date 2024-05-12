using System;
using System.ComponentModel;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Google.Protobuf.WellKnownTypes;
using TimeKeep.App.Events;
using TimeKeep.App.Services;
using TimeKeep.App.ViewModels;
using TimeKeep.App.Views;
using TimeKeep.RPC.Entries;

namespace TimeKeep.App.Controls;

public partial class EntryDisplay : UserControl
{
    private const double TotalAnimationDurationMs = 150.0d;

    public static readonly RoutedEvent<EntryDestroyedEventArgs> EntryDestroyedEvent =
        RoutedEvent.Register<EntryDestroyedEventArgs>(
            "EntryDestroyed", RoutingStrategies.Bubble, typeof(EntryDisplay));

    public static void AddEntryDestroyedHandler(Control element, EventHandler<EntryDestroyedEventArgs> handler)
    {
        element.AddHandler(EntryDestroyedEvent, handler);
    }

    public static readonly RoutedEvent<EntryDestroyedEventArgs> EntryEndedEvent =
        RoutedEvent.Register<EntryDestroyedEventArgs>(
            "EntryEnded", RoutingStrategies.Bubble, typeof(EntryDisplay));

    public static void AddEntryEndedHandler(Control element, EventHandler<EntryEndedEventArgs> handler)
    {
        element.AddHandler(EntryEndedEvent, handler);
    }

    private CancellationTokenSource? animationCancellationTokenSource;

    public EntryDisplay()
    {
        InitializeComponent();
        
        Border.AddHandler(Gestures.PullGestureEvent, OnPullGestureEvent);
        Border.AddHandler(Gestures.PullGestureEndedEvent, OnPullGestureEventEnded);
    }

    #region Display Open/Close

    private void OnPullGestureEvent(object? sender, PullGestureEventArgs e)
    {
        animationCancellationTokenSource?.Cancel();
        
        var offset = Math.Min(e.Delta.X, Controls.Bounds.Width);
        if (Display.RenderTransform is TranslateTransform transform)
        {
            transform.X = -offset;
        }
        else
        {
            Display.RenderTransform = new TranslateTransform(-offset, 0);
        }
    }

    private void OnPullGestureEventEnded(object? sender, PullGestureEndedEventArgs e)
    {
        if (Display.RenderTransform is not TranslateTransform transform)
        {
            return;
        }

        if (Math.Abs(transform.X) < Controls.Bounds.Width * 0.75)
        {
            AnimateClose(transform);
            return;
        }

        if (Math.Abs(transform.X) < Controls.Bounds.Width)
        {
            AnimateOpen(transform);
        }

        PullGestureRecognizer.IsEnabled = false;
    }

	private async void AnimateClose(TranslateTransform transform)
    {
        var animation = new Animation
        {
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Setters = { new Setter { Property = TranslateTransform.XProperty, Value = 0.0d } },
                    Cue = new Cue(1.0d),
                },
            },
            Duration = TimeSpan.FromMilliseconds(TotalAnimationDurationMs * (Math.Abs(transform.X) / Controls.Bounds.Width)),
        };
        animationCancellationTokenSource?.Cancel();
        animationCancellationTokenSource = new CancellationTokenSource();
        await animation.RunAsync(Display, animationCancellationTokenSource.Token);
        PullGestureRecognizer.IsEnabled = true;
    }

    private void AnimateOpen(TranslateTransform transform)
    {
        var animation = new Animation
        {
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Setters = { new Setter { Property = TranslateTransform.XProperty, Value = -Controls.Bounds.Width } },
                    Cue = new Cue(1.0d),
                },
            },
            Duration = TimeSpan.FromMilliseconds(TotalAnimationDurationMs * ((Controls.Bounds.Width - Math.Abs(transform.X)) / Controls.Bounds.Width)),
        };
        animationCancellationTokenSource?.Cancel();
        animationCancellationTokenSource = new CancellationTokenSource();
        animation.RunAsync(Display, animationCancellationTokenSource.Token);
        PullGestureRecognizer.IsEnabled = false;
    }

    private void OnBorderTapped(object? sender, TappedEventArgs e)
    {
        if (Display.RenderTransform is TranslateTransform transform && Math.Abs(Math.Abs(transform.X) - Controls.Bounds.Width) < 0.01d)
        {
            AnimateClose(transform);
        }
    }

    #endregion

    private void OnEditClick(object? sender, RoutedEventArgs e)
	{
		if (DataContext is not Entry entry)
		{
			ShowErrorMessage("No entry");
			return;
		}

        RaiseEvent(new NavigateEventArgs
        {
            Destination = typeof(EditEntryView),
            Arguments = [entry],
            RoutedEvent = Navigation.NavigateEvent,
            Source = this,
        });
	}

    #region Destroy

    private Flyout? DestroyFlyout =>
        Resources.TryGetValue(nameof(DestroyFlyout), out var resource) && resource is Flyout flyout ? flyout : null;
    
    private void OnDestroyClick(object? sender, RoutedEventArgs e)
    {
        DestroyFlyout?.ShowAt(Border);
    }

    private async void OnDestroyConfirmed(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not Entry entry)
            {
                ShowErrorMessage("No entry");
                return;
            }

            var client = await RpcClientFactory.EntriesClient;
            if (client is null)
            {
                ShowErrorMessage("No client");
                return;
            }
            
            var response= await client.DestroyAsync(new DestroyRequest { Id = entry.Id });

            if (response.Status == DestroyStatus.Success)
            {
                RaiseEvent(new EntryDestroyedEventArgs(entry) { RoutedEvent = EntryDestroyedEvent });
            }
            else
            {
                ShowErrorMessage($"Failed to destroy entry: {response.Status}");
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
        finally
        {
            DestroyFlyout?.Hide();
        }
    }

    private void OnDestroyCanceled(object? sender, RoutedEventArgs e)
    {
        DestroyFlyout?.Hide();
    }

    private void OnDestroyFlyoutClosing(object? sender, CancelEventArgs e)
    {
        if (Display.RenderTransform is TranslateTransform transform)
        {
            AnimateClose(transform);
        }
    }

    #endregion

    private void OnEndClick(object? sender, RoutedEventArgs e)
    {
        EndFlyout?.ShowAt(Border);
    }

    #region EndFlyout

    private Flyout? EndFlyout =>
        Resources.TryGetValue(nameof(EndFlyout), out var resource) && resource is Flyout flyout ? flyout : null;

    private DateTimePicker? endDateTimePicker;

    private async void OnEndConfirmed(object? sender, RoutedEventArgs e)
    {
        EndFlyout?.Hide();

        try
        {
            if (DataContext is not Entry entry)
            {
                ShowErrorMessage("No entry");
                return;
            }

            if (endDateTimePicker?.DateTime is null)
            {
                ShowErrorMessage("No end time");
                return;
            }
            var end = endDateTimePicker.DateTime.Value.ToUniversalTime().ToTimestamp();

            var client = await RpcClientFactory.EntriesClient;
            if (client is null)
            {
                ShowErrorMessage("No client");
                return;
            }
            
            var response = await client.SetEndAsync(new SetEndRequest { Id = entry.Id, End = end });

            if (response.Status == SetEndStatus.Success)
            {
                entry.End = end;
                RaiseEvent(new EntryEndedEventArgs(entry) { RoutedEvent = EntryEndedEvent });
            }
            else
            {
                ShowErrorMessage($"Failed to end entry: {response.Status}");
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
        finally
        {
            if (Display.RenderTransform is TranslateTransform transform)
            {
                AnimateClose(transform);
            }
        }
    }

    private void OnEndCanceled(object? sender, RoutedEventArgs e)
    {
        EndFlyout?.Hide();
    }

    private void OnEndFlyoutClosing(object? sender, CancelEventArgs e)
    {
        if (Display.RenderTransform is TranslateTransform transform)
        {
            AnimateClose(transform);
        }
    }

    private void OnEndTimePickerAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        endDateTimePicker = (sender as DateTimePicker)!;
        endDateTimePicker.DateTime = DateTimeOffset.Now;
    }

    #endregion

    #region ErrorFlyout

    private Flyout? ErrorFlyout =>
        Resources.TryGetValue(nameof(ErrorFlyout), out var resource) && resource is Flyout flyout ? flyout : null;

    private string? errorMessage = null;

    private void ShowErrorMessage(string message)
    {
        if (ErrorFlyout is null)
        {
            return;
        }

        errorMessage = message;
        ErrorFlyout.ShowAt(Border);
    }

    private void OnErrorFlyoutOK(object? sender, RoutedEventArgs e)
    {
        ErrorFlyout?.Hide();
    }

    private void OnErrorMessageBlockAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        (sender as TextBlock)!.Text = errorMessage;
    }

    #endregion
}
