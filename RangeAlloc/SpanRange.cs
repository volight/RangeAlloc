using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Volight.Allocators.Internal;

namespace Volight.Allocators;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public struct SpanRange<T> : IEquatable<SpanRange<T>> where T : unmanaged, IEquatable<T>, IComparable<T>
{
    internal NumBox<T> left;
    internal NumBox<T> right;

    public SpanRange(T left, T right) : this(new NumBox<T>(left), new NumBox<T>(right)) { }
    internal SpanRange(NumBox<T> left, NumBox<T> right)
    {
        this.left = left;
        this.right = right;
    }

    public T Left => left;
    public T Right => right;

    public T Length => Nums.Sub(right, left);

    public bool IsEmpty => left == right;

    public override bool Equals(object? obj) => obj is SpanRange<T> range && Equals(range);
    public bool Equals(SpanRange<T> other) => left.Equals(other.left) && right.Equals(other.right);
    public override int GetHashCode() => HashCode.Combine(left, right);

    public static bool operator ==(in SpanRange<T> left, in SpanRange<T> right) => left.left.Equals(right.left) && left.right.Equals(right.right);
    public static bool operator !=(in SpanRange<T> left, in SpanRange<T> right) => !(left == right);

    /// <summary>
    /// left in right ?
    /// </summary>
    public static bool operator <(in SpanRange<T> left, in SpanRange<T> right) => left.left > right.left && left.right < right.right;
    /// <summary>
    /// left in or eq right ?
    /// </summary>
    public static bool operator <=(in SpanRange<T> left, in SpanRange<T> right) => left.left >= right.left && left.right <= right.right;
    /// <summary>
    /// right in left ?
    /// </summary>
    public static bool operator >(in SpanRange<T> left, in SpanRange<T> right) => left.left < right.left && left.right > right.right;
    /// <summary>
    /// right in or eq left ?
    /// </summary>
    public static bool operator >=(in SpanRange<T> left, in SpanRange<T> right) => left.left <= right.left && left.right >= right.right;

    internal SpanRange<T> SliceLeft(in SpanRange<T> other) => new(left, new NumBox<T>(Nums.Sub<T>(other.left, left)));
    internal SpanRange<T> SliceRight(in SpanRange<T> other) => new(other.right, new NumBox<T>(Nums.Sub<T>(right, other.right)));

    private string GetDebuggerDisplay() => ToString();
    public override string ToString() => $"{left} .. {right}";
}

public static class SpanRange
{
    /// <summary>
    /// New by index length
    /// </summary>
    public static SpanRange<T> FromLength<T>(T index, T length) where T : unmanaged, IEquatable<T>, IComparable<T>
        => new(index, Nums.Add(index, length));

    /// <summary>
    /// l1 &lt; r1 &amp;&amp; l2 &lt; r2
    /// </summary>
    public static bool Lt<T>(this in SpanRange<T> left, in SpanRange<T> right) where T : unmanaged, IEquatable<T>, IComparable<T>
        => left.left < right.left && left.right < right.right;

    /// <summary>
    /// l1 &lt;= r1 &amp;&amp; l2 &lt;= r2
    /// </summary>
    public static bool Le<T>(this in SpanRange<T> left, in SpanRange<T> right) where T : unmanaged, IEquatable<T>, IComparable<T>
        => left.left <= right.left && left.right <= right.right;

    /// <summary>
    /// l1 &gt; r1 &amp;&amp; l2 &gt; r2
    /// </summary>
    public static bool Gt<T>(this in SpanRange<T> left, in SpanRange<T> right) where T : unmanaged, IEquatable<T>, IComparable<T>
        => left.left > right.left && left.right > right.right;

    /// <summary>
    /// l1 &gt;= r1 &amp;&amp; l2 &gt;= r2
    /// </summary>
    public static bool Ge<T>(this in SpanRange<T> left, in SpanRange<T> right) where T : unmanaged, IEquatable<T>, IComparable<T>
        => left.left >= right.left && left.right >= right.right;
}

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal struct NumBox<T> : IEquatable<T>, IComparable<T> where T : unmanaged, IEquatable<T>, IComparable<T>
{
    public T Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NumBox(T value) => Value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator NumBox<T>(T value) => new(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T(NumBox<T> box) => box.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => Value.GetHashCode();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string? ToString() => Value.ToString();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string? GetDebuggerDisplay() => Value.ToString();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(T other) => other.Equals(Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is NumBox<T> range && Equals(range);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(T other) => Value.CompareTo(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(NumBox<T> left, T right) => left.CompareTo(right) < 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(NumBox<T> left, T right) => left.CompareTo(right) <= 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(NumBox<T> left, T right) => left.CompareTo(right) > 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(NumBox<T> left, T right) => left.CompareTo(right) >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(NumBox<T> left, NumBox<T> right) => left.Value.Equals(right.Value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(NumBox<T> left, NumBox<T> right) => !(left == right);
}
