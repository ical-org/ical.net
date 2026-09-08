//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Runtime.Serialization;

namespace Ical.Net.Serialization;

/// <summary>
/// Implemented by types that need to be notified when deserialization of the
/// object begins and ends.
/// </summary>
/// <remarks>
/// Dispatch is direct rather than via <see cref="OnDeserializingAttribute"/> and
/// <see cref="OnDeserializedAttribute"/>, so that it survives trimming and NativeAOT.
/// </remarks>
internal interface IDeserializationCallbacks
{
    void OnDeserializing(StreamingContext context);

    void OnDeserialized(StreamingContext context);
}
