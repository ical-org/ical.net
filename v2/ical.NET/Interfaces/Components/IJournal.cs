namespace ical.net.Interfaces.Components
{
    public interface IJournal : IRecurringComponent
    {
        JournalStatus Status { get; set; }
    }
}