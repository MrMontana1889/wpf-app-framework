// TreeQuery.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Dev.Core.Tree;

/// <summary>
/// Immutable, composable query object that describes a filter over a
/// <see cref="TreeNodeModel"/> tree.
/// <para>
/// Compile the query to a predicate via <see cref="Compile"/> and pass it to
/// the search engine, or pass the <see cref="TreeQuery"/> directly to
/// <c>TreeViewControl.Search(TreeQuery)</c>.
/// </para>
/// <para>
/// The query compiles to a <c>Func&lt;TreeNodeModel, bool&gt;</c> predicate
/// using cached property-accessor delegates, so there is no additional
/// runtime cost relative to a hand-written predicate. The object form is
/// retained so queries can be stored, logged, and re-evaluated.
/// </para>
/// </summary>
/// <example>
/// <code>
/// var query = TreeQuery.For("Label").Contains("Pipe")
///                      .And(TreeQuery.For("IsChecked").EqualTo(true));
/// treeView.Search(query);
/// </code>
/// </example>
public sealed class TreeQuery
{
    private static readonly ConcurrentDictionary<(Type NodeType, string PropertyPath), Func<object, object?>> PropertyAccessorCache = new();

    private readonly string? _propertyPath;
    private readonly Func<TreeNodeModel, bool>? _predicate;
    private Func<TreeNodeModel, bool>? _compiledPredicate;

    private TreeQuery(string? propertyPath, Func<TreeNodeModel, bool>? predicate = null)
    {
        _propertyPath = propertyPath;
        _predicate = predicate;
    }

    // -----------------------------------------------------------------------
    // Factory methods
    // -----------------------------------------------------------------------

    /// <summary>
    /// Begins a query targeting the named property on <see cref="TreeNodeModel"/>.
    /// The property accessor is compiled and cached on first evaluation.
    /// </summary>
    public static TreeQuery For(string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        return new(propertyName);
    }

    /// <summary>
    /// Creates a query that accepts any node satisfying <paramref name="predicate"/>
    /// directly. Use as an escape hatch when the property-based API is insufficient.
    /// </summary>
    public static TreeQuery Where(Func<TreeNodeModel, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(propertyPath: null, predicate);
    }

    // -----------------------------------------------------------------------
    // String matchers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Matches nodes whose target string property value contains
    /// <paramref name="value"/>.
    /// </summary>
    public TreeQuery Contains(string value, bool caseSensitive = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        return WithPropertyPredicate(propertyValue =>
        {
            if (propertyValue is null)
            {
                return false;
            }

            var text = propertyValue as string ?? Convert.ToString(propertyValue, CultureInfo.InvariantCulture);
            return text is not null && text.Contains(value, comparison);
        });
    }

    /// <summary>
    /// Matches nodes whose target property value equals <paramref name="value"/>.
    /// Uses structural equality for strings and value equality for primitives.
    /// </summary>
    public TreeQuery EqualTo(object value) => WithPropertyPredicate(propertyValue => Equals(propertyValue, value));

    // -----------------------------------------------------------------------
    // Numeric matchers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Matches nodes whose target numeric property is strictly greater than
    /// <paramref name="value"/>.
    /// </summary>
    public TreeQuery GreaterThan(double value) => WithNumericPredicate(number => number > value);

    /// <summary>
    /// Matches nodes whose target numeric property is strictly less than
    /// <paramref name="value"/>.
    /// </summary>
    public TreeQuery LessThan(double value) => WithNumericPredicate(number => number < value);

    /// <summary>
    /// Matches nodes whose target numeric property falls within the inclusive
    /// range [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    public TreeQuery Between(double min, double max)
    {
        if (max < min)
        {
            throw new ArgumentOutOfRangeException(nameof(max), "max must be greater than or equal to min.");
        }

        return WithNumericPredicate(number => number >= min && number <= max);
    }

    // -----------------------------------------------------------------------
    // Boolean matcher
    // -----------------------------------------------------------------------

    /// <summary>
    /// Matches nodes whose target boolean property is <c>true</c>.
    /// Equivalent to <c>EqualTo(true)</c>.
    /// </summary>
    public TreeQuery IsTrue() => EqualTo(true);

