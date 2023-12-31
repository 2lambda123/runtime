// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Globalization;
using System.IO;
using Xunit;

class MyException : Exception
{
}

public class Help
{
	public static Exception s_exceptionToThrow;
	public static bool s_matchingException;

	public static Object s_object = new object();
}
public struct Struct<T>
where T: Exception
{
    public void StructInstanceFunctionWithManyArgs(int i, int j, int k, object o)
    {
        try
        {
            throw Help.s_exceptionToThrow;
        }
        catch (T match)
        {
            if (!Help.s_matchingException)
                throw new Exception("This should not have been caught here", match);

            Console.WriteLine("Caught matching " + match.GetType());
        }
        catch (Exception mismatch)
        {
            if (Help.s_matchingException)
                throw new Exception("Should have been caught above", mismatch);

            Console.WriteLine("Expected mismatch " + mismatch.GetType());
        }
    }
}
public class GenericExceptions
{
    public static void StructInstanceFunctionWithManyArgs()
    {
        Help.s_matchingException = true;
        Help.s_exceptionToThrow = new MyException();
        (new Struct<Exception>()).StructInstanceFunctionWithManyArgs(1, 2, 3, Help.s_object);
        (new Struct<MyException>()).StructInstanceFunctionWithManyArgs(1, 2, 3, Help.s_object);
        Help.s_matchingException = false;
        Help.s_exceptionToThrow = new Exception();
        (new Struct<MyException>()).StructInstanceFunctionWithManyArgs(1, 2, 3, Help.s_object);
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    [Fact]
    public static int TestEntryPoint()
    {
        try
        {
            Console.WriteLine("This test checks that we can catch generic exceptions.");
            Console.WriteLine("All exceptions should be handled by the test itself");
            StructInstanceFunctionWithManyArgs();
        }
        catch (Exception)
        {
            Console.WriteLine("Test Failed");
            return -1;
        }
        Console.WriteLine("Test Passed");
        return 100;
    }
}
