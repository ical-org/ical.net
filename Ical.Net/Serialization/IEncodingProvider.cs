//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;

namespace Ical.Net.Serialization;

[Obsolete("This will be removed with no public replacement.")]
internal interface IEncodingProvider
{
    string? Encode(string encoding, byte[] data);
    string? DecodeString(string encoding, string value);
    byte[]? DecodeData(string encoding, string value);
}
