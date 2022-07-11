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
    internal NumBox<T> index;
    internal NumBox<T> length;

    public SpanRange(T index, T length)
    {
        this.index = index;
        this.length = length;
    }

    public T Index => index;
    public T Length => length;

    public bool IsEmpty => Nums.IsZero<T>(length);

    public T Last => Nums.Add<T>(index, length);
    internal NumBox<T> LastB => Last;

    internal NumBox<T> Left => index;
    internal NumBox<T> Right => Last;

    public override bool Equals(object? obj) => obj is SpanRange<T> range && Equals(range);
    public bool Equals(SpanRange<T> other) => index.Equals(other.index) && length.Equals(other.length);
    public override int GetHashCode() => HashCode.Combine(index, length);

    public static bool operator ==(in SpanRange<T> left, in SpanRange<T> right) => left.Equals(right);
    public static bool operator !=(in SpanRange<T> left, in SpanRange<T> right) => !(left == right);

    /// <summary>
    /// left in right ?
    /// </summary>
    public static bool operator <(in SpanRange<T> left, in SpanRange<T> right) => left.index > right.index && left.LastB < right.LastB;
    /// <summary>
    /// left in or eq right ?
    /// </summary>
    public static bool operator <=(in SpanRange<T> left, in SpanRange<T> right) => left.index >= right.index && left.LastB <= right.LastB;
    /// <summary>
    /// right in left ?
    /// </summary>
    public static bool operator >(in SpanRange<T> left, in SpanRange<T> right) => left.index < right.index && left.LastB > right.LastB;
    /// <summary>
    /// right in or eq left ?
    /// </summary>
    public static bool operator >=(in SpanRange<T> left, in SpanRange<T> right) => left.index <= right.index && left.LastB >= right.LastB;

    internal SpanRange<T> SliceLeft(in SpanRange<T> other) => new(index, Nums.Sub<T>(other.index, index));
    internal SpanRange<T> SliceRight(in SpanRange<T> other) => new(other.LastB, Nums.Sub<T>(LastB, other.LastB));

    private string GetDebuggerDisplay() => ToString();
    public override string ToString() => $"{index} .. {LastB}";
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
