using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Text;

namespace Caravela.Framework.Impl.CompileTime
{
    sealed class CompileTimeAssemblyLoader
    {
        private readonly CSharpCompilation _compilation;
        private readonly CompileTimeAssemblyBuilder _compileTimeAssemblyBuilder;

        private readonly Dictionary<IAssemblySymbol, Assembly> _assemblyMap = new();

        public CompileTimeAssemblyLoader( CSharpCompilation compilation, CompileTimeAssemblyBuilder compileTimeAssemblyBuilder )
        {
            this._compilation = compilation;
            this._compileTimeAssemblyBuilder = compileTimeAssemblyBuilder;
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
            if ( !this._assemblyMap.TryGetValue( assemblySymbol, out var assembly ) )
            {
                assembly = this.LoadCompileTimeAssembly( assemblySymbol );
                this._assemblyMap[assemblySymbol] = assembly;
            }

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

        private Assembly LoadCompileTimeAssembly( IAssemblySymbol assemblySymbol )
        {
            if (assemblySymbol is ISourceAssemblySymbol sourceAssemblySymbol)
            {
                var assemblyStream = this._compileTimeAssemblyBuilder.EmitCompileTimeAssembly( sourceAssemblySymbol.Compilation );

                if ( assemblyStream == null )
                    throw new InvalidOperationException( $"Could not create compile-time assembly for {assemblySymbol}." );

                return Load( assemblyStream );
            }
            else
            {
                if ( this._compilation.GetMetadataReference( assemblySymbol ) is not { } reference )
                    throw new InvalidOperationException( $"Could not find reference for assembly {assemblySymbol}." );

                if ( reference is not PortableExecutableReference peReference )
                    throw new InvalidOperationException( $"The assembly {assemblySymbol} does not correspond to a PE reference." );

                if ( peReference.FilePath is not { } path )
                    throw new InvalidOperationException( $"Could not access path for the assembly {assemblySymbol}." );

                // TODO: use S.R.M to avoid loading the runtime assembly?
                var runtimeAssembly = Assembly.ReflectionOnlyLoad( File.ReadAllBytes( path ) );

                using var stream = runtimeAssembly.GetManifestResourceStream( this._compileTimeAssemblyBuilder.GetResourceName( assemblySymbol.Name ) );
                if ( stream == null )
                    throw new InvalidOperationException( $"Runtime assembly {assemblySymbol} does not contain a compile-time assembly reasource." );

                var memoryStream = new MemoryStream( (int)stream.Length );
                stream.CopyTo( memoryStream );

                return Load( memoryStream );
            }
        }

        private static Assembly Load(MemoryStream stream)
        {
            // TODO: use AssemblyLoadContext on .Net Core? (requires multi-targetting)
            return Assembly.Load( stream.ToArray() );
        }
    }

    class DiagnosticsException : CaravelaException
    {
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public DiagnosticsException( ImmutableArray<Diagnostic> diagnostics )
            : base( GeneralDiagnosticDescriptors.ErrorBuildingCompileTimeAssembly, string.Join( Environment.NewLine, diagnostics ) ) =>
            this.Diagnostics = diagnostics;
    }
}
