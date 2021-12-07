// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl;
using Metalama.Framework.Impl.CompileTime;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime.Serializers
{
    public class MetaSerializerTestBase : TestBase
    {
        private protected static CompileTimeProject CreateCompileTimeProject( CompileTimeDomain domain, TestContext testContext, string code )
        {
            var runtimeCompilation = CreateCSharpCompilation(
                code,
                name: "test_A" );

            var compileTimeCompilationBuilder = new CompileTimeCompilationBuilder( testContext.ServiceProvider, domain );
            DiagnosticList diagnosticList = new();

            Assert.True(
                compileTimeCompilationBuilder.TryGetCompileTimeProject(
                    runtimeCompilation,
                    null,
                    Array.Empty<CompileTimeProject>(),
                    diagnosticList,
                    false,
                    CancellationToken.None,
                    out var project ),
                string.Join( "\n", diagnosticList.Select( x => x.ToString() ) ) );

            return project!;
        }

        private protected static Type GetMetaSerializerType( Type type )
        {
            var metaSerializerTypes =
                type.GetNestedTypes()
                    .Where( nestedType => typeof(IMetaSerializer).IsAssignableFrom( nestedType ) )
                    .ToArray();

            Assert.Single( metaSerializerTypes );

            return metaSerializerTypes[0];
        }

        private protected static IMetaSerializer GetMetaSerializer( Type type )
        {
            var metaSerializerType = GetMetaSerializerType( type );

            return (IMetaSerializer) Activator.CreateInstance( metaSerializerType ).AssertNotNull();
        }

        private protected class TestArgumentsReader : IArgumentsReader
        {
            private (string Name, object? Value, string? Scope)[]? _data;

            public virtual bool TryGetValue<T>( string name, [NotNullWhen( true )] out T value, string? scope = null )
            {
                var dataValue =
                    this._data.AssertNotNull()
                        .Select( x => ((string Name, object? Value, string? Scope)?) x )
                        .SingleOrDefault(
                            d => StringComparer.Ordinal.Equals( d.AssertNotNull().Name, name )
                                 && StringComparer.Ordinal.Equals( d.AssertNotNull().Scope, scope ) );

                if ( dataValue == null )
                {
                    value = default!;

                    return false;
                }

                value = (T) dataValue.Value.Value!;

                return true;
            }

            public virtual T GetValue<T>( string name, string? scope = null )
            {
                if ( !this.TryGetValue<T>( name, out var value, scope ) )
                {
                    throw new InvalidOperationException();
                }

                return value;
            }

            public void SetData( params (string Name, object? Value, string? Scope)[] data )
            {
                this._data = data;
            }
        }

        private protected class ThrowingArgumentsWriter : IArgumentsWriter
        {
            public virtual void SetValue( string name, object? value, string? scope = null )
            {
                throw new NotSupportedException();
            }
        }

        private protected class TestArgumentsWriter : IArgumentsWriter
        {
            private List<(string Name, object? Value, string? Scope)> Data { get; } = new();

            public virtual void SetValue( string name, object? value, string? scope = null )
            {
                this.Data.Add( (name, value, scope) );
            }

            public virtual TestArgumentsReader ToReader()
            {
                var reader = new TestArgumentsReader();
                reader.SetData( this.Data.ToArray() );

                return reader;
            }
        }
    }
}