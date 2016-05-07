namespace DDay.iCal
{
    public interface IEncodableDataType : ICalendarDataType
    {
        string Encoding { get; set; }
    }
}
