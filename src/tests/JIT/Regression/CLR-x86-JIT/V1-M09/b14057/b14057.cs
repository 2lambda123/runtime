// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//

using System;
using Xunit;

public class test
{
    static byte by = 13;
    [Fact]
    public static void TestEntryPoint()
    {
        byte by1 = (byte)(by >> 1);
        byte by2 = (byte)(by >> 1);

        Console.WriteLine(by1);
        Console.WriteLine(by2);
    }
}
