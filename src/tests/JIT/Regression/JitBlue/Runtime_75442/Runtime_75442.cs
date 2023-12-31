// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Generated by Fuzzlyn v1.5 on 2023-01-16 17:01:38
// Run on X64 Windows
// Seed: 10178493004625664135
// Reduced from 625.1 KiB to 1.5 KiB in 00:03:14
// Debug: Outputs 0
// Release: Outputs 1
using Xunit;
using System;
using System.Runtime.CompilerServices;

public struct S0
{
    public bool F1;
}

public struct S1
{
    public S0 F2;
}

public class Runtime_75442
{
    public static ushort s_19;
    public static S1 s_21;
    public static ushort[] s_32;

    [Fact]
    public static int TestEntryPoint()
    {
        ulong[] vr0 = new ulong[] { 0 };
        for (int vr1 = 0; vr1 < UpperBound(); vr1++)
        {
            vr0[0] = 1;
            if (s_21.F2.F1)
            {
                s_32[0] = s_19;
            }

            vr0[0] ^= vr0[0];
            Use(vr1);
        }

        if (vr0[0] != 0)
        {
            Console.WriteLine("FAIL: vr0[0] == {0}", vr0[0]);
            return 101;
        }

        return 100;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int UpperBound() => 2;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Use(int val)
    {
    }
}
