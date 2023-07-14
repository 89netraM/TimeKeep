using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;

namespace TimeKeep.App.Controls;

public partial class CategoriesDisplay : UserControl
{
    public static readonly StyledProperty<ICollection<string>?> CategoriesProperty =
        AvaloniaProperty.Register<CategoriesDisplay, ICollection<string>?>(nameof(Categories), defaultBindingMode: BindingMode.TwoWay);

    public ICollection<string>? Categories
    {
        get => GetValue(CategoriesProperty);
        set => SetValue(CategoriesProperty, value);
    }

    public static readonly StyledProperty<bool> CanRemoveProperty =
        AvaloniaProperty.Register<CategoriesDisplay, bool>(nameof(CanRemove));

    public bool CanRemove
    {
        get => GetValue(CanRemoveProperty);
        set => SetValue(CanRemoveProperty, value);
    }

    public CategoriesDisplay()
    {
        InitializeComponent();
    }

    private void OnCategoryTapped(object? sender, TappedEventArgs e)
    {
        if (CanRemove && sender is Control { DataContext: string category })
        {
            Categories?.Remove(category);
        }
    }
}
