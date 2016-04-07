namespace DDay.iCal
{
    public interface IFreeBusyEntry :
        IPeriod
    {
        FreeBusyStatus Status { get; set; }
    }
}
