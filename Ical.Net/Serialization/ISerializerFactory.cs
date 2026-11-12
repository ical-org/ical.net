//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;

namespace Ical.Net.Serialization;

[Obsolete("This will be removed with no public replacement.")]
public interface ISerializerFactory
{
    ISerializer? Build(Type objectType, SerializationContext ctx);
}
