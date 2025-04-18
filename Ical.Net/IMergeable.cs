//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
namespace Ical.Net;

public interface IMergeable
{
    /// <summary>
    /// Merges this object with another.
    /// </summary>
    void MergeWith(IMergeable obj);
}
