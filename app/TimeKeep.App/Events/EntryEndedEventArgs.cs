using Avalonia.Interactivity;
using TimeKeep.RPC.Entries;

namespace TimeKeep.App.Events;

public class EntryEndedEventArgs : RoutedEventArgs
{
    public Entry Entry { get; }

    public EntryEndedEventArgs(Entry entry)
    {
        Entry = entry;
    }
}
