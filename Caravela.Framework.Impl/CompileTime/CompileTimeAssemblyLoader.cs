using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Caravela.Framework.Impl.CompileTime
{
    sealed class CompileTimeAssemblyLoader
    {
        private readonly CompileTimeAssemblyBuilder _compileTimeAssemblyBuilder;
        private readonly Dictionary<IAssemblySymbol, Assembly> _assemblyMap = new();

        public CompileTimeAssemblyLoader( CompileTimeAssemblyBuilder compileTimeAssemblyBuilder ) => this._compileTimeAssemblyBuilder = compileTimeAssemblyBuilder;

        public object CreateInstance ( INamedTypeSymbol typeSymbol)
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
                var compilation = this._compileTimeAssemblyBuilder.CreateCompileTimeAssembly( sourceAssemblySymbol.Compilation );

                if ( compilation == null )
                    throw new InvalidOperationException( $"Could not create compile-time assembly for {assemblySymbol}." );

                var assemblyStream = Emit( compilation );
                return Load( assemblyStream );
            }
            else
            {
                // TODO: load compile-time assemblies from resources
                throw new NotImplementedException();
            }
        }

        private static MemoryStream Emit(Compilation compilation)
        {
            var stream = new MemoryStream();

            var result = compilation.Emit( stream );

            if (!result.Success)
            {
                throw new DiagnosticsException( result.Diagnostics );
            }

            stream.Position = 0;

            return stream;
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
