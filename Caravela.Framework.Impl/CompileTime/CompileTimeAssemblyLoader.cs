// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CompileTime
{
    internal sealed class CompileTimeAssemblyLoader : IDisposable, ICompileTimeTypeResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CSharpCompilation _compilation;
        private readonly CompileTimeAssemblyBuilder _compileTimeAssemblyBuilder;

        private readonly Dictionary<IAssemblySymbol, Assembly> _assemblyMap = new();
        private readonly Dictionary<string, byte[]?> _assemblyBytesMap = new();
        private readonly AttributeDeserializer _attributeDeserializer;

        public CompileTimeAssemblyLoader( IServiceProvider serviceProvider, CSharpCompilation compilation, CompileTimeAssemblyBuilder compileTimeAssemblyBuilder )
        {
            this._serviceProvider = serviceProvider;
            this._compilation = compilation;
            this._compileTimeAssemblyBuilder = compileTimeAssemblyBuilder;

            // TODO: this is probably not enough
            this._assemblyMap.Add( compilation.ObjectType.ContainingAssembly, typeof( object ).Assembly );

            this._attributeDeserializer = new AttributeDeserializer( this );
            AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;
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
            // TODO: use AssemblyLoadContext on .Net Core? (requires multi-targetting)
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
                r => r is PortableExecutableReference { FilePath: string path } && Path.GetFileNameWithoutExtension( path ) == new AssemblyName( args.Name ).Name );
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

        public object CreateAttributeInstance( Code.IAttribute attribute )
        {

            return this._attributeDeserializer.CreateAttribute( attribute );
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
            var result = assembly.GetType( ReflectionNameHelper.GetReflectionName( typeSymbol ) );

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

        private byte[]? GetResourceBytes( string assemblyPath, string resourceName )
        {
            var resolver = new PathAssemblyResolver( new[] { typeof( object ).Assembly.Location } );
            using var mlc = new MetadataLoadContext( resolver, typeof( object ).Assembly.GetName().Name );

            // LoadFromAssemblyPath throws for mscorlib
            if ( Path.GetFileNameWithoutExtension( assemblyPath ) == typeof( object ).Assembly.GetName().Name )
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
            else
            {
                return null;
            }
        }

        public byte[]? GetCompileTimeAssembly( string path )
        {
            if ( this._assemblyBytesMap.TryGetValue( path, out var assemblyBytes ) )
            {
                return assemblyBytes;
            }

            assemblyBytes = this.GetResourceBytes( path, this._compileTimeAssemblyBuilder.GetResourceName() );

            this._assemblyBytesMap.Add( path, assemblyBytes );
            return assemblyBytes;
        }

        private byte[] GetCompileTimeAssembly( IAssemblySymbol assemblySymbol )
        {
            MemoryStream? assemblyStream;

            if ( assemblySymbol is ISourceAssemblySymbol sourceAssemblySymbol )
            {
                assemblyStream = this._compileTimeAssemblyBuilder.EmitCompileTimeAssembly( sourceAssemblySymbol.Compilation );
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
                    assemblyStream = new CompileTimeAssemblyBuilder( this._serviceProvider, compilation ).EmitCompileTimeAssembly( compilation );
                }
                else
                {
                    if ( reference is not PortableExecutableReference peReference )
                    {
                        throw new InvalidOperationException( $"The assembly {assemblySymbol} does not correspond to a known kind of reference." );
                    }

                    if ( peReference.FilePath is not { } path )
                    {
                        throw new InvalidOperationException( $"Could not access path for the assembly {assemblySymbol}." );
                    }

                    if ( this.GetCompileTimeAssembly( path ) is not { } assemblyBytes )
                    {
                        throw new InvalidOperationException( $"Runtime assembly {assemblySymbol} does not contain a compile-time assembly resource." );
                    }

                    return assemblyBytes;
                }
            }

            if ( assemblyStream == null )
            {
                throw new InvalidOperationException( $"Could not create compile-time assembly for {assemblySymbol}." );
            }

            return assemblyStream.ToArray();
        }

        private Assembly LoadCompileTimeAssembly( IAssemblySymbol assemblySymbol )
        {
            if ( !this._assemblyMap.TryGetValue( assemblySymbol, out var assembly ) )
            {
                assembly = Load( this.GetCompileTimeAssembly( assemblySymbol ) );
                this._assemblyMap[assemblySymbol] = assembly;
            }

            return assembly;
        }
    }
}
