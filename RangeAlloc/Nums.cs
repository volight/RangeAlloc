using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Volight.Allocators.Internal;

public static class Nums
{
    [MethodImpl(MethodImplOptions.AggressiveInlining
#if !NETSTANDARD
    | MethodImplOptions.AggressiveOptimization
#endif
    )]
    public static bool IsZero<T>(T v) where T : unmanaged => v switch
    {
        sbyte x => x == 0,
        short x => x == 0,
        int x => x == 0,
        long x => x == 0,
        nint x => x == 0,
        byte x => x == 0,
        ushort x => x == 0,
        uint x => x == 0,
        ulong x => x == 0,
        nuint x => x == 0,
#if !NETSTANDARD
        Half x => x == (Half)0,
#endif
        float x => x == 0,
        double x => x == 0,
        decimal x => x == 0,
        _ => throw new NotImplementedException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining
#if !NETSTANDARD
    | MethodImplOptions.AggressiveOptimization
#endif
    )]
    public static T Add<T>(T a, T b) where T : unmanaged => (a, b) switch
    {
        (sbyte x, sbyte y) => (sbyte)(x + y) is T r ? r : throw new NotImplementedException(),
        (short x, short y) => (short)(x + y) is T r ? r : throw new NotImplementedException(),
        (int x, int y) => x + y is T r ? r : throw new NotImplementedException(),
        (long x, long y) => x + y is T r ? r : throw new NotImplementedException(),
        (nint x, nint y) => x + y is T r ? r : throw new NotImplementedException(),
        (byte x, byte y) => (byte)(x + y) is T r ? r : throw new NotImplementedException(),
        (ushort x, ushort y) => (ushort)(x + y) is T r ? r : throw new NotImplementedException(),
        (uint x, uint y) => x + y is T r ? r : throw new NotImplementedException(),
        (ulong x, ulong y) => x + y is T r ? r : throw new NotImplementedException(),
        (nuint x, nuint y) => x + y is T r ? r : throw new NotImplementedException(),
#if !NETSTANDARD
        (Half x, Half y) => (Half)((float)x + (float)y) is T r ? r : throw new NotImplementedException(),
#endif
        (float x, float y) => x + y is T r ? r : throw new NotImplementedException(),
        (double x, double y) => x + y is T r ? r : throw new NotImplementedException(),
        (decimal x, decimal y) => x + y is T r ? r : throw new NotImplementedException(),
        _ => throw new NotImplementedException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining
#if !NETSTANDARD
    | MethodImplOptions.AggressiveOptimization
#endif
    )]
    public static T Sub<T>(T a, T b) where T : unmanaged => (a, b) switch
    {
        (sbyte x, sbyte y) => (sbyte)(x - y) is T r ? r : throw new NotImplementedException(),
        (short x, short y) => (short)(x - y) is T r ? r : throw new NotImplementedException(),
        (int x, int y) => x - y is T r ? r : throw new NotImplementedException(),
        (long x, long y) => x - y is T r ? r : throw new NotImplementedException(),
        (nint x, nint y) => x - y is T r ? r : throw new NotImplementedException(),
        (byte x, byte y) => (byte)(x - y) is T r ? r : throw new NotImplementedException(),
        (ushort x, ushort y) => (ushort)(x - y) is T r ? r : throw new NotImplementedException(),
        (uint x, uint y) => x - y is T r ? r : throw new NotImplementedException(),
        (ulong x, ulong y) => x - y is T r ? r : throw new NotImplementedException(),
        (nuint x, nuint y) => x - y is T r ? r : throw new NotImplementedException(),
#if !NETSTANDARD
        (Half x, Half y) => (Half)((float)x - (float)y) is T r ? r : throw new NotImplementedException(),
#endif
        (float x, float y) => x - y is T r ? r : throw new NotImplementedException(),
        (double x, double y) => x - y is T r ? r : throw new NotImplementedException(),
        (decimal x, decimal y) => x - y is T r ? r : throw new NotImplementedException(),
        _ => throw new NotImplementedException()
    };
}
