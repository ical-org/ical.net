namespace Ical.Net.Interfaces.DataTypes
{
    public interface IStatusCode : IEncodableDataType
    {
        int[] Parts { get; set; }
        int Primary { get; }
        int Secondary { get; }
        int Tertiary { get; }
    }
}