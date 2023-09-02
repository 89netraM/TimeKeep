using Avalonia.Interactivity;
using TimeKeep.RPC.Entries;

namespace TimeKeep.App.Events;

public class EntryDestroyedEventArgs : RoutedEventArgs
{
    public Entry Entry { get; }

    public EntryDestroyedEventArgs(Entry entry)
    {
        Entry = entry;
    }
}
