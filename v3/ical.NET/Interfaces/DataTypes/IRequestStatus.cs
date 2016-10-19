using Ical.Net.DataTypes;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface IRequestStatus : IEncodableDataType
    {
        string Description { get; set; }
        string ExtraData { get; set; }
        StatusCode StatusCode { get; set; }
    }
}