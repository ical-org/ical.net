//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Runtime.Serialization;

namespace Ical.Net.Serialization;

internal static class SerializationUtil
{
    public static void OnDeserializing(object obj)
        => (obj as IDeserializationCallbacks)?.OnDeserializing(new StreamingContext());

    public static void OnDeserialized(object obj)
        => (obj as IDeserializationCallbacks)?.OnDeserialized(new StreamingContext());
}
