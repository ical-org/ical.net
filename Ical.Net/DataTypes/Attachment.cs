//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Text;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.DataTypes;

/// <summary>
/// Attachments represent the ATTACH element that can be associated with Alarms, Journals, Todos, and Events. There are two kinds of attachments:
/// 1) A string representing a URI which is typically human-readable, OR
/// 2) A base64-encoded string that can represent anything
/// </summary>
public class Attachment : EncodableDataType
{
    public virtual Uri? Uri { get; set; }
    public virtual byte[]? Data { get; private set; } // private set for CopyFrom

    private Encoding _valueEncoding = System.Text.Encoding.UTF8;
    public virtual Encoding ValueEncoding //NOSONAR
    {
        get => _valueEncoding;
        set => _valueEncoding = value;
    }

    public virtual string? FormatType
    {
        get => Parameters.Get("FMTTYPE");
        set => Parameters.Set("FMTTYPE", value);
    }

    public Attachment() { }

    public Attachment(byte[]? value) : this()
    {
        if (value != null)
        {
            Data = value;
        }
    }

    public Attachment(string? value) : this()
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        var serializer = new AttachmentSerializer();
        var a = serializer.Deserialize(value);
        if (a == null)
        {
            throw new ArgumentException($"{value} is not a valid ATTACH component");
        }

        _valueEncoding = a.ValueEncoding;

        Data = a.Data;
        Uri = a.Uri;
    }

    public override string ToString()
        => Data == null
            ? string.Empty
            : ValueEncoding.GetString(Data);

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        if (obj is not Attachment att) return;
        base.CopyFrom(obj);

        Uri = att.Uri != null ? new Uri(att.Uri.ToString()) : null;
        if (att.Data != null)
        {
            Data = new byte[att.Data.Length];
            Array.Copy(att.Data, Data, att.Data.Length);
        }

        ValueEncoding = att.ValueEncoding;
        FormatType = att.FormatType;
    }
}
