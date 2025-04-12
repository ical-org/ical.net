//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Collections.Generic;

namespace Ical.Net.Collections;

public class MultiLinkedList<TType> :
    List<TType>,
    IMultiLinkedList<TType>
{
    private IMultiLinkedList<TType>? _previous;

    public virtual void SetPrevious(IMultiLinkedList<TType> previous) => _previous = previous;

    public virtual int StartIndex => _previous?.ExclusiveEnd ?? 0;

    public virtual int ExclusiveEnd => Count > 0 ? StartIndex + Count : StartIndex;
}
