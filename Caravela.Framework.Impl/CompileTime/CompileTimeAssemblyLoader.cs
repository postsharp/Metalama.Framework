// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.CompileTime
{
    internal sealed class CompileTimeAssemblyLoader : IDisposable, ICompileTimeTypeResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CSharpCompilation _compilation;

        public CompileTimeAssemblyBuilder CompileTimeAssemblyBuilder { get; }

        private readonly Dictionary<IAssemblySymbol, Assembly?> _assemblyMap = new();
        private readonly Dictionary<string, byte[]?> _assemblyBytesMap = new();
        private readonly AttributeDeserializer _attributeDeserializer;

        private CompileTimeAssemblyLoader(
            IServiceProvider serviceProvider,
            CSharpCompilation compilation,
            CompileTimeAssemblyBuilder compileTimeAssemblyBuilder )
        {
            this._serviceProvider = serviceProvider;
            this._compilation = compilation;
            this.CompileTimeAssemblyBuilder = compileTimeAssemblyBuilder;

            // TODO: this is probably not enough
            this._assemblyMap.Add( compilation.ObjectType.ContainingAssembly, typeof(object).Assembly );

            this._attributeDeserializer = new AttributeDeserializer( this );
            AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;
        }

        public static CompileTimeAssemblyLoader Create(
            IServiceProvider serviceProvider,
            CSharpCompilation compilation )
        {
            CompileTimeAssemblyBuilder builder = new( serviceProvider );
            CompileTimeAssemblyLoader loader = new( serviceProvider, compilation, builder );
            builder.CompileTimeAssemblyLoader = loader;

            return loader;
        }

        private static IEnumerable<ITypeSymbol> CollectTypeArguments( INamedTypeSymbol? s )
        {
            var typeArguments = new List<ITypeSymbol>();

            while ( s != null )
            {
                typeArguments.InsertRange( 0, s.TypeArguments );

                s = s.ContainingSymbol as INamedTypeSymbol;
            }

            return typeArguments;
        }

        private static Assembly Load( byte[] assembly )
        {
            // TODO: use AssemblyLoadContext on .Net Core? (requires multi-targeting)
            return Assembly.Load( assembly );
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= this.CurrentDomain_AssemblyResolve;
        }

        // TODO: perf
        private Assembly? CurrentDomain_AssemblyResolve( object sender, ResolveEventArgs args )
        {
            if ( !this._assemblyMap.ContainsValue( args.RequestingAssembly ) )
            {
                return null;
            }

            var reference = this._compilation.References.SingleOrDefault(
                r =>
                {
                    var assemblyName = new AssemblyName( args.Name ).Name;

                    return r is PortableExecutableReference { FilePath: not null } peReference
                           && Path.GetFileNameWithoutExtension( peReference.FilePath ) == assemblyName;
                } );

            if ( reference == null )
            {
                return null;
            }

            if ( this._compilation.GetAssemblyOrModuleSymbol( reference ) is not IAssemblySymbol symbol )
            {
                return null;
            }

            return this.LoadCompileTimeAssembly( symbol );
        }

        public bool TryCreateAttributeInstance( IAttribute attribute, IDiagnosticAdder diagnosticAdder, [NotNullWhen( true )] out Attribute? attributeInstance )
        {
            return this._attributeDeserializer.TryCreateAttribute( attribute, diagnosticAdder, out attributeInstance );
        }

        public Type? GetCompileTimeType( ITypeSymbol typeSymbol, bool fallbackToMock )
        {
            if ( typeSymbol is IArrayTypeSymbol arrayType )
            {
                var elementType = this.GetCompileTimeType( arrayType.ElementType, fallbackToMock );

                if ( arrayType.IsSZArray )
                {
                    return elementType?.MakeArrayType();
                }

                return elementType?.MakeArrayType( arrayType.Rank );
            }

            var assemblySymbol = typeSymbol.ContainingAssembly;

            var assembly = this.LoadCompileTimeAssembly( assemblySymbol );

            var result = assembly?.GetType( ReflectionNameHelper.GetReflectionName( typeSymbol ) );

            if ( result == null )
            {
                if ( fallbackToMock )
                {
                    result = new CompileTimeType( typeSymbol );
                }
                else
                {
                    return null;
                }
            }

            if ( typeSymbol is INamedTypeSymbol { IsGenericType: true, IsUnboundGenericType: false } namedTypeSymbol )
            {
                var typeArguments = CollectTypeArguments( namedTypeSymbol );

                result = result.MakeGenericType( typeArguments.Select( typeSymbol1 => this.GetCompileTimeType( typeSymbol1, fallbackToMock ) ).ToArray() );
            }

            return result;
        }

        public Assembly? LoadCompileTimeAssembly( IAssemblySymbol assemblySymbol )
        {
            DiagnosticList diagnostics = new();

            if ( !this.TryLoadCompileTimeAssembly( assemblySymbol, diagnostics, out var assembly ) )
            {
                throw new InvalidUserCodeException( "Cannot compile the compile-time project.", diagnostics.ToImmutableArray() );
            }

            return assembly;
        }

        private static byte[]? ReadCompileTimeBytesFromRunTimeFile( string assemblyPath, string resourceName )
        {
            var resolver = new PathAssemblyResolver( new[] { typeof(object).Assembly.Location } );
            using var mlc = new MetadataLoadContext( resolver, typeof(object).Assembly.GetName().Name );

            // LoadFromAssemblyPath throws for mscorlib
            if ( Path.GetFileNameWithoutExtension( assemblyPath ) == typeof(object).Assembly.GetName().Name )
            {
                return null;
            }

            var runtimeAssembly = mlc.LoadFromAssemblyPath( assemblyPath );

            if ( runtimeAssembly.GetManifestResourceNames().Contains( resourceName ) )
            {
                using var resourceStream = runtimeAssembly.GetManifestResourceStream( resourceName );

                if ( resourceName == null )
                {
                    throw new FileNotFoundException();
                }

                var memoryStream = new MemoryStream( (int) resourceStream!.Length );
                resourceStream.CopyTo( memoryStream );

                return memoryStream.ToArray();
            }

            return null;
        }

        public byte[]? GetCompileTimeAssemblyBytes( string path )
        {
            if ( this._assemblyBytesMap.TryGetValue( path, out var assemblyBytes ) )
            {
                return assemblyBytes;
            }

            assemblyBytes = ReadCompileTimeBytesFromRunTimeFile( path, CompileTimeAssemblyBuilder.ResourceName );

            this._assemblyBytesMap.Add( path, assemblyBytes );

            return assemblyBytes;
        }

        private bool TryGetCompileTimeAssemblyBytes( IAssemblySymbol assemblySymbol, IDiagnosticAdder diagnosticSink, out byte[]? compileTimeAssemblyBytes )
        {
            MemoryStream? assemblyStream;

            if ( assemblySymbol is ISourceAssemblySymbol sourceAssemblySymbol )
            {
                if ( !this.CompileTimeAssemblyBuilder.TryEmitCompileTimeAssembly( sourceAssemblySymbol.Compilation, diagnosticSink, out assemblyStream ) )
                {
                    compileTimeAssemblyBytes = null;

                    return false;
                }
                else
                {
                    compileTimeAssemblyBytes = assemblyStream?.ToArray();
                }
            }
            else
            {
                if ( this._compilation.GetMetadataReference( assemblySymbol ) is not { } reference )
                {
                    throw new InvalidOperationException( $"Could not find reference for assembly {assemblySymbol}." );
                }

                if ( reference is CompilationReference compilationReference )
                {
                    // note: this should only happen in Try Caravela
                    var compilation = compilationReference.Compilation;

                    if ( !new CompileTimeAssemblyBuilder( this._serviceProvider ).TryEmitCompileTimeAssembly(
                        compilation,
                        diagnosticSink,
                        out assemblyStream ) )
                    {
                        compileTimeAssemblyBytes = null;

                        return false;
                    }
                    else
                    {
                        compileTimeAssemblyBytes = assemblyStream?.ToArray();
                    }
                }
                else
                {
                    if ( reference is not PortableExecutableReference peReference )
                    {
                        throw new AssertionFailedException( $"The assembly {assemblySymbol} does not correspond to a known kind of reference." );
                    }

                    if ( peReference.FilePath is not { } path )
                    {
                        throw new InvalidOperationException( $"Could not access path for the assembly {assemblySymbol}." );
                    }

                    if ( this.GetCompileTimeAssemblyBytes( path ) is not { } assemblyBytes )
                    {
                        throw new InvalidOperationException( $"Runtime assembly {assemblySymbol} does not contain a compile-time assembly resource." );
                    }

                    compileTimeAssemblyBytes = assemblyBytes;
                }
            }

            return true;
        }

        public bool TryLoadCompileTimeAssembly( IAssemblySymbol assemblySymbol, IDiagnosticAdder diagnosticSink, out Assembly? compileTimeAssembly )
        {
            if ( !this._assemblyMap.TryGetValue( assemblySymbol, out compileTimeAssembly ) )
            {
                if ( !this.TryGetCompileTimeAssemblyBytes( assemblySymbol, diagnosticSink, out var compileTimeAssemblyBytes ) )
                {
                    compileTimeAssembly = null;

                    return false;
                }

                if ( compileTimeAssemblyBytes != null )
                {
                    compileTimeAssembly = Load( compileTimeAssemblyBytes );
                }
                else
                {
                    compileTimeAssembly = null;
                }

                this._assemblyMap[assemblySymbol] = compileTimeAssembly;
            }

            return true;
        }
    }
}