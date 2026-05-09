// PromptId.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Prompts;

/// <summary>
/// Stable namespaced identity for a prompt definition.
/// </summary>
public readonly record struct PromptId
{
    private const char NamespaceSeparator = '.';

    public string Value { get; }

    public PromptId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Prompt id cannot be empty or whitespace.", nameof(value));

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            throw new ArgumentException("Prompt id cannot include leading or trailing whitespace.", nameof(value));

        if (!value.Contains(NamespaceSeparator, StringComparison.Ordinal))
            throw new ArgumentException("Prompt id must be namespaced using '.' separators.", nameof(value));

        if (value.Split(NamespaceSeparator, StringSplitOptions.None).Any(segment => segment.Length == 0))
            throw new ArgumentException("Prompt id cannot contain empty namespace segments.", nameof(value));

        Value = value;
    }

    public override string ToString() => Value;
}
