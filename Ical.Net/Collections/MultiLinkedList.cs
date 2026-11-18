//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;

namespace Ical.Net.Collections;

internal sealed class MultiLinkedList<TType> : List<TType>
{
    private MultiLinkedList<TType>? _previous;

    public void SetPrevious(MultiLinkedList<TType> previous) => _previous = previous;

    public int StartIndex => _previous?.ExclusiveEnd ?? 0;

    public int ExclusiveEnd => Count > 0 ? StartIndex + Count : StartIndex;
}
