using CommunityToolkit.Mvvm.ComponentModel;
using Model.Event;

namespace ViewModel
{
    public partial class HistoryVM : ObservableObject
    {
        [ObservableProperty]
        private List<StorableObjectEvent> storableObjectEvents;

        partial void OnStorableObjectEventsChanged(List<StorableObjectEvent> value)
        {
            History.Clear();
            List<StorableObjectEvent> sortedEvents = value.OrderByDescending(x => x.DateTime).ToList();
            foreach (var @event in sortedEvents)
            {
                History.Add(EventStringBuilder.EventToString(@event));
            }
        }

        [ObservableProperty]
        private List<string> history = [];
    }
}
