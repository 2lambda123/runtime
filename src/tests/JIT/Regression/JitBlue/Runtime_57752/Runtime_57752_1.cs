// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;
// Generated by Fuzzlyn v1.3 on 2021-08-19 15:47:06
// Run on .NET 6.0.0-dev on X64 Windows
// Seed: 16489483397161801783
// Reduced from 251.9 KiB to 1.7 KiB in 02:46:36
// 
// Assert failure(PID 27840 [0x00006cc0], Thread: 1464 [0x05b8]): Assertion failed '!m_VariableLiveRanges->back().m_EndEmitLocation.Valid()' in 'Program:M51(System.Boolean[],System.UInt16[],System.Boolean[],long,System.Int32[],byref)' during 'Generate code' (IL size 140)
// 
//     File: D:\dev\dotnet\runtime\src\coreclr\jit\codegencommon.cpp Line: 11990
//     Image: D:\dev\Fuzzlyn\Fuzzlyn\publish\windows-x64\Fuzzlyn.exe
// 
// 
public class Runtime_57752_1
{
    internal static I s_rt;
    internal static long s_10;
    internal static bool[] s_27 = new[]{true};
    internal static short s_28;
    internal static bool s_33;
    internal static bool s_53;
    internal static int[][] s_56;
    [Fact]
    public static void TestEntryPoint()
    {
        s_rt = new C();
        var vr9 = new ushort[]{0};
        var vr10 = new bool[]{true};
        var vr11 = new int[]{0};
        M51(s_27, vr9, vr10, 0, vr11, ref s_28);
    }

    internal static void M51(bool[] arg0, ushort[] arg5, bool[] arg9, ulong arg10, int[] arg11, ref short arg12)
    {
        if (arg0[0])
        {
            if (arg0[0])
            {
                arg11[0] = arg11[0];
            }
            else
            {
                try
                {
                }
                finally
                {
                    arg9[0] = arg9[0];
                }

                try
                {
                    arg0[0] = true;
                }
                finally
                {
                    for (int var4 = 0; var4 < 2; var4++)
                    {
                        s_53 = arg0[0];
                        arg10 = 0;
                        ushort var6 = arg5[0];
                        long var8 = 0;
                        s_rt.Write(s_56[0][0]);
                        s_rt.Write(var8);
                    }

                    s_33 = false;
                    try
                    {
                        s_10 = s_10;
                    }
                    finally
                    {
                        arg12 = ref arg12;
                    }
                }
            }

            bool vr1 = arg9[0];
        }
    }
}

public interface I
{
    void Write<T>(T val);
}

public class C : I
{
    public void Write<T>(T val)
    {
    }
}