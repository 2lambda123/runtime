// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//

using Xunit;
namespace Test
{
    using System;

    public class AA
    {
        [Fact]
        public static void TestEntryPoint()
        {
            try
            {
                try
                {
                    // blah blah blah ...
                }
                finally
                {
                    int[] an = new int[2];
                    an[-1] = 0;
                }
            }
            catch (Exception) { }
        }
    }
}
