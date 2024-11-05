//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

namespace Ical.Net
{
    public interface ICopyable
    {
        /// <summary>
        /// (Deep) copies all relevant members from
        /// the source object to the current one.
        /// <para/>
        /// If an object cannot use the <see cref="CopyFrom"/> implementation of a base class,
        /// it must override this method and implement the copy logic itself.
        /// </summary>
        void CopyFrom(ICopyable obj);

        /// <summary>
        /// Returns a deep copy of the current object, mostly by using the <see cref="CopyFrom"/> method
        /// of the object when it is overridden, otherwise is used the implementation of the base class.
        /// This is necessary when working with mutable reference types.
        /// </summary>
        T Copy<T>();
    }
}
