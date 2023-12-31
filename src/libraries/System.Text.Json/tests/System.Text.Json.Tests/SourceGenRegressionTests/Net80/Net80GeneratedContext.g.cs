﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Source files represent a source generated JsonSerializerContext as produced by the .NET 8 SDK.
// Used to validate correctness of contexts generated by previous SDKs against the current System.Text.Json runtime components.
// Unless absolutely necessary DO NOT MODIFY any of these files -- it would invalidate the purpose of the regression tests.

// <auto-generated/>

#nullable enable annotations
#nullable disable warnings

// Suppress warnings about [Obsolete] member usage in generated code.
#pragma warning disable CS0612, CS0618

namespace System.Text.Json.Tests.SourceGenRegressionTests.Net80
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Text.Json.SourceGeneration", "8.0.9.3103")]
    public partial class Net80GeneratedContext
    {
        private readonly static global::System.Text.Json.JsonSerializerOptions s_defaultOptions = new();

        /// <summary>
        /// The default <see cref="global::System.Text.Json.Serialization.JsonSerializerContext"/> associated with a default <see cref="global::System.Text.Json.JsonSerializerOptions"/> instance.
        /// </summary>
        public static global::System.Text.Json.Tests.SourceGenRegressionTests.Net80.Net80GeneratedContext Default { get; } = new global::System.Text.Json.Tests.SourceGenRegressionTests.Net80.Net80GeneratedContext(new global::System.Text.Json.JsonSerializerOptions(s_defaultOptions));
        
        /// <summary>
        /// The source-generated options associated with this context.
        /// </summary>
        protected override global::System.Text.Json.JsonSerializerOptions? GeneratedSerializerOptions { get; } = s_defaultOptions;
        
        /// <inheritdoc/>
        public Net80GeneratedContext() : base(null)
        {
        }
        
        /// <inheritdoc/>
        public Net80GeneratedContext(global::System.Text.Json.JsonSerializerOptions options) : base(options)
        {
        }

        private static bool TryGetTypeInfoForRuntimeCustomConverter<TJsonMetadataType>(global::System.Text.Json.JsonSerializerOptions options, out global::System.Text.Json.Serialization.Metadata.JsonTypeInfo<TJsonMetadataType> jsonTypeInfo)
        {
            global::System.Text.Json.Serialization.JsonConverter? converter = GetRuntimeConverterForType(typeof(TJsonMetadataType), options);
            if (converter != null)
            {
                jsonTypeInfo = global::System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateValueInfo<TJsonMetadataType>(options, converter);
                return true;
            }
        
            jsonTypeInfo = null;
            return false;
        }
        
        private static global::System.Text.Json.Serialization.JsonConverter? GetRuntimeConverterForType(global::System.Type type, global::System.Text.Json.JsonSerializerOptions options)
        {
            for (int i = 0; i < options.Converters.Count; i++)
            {
                global::System.Text.Json.Serialization.JsonConverter? converter = options.Converters[i];
                if (converter?.CanConvert(type) == true)
                {
                    return ExpandConverter(type, converter, options, validateCanConvert: false);
                }
            }
        
            return null;
        }
        
        private static global::System.Text.Json.Serialization.JsonConverter ExpandConverter(global::System.Type type, global::System.Text.Json.Serialization.JsonConverter converter, global::System.Text.Json.JsonSerializerOptions options, bool validateCanConvert = true)
        {
            if (validateCanConvert && !converter.CanConvert(type))
            {
                throw new global::System.InvalidOperationException(string.Format("The converter '{0}' is not compatible with the type '{1}'.", converter.GetType(), type));
            }
        
            if (converter is global::System.Text.Json.Serialization.JsonConverterFactory factory)
            {
                converter = factory.CreateConverter(type, options);
                if (converter is null || converter is global::System.Text.Json.Serialization.JsonConverterFactory)
                {
                    throw new global::System.InvalidOperationException(string.Format("The converter '{0}' cannot return null or a JsonConverterFactory instance.", factory.GetType()));
                }
            }
        
            return converter;
        }
    }
}
