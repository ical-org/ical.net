//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#if NETSTANDARD2_0

// Polyfills for trimming/AOT annotation attributes that netstandard2.0 does not provide.
// The compiler only needs these to exist; on the modern target frameworks the BCL types are used.

namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
internal sealed class RequiresUnreferencedCodeAttribute : Attribute
{
    public RequiresUnreferencedCodeAttribute(string message) => Message = message;

    public string Message { get; }

    public string? Url { get; set; }
}

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
internal sealed class UnconditionalSuppressMessageAttribute : Attribute
{
    public UnconditionalSuppressMessageAttribute(string category, string checkId)
    {
        Category = category;
        CheckId = checkId;
    }

    public string Category { get; }

    public string CheckId { get; }

    public string? Scope { get; set; }

    public string? Target { get; set; }

    public string? MessageId { get; set; }

    public string? Justification { get; set; }
}

#endif
