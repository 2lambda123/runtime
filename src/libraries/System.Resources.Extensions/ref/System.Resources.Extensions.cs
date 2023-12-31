// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// ------------------------------------------------------------------------------
// Changes to this file must follow the https://aka.ms/api-review process.
// ------------------------------------------------------------------------------

namespace System.Resources.Extensions
{
    public sealed partial class DeserializingResourceReader : System.Collections.IEnumerable, System.IDisposable, System.Resources.IResourceReader
    {
        public DeserializingResourceReader(System.IO.Stream stream) { }
        public DeserializingResourceReader(string fileName) { }
        public void Close() { }
        public void Dispose() { }
        public System.Collections.IDictionaryEnumerator GetEnumerator() { throw null; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw null; }
    }
    public sealed partial class PreserializedResourceWriter : System.IDisposable, System.Resources.IResourceWriter
    {
        public PreserializedResourceWriter(System.IO.Stream stream) { }
        public PreserializedResourceWriter(string fileName) { }
        public void AddActivatorResource(string name, System.IO.Stream value, string typeName, bool closeAfterWrite = false) { }
        [System.ObsoleteAttribute("BinaryFormatter serialization is obsolete and should not be used. See https://aka.ms/binaryformatter for more information.", DiagnosticId = "SYSLIB0011", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
        public void AddBinaryFormattedResource(string name, byte[] value, string? typeName = null) { }
        public void AddResource(string name, byte[]? value) { }
        public void AddResource(string name, System.IO.Stream? value, bool closeAfterWrite = false) { }
        public void AddResource(string name, object? value) { }
        public void AddResource(string name, string? value) { }
        public void AddResource(string name, string value, string typeName) { }
        public void AddTypeConverterResource(string name, byte[] value, string typeName) { }
        public void Close() { }
        public void Dispose() { }
        public void Generate() { }
    }
}
