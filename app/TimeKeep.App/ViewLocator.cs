using Avalonia.Controls;
using System;
using Avalonia.Controls.Templates;
using TimeKeep.App.ViewModels;

namespace TimeKeep.App;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        var name = data?.GetType().FullName?.Replace("ViewModel", "View");
        if (name is null)
        {
            return null;
        }

        var type = Type.GetType(name);
        if (type is null)
        {
            return new TextBlock { Text = "Not Found: " + name };
        }

        return Activator.CreateInstance(type) as Control;
    }

    public bool Match(object? data) =>
        data is ViewModelBase;
}
