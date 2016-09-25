namespace ical.net.Interfaces.DataTypes
{
    public interface IFreeBusyEntry : IPeriod
    {
        FreeBusyStatus Status { get; set; }
    }
}