namespace Ical.Net.Interfaces.General
{
    public interface ICopyable
    {
        /// <summary>
        /// Copies all relevant fields/properties from
        /// the target object to the current one.
        /// </summary>
        void CopyFrom(ICopyable obj);

        /// <summary>
        /// Returns a copy of the current object, including
        /// all relevent fields/properties, resulting in a
        /// semantically equivalent copy of the object.
        /// (which consequently passes an object.Equals(obj1, obj2)
        /// test).
        /// </summary>
        T Copy<T>();
    }
}