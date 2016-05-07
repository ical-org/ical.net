namespace DDay.iCal.Serialization
{
    public interface ICalendarComponentFactory
    {
        ICalendarComponent Build(string objectName, bool uninitialized);
    }
}
