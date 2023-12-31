// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//

using System;
using System.Runtime.CompilerServices;
using Xunit;

// Generated by Fuzzlyn v1.5 on 2023-06-15 11:50:35
// Run on X64 Windows
// Seed: 1595496264005900603
// Reduced from 20.9 KiB to 0.9 KiB in 00:00:18
// Debug: Outputs 0
// Release: Outputs 1
public struct S0
{
    public bool F0;
    public short F1;
    public byte F2;
    public uint F3;
    public uint F4;
    public short F5;
    public sbyte F6;
    public bool F7;
    public S0(bool f0, short f1, byte f2, uint f3, uint f4, short f5, sbyte f6, bool f7)
    {
        F0 = f0;
        F1 = f1;
        F2 = f2;
        F3 = f3;
        F4 = f4;
        F5 = f5;
        F6 = f6;
        F7 = f7;
    }
}

public struct S1
{
    public S0 F1;
    public S0 F2;
}

public class Runtime_87614
{
    [Fact]
    public static int TestEntryPoint()
    {
        var vr1 = new S1();
        int result = M3(vr1);
        if (result != 0)
        {
            Console.WriteLine("FAIL: result is {0}", result);
        }
        return result == 0 ? 100 : 101;
    }

    public static int M3(S1 arg0)
    {
        arg0.F1 = new S0(arg0.F2.F0, 1, 0, arg0.F1.F4, arg0.F1.F4, arg0.F1.F1, 0, true);
        Consume(arg0.F1.F0);
        Consume(arg0.F1.F1);
        Consume(arg0.F1.F4);
        return arg0.F1.F5;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Consume<T>(T value)
    {
    }
}
