//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

namespace Ical.Net.Collections;

public interface IGroupedObject<TGroup>
{
    TGroup Group { get; set; }
}