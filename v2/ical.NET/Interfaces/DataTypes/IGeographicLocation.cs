namespace ical.net.Interfaces.DataTypes
{
    public interface IGeographicLocation : IEncodableDataType
    {
        double Latitude { get; set; }
        double Longitude { get; set; }
    }
}