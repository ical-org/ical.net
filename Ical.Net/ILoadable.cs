//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;

namespace Ical.Net;

public interface ILoadable
{
    /// <summary>
    /// Gets whether the object has been loaded.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// An event that fires when the object has been loaded.
    /// </summary>
    event EventHandler Loaded;

    /// <summary>
    /// Fires the Loaded event.
    /// </summary>
    void OnLoaded();
}
