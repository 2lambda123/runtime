// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace System.DirectoryServices.ActiveDirectory.Tests
{
    [ConditionalClass(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer))]
    public class ForestTests
    {
        [Fact]
        public void GetForest_NullContext_ThrowsArgumentNullException()
        {
            AssertExtensions.Throws<ArgumentNullException>("context", () => Forest.GetForest(null));
        }

        [Theory]
        [InlineData(DirectoryContextType.ApplicationPartition)]
        [InlineData(DirectoryContextType.ConfigurationSet)]
        [InlineData(DirectoryContextType.Domain)]
        public void GetForest_InvalidContextType_ThrowsArgumentException(DirectoryContextType contextType)
        {
            var context = new DirectoryContext(contextType, "Name");
            AssertExtensions.Throws<ArgumentException>("context", () => Forest.GetForest(context));
        }

        [Fact]
        [OuterLoop]
        public void GetForest_NullNameAndNotRootedDomain_ThrowsActiveDirectoryOperationException()
        {
            var context = new DirectoryContext(DirectoryContextType.Forest);

            if (!PlatformDetection.IsDomainJoinedMachine)
                Assert.Throws<ActiveDirectoryOperationException>(() => Forest.GetForest(context));
        }

        [ConditionalTheory(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer), nameof(PlatformDetection.IsNotWindowsIoTCore))]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/34442", TestPlatforms.Windows, TargetFrameworkMonikers.Netcoreapp, TestRuntimes.Mono)]
        [InlineData(DirectoryContextType.DirectoryServer, "\0")]
        [InlineData(DirectoryContextType.Forest, "server:port")]
        public void GetForest_NonNullNameAndNotRootedDomain_ThrowsActiveDirectoryObjectNotFoundException(DirectoryContextType type, string name)
        {
            var context = new DirectoryContext(type, name);
            Assert.Throws<ActiveDirectoryObjectNotFoundException>(() => Forest.GetForest(context));

            // The result of validation is cached, so repeat this to make sure it's cached properly.
            Assert.Throws<ActiveDirectoryObjectNotFoundException>(() => Forest.GetForest(context));
        }

        [ConditionalTheory(typeof(PlatformDetection), nameof(PlatformDetection.IsNotWindowsNanoServer))]
        [ActiveIssue("https://github.com/dotnet/runtime/issues/34442", TestPlatforms.Windows, TargetFrameworkMonikers.Netcoreapp, TestRuntimes.Mono)]
        [OuterLoop("Takes too long on domain joined machines")]
        [InlineData(DirectoryContextType.Forest, "\0")]
        [InlineData(DirectoryContextType.DirectoryServer, "server:port")]
        public void GetForest_NonNullNameAndNotRootedDomain(DirectoryContextType type, string name)
        {
            var context = new DirectoryContext(type, name);
            if (!PlatformDetection.IsDomainJoinedMachine)
            {
                Exception exception = Record.Exception(() => Forest.GetForest(context));
                Assert.NotNull(exception);
                Assert.True(exception is ActiveDirectoryObjectNotFoundException ||
                            exception is ActiveDirectoryOperationException,
                            $"We got unrecognized exception {exception}");


                // The result of validation is cached, so repeat this to make sure it's cached properly.
                exception = Record.Exception(() => Forest.GetForest(context));
                Assert.NotNull(exception);
                Assert.True(exception is ActiveDirectoryObjectNotFoundException ||
                            exception is ActiveDirectoryOperationException,
                            $"We got unrecognized exception {exception}");
            }
        }
    }
}
