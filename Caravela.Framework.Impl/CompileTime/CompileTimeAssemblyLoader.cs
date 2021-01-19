using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Caravela.Framework.Impl.CompileTime
{
    sealed class CompileTimeAssemblyLoader : IDisposable
    {
        private readonly CSharpCompilation _compilation;
        private readonly CompileTimeAssemblyBuilder _compileTimeAssemblyBuilder;

        private readonly Dictionary<IAssemblySymbol, Assembly> _assemblyMap = new();
        private readonly Dictionary<string, byte[]?> _assemblyBytesMap = new();

        public CompileTimeAssemblyLoader( CSharpCompilation compilation, CompileTimeAssemblyBuilder compileTimeAssemblyBuilder )
        {
            this._compilation = compilation;
            this._compileTimeAssemblyBuilder = compileTimeAssemblyBuilder;

            AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= this.CurrentDomain_AssemblyResolve;
        }

        // TODO: perf
        private Assembly? CurrentDomain_AssemblyResolve( object sender, ResolveEventArgs args )
        {
            if ( !this._assemblyMap.ContainsValue( args.RequestingAssembly ) )
                return null;

            var reference = this._compilation.References.SingleOrDefault(
                r => r is PortableExecutableReference { FilePath: string path } && Path.GetFileNameWithoutExtension( path ) == new AssemblyName(args.Name).Name );
            if ( reference == null )
                return null;

            if ( this._compilation.GetAssemblyOrModuleSymbol( reference ) is not IAssemblySymbol symbol )
                return null;

            return this.LoadCompileTimeAssembly( symbol );
        }

        public object CreateInstance ( INamedTypeSymbol typeSymbol )
        {
            var type = this.GetCompileTimeType( typeSymbol );

            if ( type == null )
                throw new InvalidOperationException( $"Could not load type {typeSymbol}." );

            return Activator.CreateInstance( type );
        }

        public Type? GetCompileTimeType( INamedTypeSymbol typeSymbol )
        {
            var assemblySymbol = typeSymbol.ContainingAssembly;
            var assembly = this.LoadCompileTimeAssembly( assemblySymbol );
            return assembly.GetType( GetFullMetadataName( typeSymbol ) );
        }

        // https://stackoverflow.com/a/27106959/41071
        private static string GetFullMetadataName( ISymbol? s )
        {
            if ( s == null || IsRootNamespace( s ) )
            {
                return string.Empty;
            }

            var sb = new StringBuilder( s.MetadataName );
            var last = s;

            s = s.ContainingSymbol;

            while ( !IsRootNamespace( s ) )
            {
                if ( s is ITypeSymbol && last is ITypeSymbol )
                {
                    sb.Insert( 0, '+' );
                }
                else
                {
                    sb.Insert( 0, '.' );
                }

                //sb.Insert(0, s.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                sb.Insert( 0, s.MetadataName );
                s = s.ContainingSymbol;
            }

            return sb.ToString();
        }

        private static bool IsRootNamespace( ISymbol symbol ) => symbol is INamespaceSymbol ns && ns.IsGlobalNamespace;

        private byte[]? GetResourceBytes( string assemblyPath, string resourceName )
        {
            var resolver = new PathAssemblyResolver( new string[] { typeof( object ).Assembly.Location } );
            using var mlc = new MetadataLoadContext( resolver, typeof( object ).Assembly.GetName().Name );

            // LoadFromAssemblyPath throws for mscorlib
            if ( Path.GetFileNameWithoutExtension( assemblyPath ) == typeof( object ).Assembly.GetName().Name )
                return null;

            var runtimeAssembly = mlc.LoadFromAssemblyPath( assemblyPath );

            if ( runtimeAssembly.GetManifestResourceNames().Contains( resourceName ) )
            {
                using var resourceStream = runtimeAssembly.GetManifestResourceStream( resourceName );

                var memoryStream = new MemoryStream( (int) resourceStream.Length );
                resourceStream.CopyTo( memoryStream );
                return memoryStream.ToArray();
            }
            else
                return null;
        }

        public byte[]? GetCompileTimeAssembly( string path )
        {
            if ( this._assemblyBytesMap.TryGetValue( path, out var assemblyBytes ) )
                return assemblyBytes;

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
                    throw new InvalidOperationException( $"Could not find reference for assembly {assemblySymbol}." );

                if ( reference is CompilationReference compilationReference )
                {
                    // note: this should only happen in Try Caravela
                    var compilation = compilationReference.Compilation;
                    assemblyStream = AspectPipeline.CreateCompileTimeAssemblyBuilder( compilation ).EmitCompileTimeAssembly( compilation );
                }
                else
                {
                    if ( reference is not PortableExecutableReference peReference )
                        throw new InvalidOperationException( $"The assembly {assemblySymbol} does not correspond to a known kind of reference." );

                    if ( peReference.FilePath is not { } path )
                        throw new InvalidOperationException( $"Could not access path for the assembly {assemblySymbol}." );

                    if ( this.GetCompileTimeAssembly( path ) is not { } assemblyBytes )
                        throw new InvalidOperationException( $"Runtime assembly {assemblySymbol} does not contain a compile-time assembly resource." );

                    return assemblyBytes;
                }
            }

            if ( assemblyStream == null )
                throw new InvalidOperationException( $"Could not create compile-time assembly for {assemblySymbol}." );

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

        private static Assembly Load( byte[] assembly )
        {
            // TODO: use AssemblyLoadContext on .Net Core? (requires multi-targetting)
            return Assembly.Load( assembly );
        }
    }
}
