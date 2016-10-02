using ical.net.DataTypes;

namespace ical.net.Interfaces.DataTypes
{
    public interface IRequestStatus : IEncodableDataType
    {
        string Description { get; set; }
        string ExtraData { get; set; }
        StatusCode StatusCode { get; set; }
    }
}