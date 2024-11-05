//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

namespace Ical.Net
{
    public interface IMergeable
    {
        /// <summary>
        /// Merges this object with another.
        /// </summary>
        void MergeWith(IMergeable obj);
    }
}
