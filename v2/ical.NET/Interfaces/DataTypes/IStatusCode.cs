namespace ical.net.Interfaces.DataTypes
{
    public interface IStatusCode : IEncodableDataType
    {
        int[] Parts { get; }
        int Primary { get; }
        int Secondary { get; }
        int Tertiary { get; }
    }
}