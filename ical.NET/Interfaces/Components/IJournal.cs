namespace DDay.iCal
{
    public interface IJournal :
        IRecurringComponent
    {
        JournalStatus Status { get; set; }
    }
}
