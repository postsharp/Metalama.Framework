// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class CompileTimeAssemblyBuilder
    {
        private static readonly IEnumerable<MetadataReference> _fixedReferences;

        private readonly IServiceProvider _serviceProvider;
        private readonly Random _random = new();
        private readonly Dictionary<string, MemoryStream> _assemblyCache = new();
        private readonly Dictionary<Compilation, Compilation?> _compilationCache = new();

        public const string ResourceName = "Caravela.CompileTimeAssembly";

        static CompileTimeAssemblyBuilder()
        {
            var referenceAssemblyPaths = ReferenceAssemblyLocator.GetReferenceAssemblies();

            // the SDK assembly might not be loaded at this point, so make sure it is
            _ = new AspectWeaverAttribute( null! );

            var caravelaAssemblies = new[] { "Caravela.Framework.dll", "Caravela.Framework.Sdk.dll", "Caravela.Framework.Impl.dll" };

            var caravelaPaths = AppDomain.CurrentDomain.GetAssemblies()
                .Where( a => !a.IsDynamic ) // accessing Location of dynamic assemblies throws
                .Select( a => a.Location )
                .Where( path => caravelaAssemblies.Contains( Path.GetFileName( path ) ) );

            _fixedReferences = referenceAssemblyPaths.Concat( caravelaPaths )
                .Select( path => MetadataReference.CreateFromFile( path ) )
                .ToImmutableArray();
        }

        // cannot be constructor-injected, because CompileTimeAssemblyLoader and CompileTimeAssemblyBuilder depend on each other
        public CompileTimeAssemblyLoader? CompileTimeAssemblyLoader { get; set; }

        public CompileTimeAssemblyBuilder( IServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
        }

        public IReadOnlyDictionary<string, MemoryStream> BuiltAssemblies => this._assemblyCache;

        private bool TryCreateCompileTimeCompilation( Compilation runTimeCompilation, IDiagnosticAdder diagnosticSink, out Compilation? compileTimeCompilation )
        {
            if ( this._compilationCache.TryGetValue( runTimeCompilation, out compileTimeCompilation ) )
            {
                return true;
            }

            // Take the compile-time assembly of project references, if any.
            var compileTimeProjectReferences =
                runTimeCompilation.References
                    .Select( r => this.GetCompileTimeAssemblyReference( r, diagnosticSink ) )
                    .WhereNotNull()
                    .ToArray();

            compileTimeCompilation = CSharpCompilation.Create(
                runTimeCompilation.AssemblyName,
                Array.Empty<SyntaxTree>(),
                compileTimeProjectReferences.Concat( _fixedReferences ),
                new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary, deterministic: true ) );

            var produceCompileTimeCodeRewriter = new ProduceCompileTimeCodeRewriter(
                SymbolClassifier.GetInstance( runTimeCompilation ),
                runTimeCompilation,
                compileTimeCompilation,
                diagnosticSink );

            var modifiedRunTimeCompilation = produceCompileTimeCodeRewriter.VisitAllTrees( runTimeCompilation );

            if ( !produceCompileTimeCodeRewriter.Success )
            {
                return false;
            }

            if ( !produceCompileTimeCodeRewriter.FoundCompileTimeCode )
            {
                compileTimeCompilation = null;

                return true;
            }

            compileTimeCompilation = compileTimeCompilation.AddSyntaxTrees(
                SyntaxFactory.ParseSyntaxTree(
                    $"[assembly: System.Reflection.AssemblyVersion(\"{this.GetUniqueVersion()}\")]",
                    runTimeCompilation.SyntaxTrees.First().Options ) );

            compileTimeCompilation = compileTimeCompilation.AddSyntaxTrees( modifiedRunTimeCompilation.SyntaxTrees );

            compileTimeCompilation = new RemoveInvalidUsingRewriter( compileTimeCompilation ).VisitAllTrees( compileTimeCompilation );

            this._compilationCache.Add( runTimeCompilation, compileTimeCompilation );

            return true;
        }

        private MetadataReference? GetCompileTimeAssemblyReference( MetadataReference reference, IDiagnosticAdder diagnosticAdder )
        {
            switch ( reference )
            {
                case PortableExecutableReference { FilePath: { } filePath }:
                    {
                        var assemblyBytes = this.CompileTimeAssemblyLoader?.GetCompileTimeAssemblyBytes( filePath! );

                        if ( assemblyBytes != null )
                        {
                            return MetadataReference.CreateFromImage( assemblyBytes );
                        }

                        break;
                    }

                case CompilationReference compilationReference:
                    if ( this.TryCreateCompileTimeCompilation( compilationReference.Compilation, diagnosticAdder, out var compileTimeCompilation ) &&
                         compileTimeCompilation != null )
                    {
                        return compileTimeCompilation.ToMetadataReference();
                    }
                    else
                    {
                        return null;
                    }
            }

            return null!;
        }

        // this is not nearly as good as a GUID, but should be good enough for the purpose of preventing collisions within the same process
        private string GetUniqueVersion()
        {
            int GetVersionComponent() => this._random.Next( 0, ushort.MaxValue );

            var major = GetVersionComponent();
            var minor = GetVersionComponent();
            var build = GetVersionComponent();
            var revision = GetVersionComponent();

            return new Version( major, minor, build, revision ).ToString();
        }

        private bool TryEmit( Compilation compileTimeCompilation, IDiagnosticAdder diagnosticSink, out MemoryStream stream )
        {
            var buildOptions = this._serviceProvider.GetService<IBuildOptions>();
            var compileTimeProjectDirectory = buildOptions.CompileTimeProjectDirectory;

            if ( !TryEmit( compileTimeCompilation, compileTimeProjectDirectory, diagnosticSink, out stream ) )
            {
                return false;
            }

            this._assemblyCache.Add( compileTimeCompilation.AssemblyName!, stream );

            return true;
        }

        private static bool TryEmit(
            Compilation compileTimeCompilation,
            string? compileTimeProjectDirectory,
            IDiagnosticAdder diagnosticSink,
            out MemoryStream stream )
        {
            EmitResult? result;
            stream = new MemoryStream();

            // Write the generated files to disk if we should.
            if ( !string.IsNullOrWhiteSpace( compileTimeProjectDirectory ) )
            {
                var mutexName = "Global\\Caravela_" + nameof(CompileTimeAssemblyBuilder) + "_" + HashUtilities.HashString( compileTimeProjectDirectory! );
                using Mutex mutex = new( false, mutexName );
                mutex.WaitOne();

                try
                {
                    if ( !Directory.Exists( compileTimeProjectDirectory ) )
                    {
                        Directory.CreateDirectory( compileTimeProjectDirectory );
                    }

                    compileTimeCompilation =
                        compileTimeCompilation.WithOptions( compileTimeCompilation.Options.WithOptimizationLevel( OptimizationLevel.Debug ) );

                    var names = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

                    foreach ( var tree in compileTimeCompilation.SyntaxTrees )
                    {
                        // Find a decent and unique name.
                        var treeName = !string.IsNullOrWhiteSpace( tree.FilePath ) ? Path.GetFileNameWithoutExtension( tree.FilePath ) : "Anonymous";

                        if ( names.Contains( treeName ) )
                        {
                            var treeNameSuffix = treeName;

                            for ( var i = 1; names.Contains( treeName = treeNameSuffix + "_" + i ); i++ )
                            {
                                // Intentionally empty.
                            }

                            _ = names.Add( treeName );
                        }

                        var path = Path.Combine( compileTimeProjectDirectory, treeName + ".cs" );
                        var text = tree.GetText();

                        // Write the file in a retry loop to handle locks. It seems there are still file lock issues
                        // despite the Mutex. 
                        // TODO: Daniel can fix this?
                        RetryHelper.Retry(
                            () =>
                            {
                                using ( var textWriter = new StreamWriter( path, false, Encoding.UTF8 ) )
                                {
                                    text.Write( textWriter );
                                }
                            },
                            e => (uint) e.HResult == 0x80070020 );

                        // Update the link to the file path.
                        var newTree = CSharpSyntaxTree.Create( (CSharpSyntaxNode) tree.GetRoot(), (CSharpParseOptions?) tree.Options, path, Encoding.UTF8 );
                        compileTimeCompilation = compileTimeCompilation.ReplaceSyntaxTree( tree, newTree );
                    }

                    var options = new EmitOptions( debugInformationFormat: DebugInformationFormat.Embedded );

                    result = compileTimeCompilation.Emit( stream, options: options );
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            else
            {
                result = compileTimeCompilation.Emit( stream );
            }

            stream.Position = 0;

            diagnosticSink.ReportDiagnostics( result.Diagnostics.Where( d => d.Severity >= DiagnosticSeverity.Error ) );

            return result.Success;
        }

        public bool TryEmitCompileTimeAssembly( Compilation compilation, IDiagnosticAdder diagnosticSink, out MemoryStream? compileTimeAssemblyStream )
        {
            var sourceCodeDiagnostics = compilation.GetDiagnostics().Where( d => d.Severity == DiagnosticSeverity.Error ).ToList();

            if ( sourceCodeDiagnostics.Count > 0 )
            {
                // We don't continue with errors in the source code. This ensures that errors discovered later
                // can be attributed to the template compiler instead of to the user.
            }

            if ( !this.TryCreateCompileTimeCompilation( compilation, diagnosticSink, out var compileTimeCompilation ) )
            {
                compileTimeAssemblyStream = null;

                return false;
            }

            if ( compileTimeCompilation == null )
            {
                // The run-time compilation does not contain compile-time classes.
                compileTimeAssemblyStream = null;

                return true;
            }
            else
            {
                return this.TryEmit( compileTimeCompilation, diagnosticSink, out compileTimeAssemblyStream );
            }
        }

        /// <summary>
        /// Prepares run-time assembly by making compile-time only methods throw <see cref="NotSupportedException"/>.
        /// </summary>
        public static CSharpCompilation PrepareRunTimeAssembly( CSharpCompilation compilation )
            => new PrepareRunTimeAssemblyRewriter( compilation ).VisitAllTrees( compilation );

        private record BuiltAssembly ( Compilation Compilation, MemoryStream MemoryStream );
    }
}