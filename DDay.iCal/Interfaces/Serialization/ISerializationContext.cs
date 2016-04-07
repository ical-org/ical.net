namespace DDay.iCal.Serialization
{
    public interface ISerializationContext : 
        IServiceProvider
    {
        void Push(object item);
        object Pop();
        object Peek();        
    }
}
