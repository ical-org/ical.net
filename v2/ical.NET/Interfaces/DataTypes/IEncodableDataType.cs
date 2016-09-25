namespace ical.net.Interfaces.DataTypes
{
    public interface IEncodableDataType : ICalendarDataType
    {
        string Encoding { get; set; }
    }
}