    // -----------------------------------------------------------------------
    // Logical combinators
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns a new query that matches when both this query and
    /// <paramref name="other"/> match the same node.
    /// </summary>
    public TreeQuery And(TreeQuery other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Where(node => Compile()(node) && other.Compile()(node));
    }

    /// <summary>
    /// Returns a new query that matches when either this query or
    /// <paramref name="other"/> matches the node.
    /// </summary>
    public TreeQuery Or(TreeQuery other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Where(node => Compile()(node) || other.Compile()(node));
    }

    /// <summary>
    /// Returns a new query that matches when this query does <em>not</em> match.
    /// </summary>
    public TreeQuery Not() => Where(node => !Compile()(node));

    // -----------------------------------------------------------------------
    // Compilation
    // -----------------------------------------------------------------------

    /// <summary>
    /// Compiles the query to a <see cref="Func{TreeNodeModel, Boolean}"/> predicate
    /// using cached property-accessor delegates. The compiled predicate is cached on
    /// the first call and reused on subsequent calls.
    /// </summary>
    public Func<TreeNodeModel, bool> Compile()
    {
        if (_compiledPredicate is not null)
        {
            return _compiledPredicate;
        }

        _compiledPredicate = _predicate
            ?? throw new InvalidOperationException("TreeQuery must have a terminal matcher before compilation.");

        return _compiledPredicate;
    }

    private TreeQuery WithPropertyPredicate(Func<object?, bool> propertyPredicate)
    {
        ArgumentNullException.ThrowIfNull(propertyPredicate);

        if (string.IsNullOrWhiteSpace(_propertyPath))
        {
            throw new InvalidOperationException("Property-based matchers require a property path created via TreeQuery.For(...).");
        }

        return Where(node => propertyPredicate(GetPropertyValue(node, _propertyPath)));
    }

    private TreeQuery WithNumericPredicate(Func<double, bool> numericPredicate)
    {
        ArgumentNullException.ThrowIfNull(numericPredicate);
        return WithPropertyPredicate(propertyValue => TryConvertToDouble(propertyValue, out var number) && numericPredicate(number));
    }

    private static object? GetPropertyValue(TreeNodeModel node, string propertyPath)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyPath);

        var accessor = PropertyAccessorCache.GetOrAdd((node.GetType(), propertyPath), static key => CreatePropertyAccessor(key.NodeType, key.PropertyPath));
        return accessor(node);
    }

    private static Func<object, object?> CreatePropertyAccessor(Type nodeType, string propertyPath)
    {
        var propertySegments = propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (propertySegments.Length == 0)
        {
            throw new ArgumentException("Property path must contain at least one segment.", nameof(propertyPath));
        }

        var propertyChain = new PropertyInfo[propertySegments.Length];
        var currentType = nodeType;

        for (var index = 0; index < propertySegments.Length; index++)
        {
            var property = currentType.GetProperty(propertySegments[index], BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                ?? throw new ArgumentException($"Property '{propertySegments[index]}' was not found on type '{currentType.Name}'.", nameof(propertyPath));

            propertyChain[index] = property;
            currentType = property.PropertyType;
        }

        return node =>
        {
            object? current = node;
            foreach (var property in propertyChain)
            {
                if (current is null)
                {
                    return null;
                }

                current = property.GetValue(current);
            }

            return current;
        };
    }

    private static bool TryConvertToDouble(object? value, out double number)
    {
        switch (value)
        {
            case null:
                number = default;
                return false;
            case byte byteValue:
                number = byteValue;
                return true;
            case sbyte sbyteValue:
                number = sbyteValue;
                return true;
            case short shortValue:
                number = shortValue;
                return true;
            case ushort ushortValue:
                number = ushortValue;
                return true;
            case int intValue:
                number = intValue;
                return true;
            case uint uintValue:
                number = uintValue;
                return true;
            case long longValue:
                number = longValue;
                return true;
            case ulong ulongValue:
                number = ulongValue;
                return true;
            case float floatValue:
                number = floatValue;
                return true;
            case double doubleValue:
                number = doubleValue;
                return true;
            case decimal decimalValue:
                number = (double)decimalValue;
                return true;
            default:
                return double.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture, out number);
        }
    }
}
