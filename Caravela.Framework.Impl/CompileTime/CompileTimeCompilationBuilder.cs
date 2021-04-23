// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class CompileTimeCompilationBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Random _random = new();
        private readonly CompileTimeDomain _domain;

        public const string ResourceName = "Caravela.CompileTimeAssembly";

        public CompileTimeCompilationBuilder( IServiceProvider serviceProvider, CompileTimeDomain domain )
        {
            this._serviceProvider = serviceProvider;
            this._domain = domain;
        }

        private bool TryCreateCompileTimeCompilation(
            Compilation runTimeCompilation,
            IEnumerable<CompileTimeProject> referencedProjects,
            IDiagnosticAdder diagnosticSink,
            out Compilation? compileTimeCompilation )
        {
            compileTimeCompilation = this.CreateEmptyCompileTimeCompilation( runTimeCompilation.AssemblyName.AssertNotNull(), referencedProjects )
                .AddSyntaxTrees(
                    SyntaxFactory.ParseSyntaxTree(
                        $"[assembly: System.Reflection.AssemblyVersion(\"{this.GetUniqueVersion()}\")]",
                        CSharpParseOptions.Default ) );

            var produceCompileTimeCodeRewriter = new ProduceCompileTimeCodeRewriter(
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

            compileTimeCompilation = compileTimeCompilation.AddSyntaxTrees( modifiedRunTimeCompilation.SyntaxTrees );

            compileTimeCompilation = new RemoveInvalidUsingRewriter( compileTimeCompilation ).VisitAllTrees( compileTimeCompilation );

            return true;
        }

        private CSharpCompilation CreateEmptyCompileTimeCompilation(
            string assemblyName,
            IEnumerable<CompileTimeProject> referencedProjects )
        {
            var assemblyLocator = this._serviceProvider.GetService<ReferenceAssemblyLocator>();

            var standardReferences = assemblyLocator.StandardAssemblyPaths
                .Select( path => MetadataReference.CreateFromFile( path ) );

            return CSharpCompilation.Create(
                    assemblyName,
                    Array.Empty<SyntaxTree>(),
                    standardReferences,
                    new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary, deterministic: true ) )
                .AddReferences(
                    referencedProjects
                        .Where( r => !r.IsEmpty )
                        .Select( r => r.ToMetadataReference() ) );
        }

        // this is not nearly as good as a GUID, but should be good enough for the purpose of preventing collisions within the same process
        private Version GetUniqueVersion()
        {
            lock ( this._random )
            {
                int GetVersionComponent() => this._random.Next( 0, ushort.MaxValue );

                var major = GetVersionComponent();
                var minor = GetVersionComponent();
                var build = GetVersionComponent();
                var revision = GetVersionComponent();

                return new Version( major, minor, build, revision );
            }
        }

        private bool TryEmit(
            Compilation compileTimeCompilation,
            IDiagnosticAdder diagnosticSink,
            out MemoryStream stream )
        {
            var buildOptions = this._serviceProvider.GetService<IBuildOptions>();

            EmitResult? result;
            stream = new MemoryStream();

            // Write the generated files to disk if we should.
            if ( !string.IsNullOrWhiteSpace( buildOptions.CompileTimeProjectDirectory ) )
            {
                var compileTimeProjectDirectory = Path.Combine( buildOptions.CompileTimeProjectDirectory, compileTimeCompilation.AssemblyName );

                var mutexName = "Global\\Caravela_" + nameof(CompileTimeCompilationBuilder) + "_" + HashUtilities.HashString( compileTimeProjectDirectory! );
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

        internal bool TryCreateCompileTimeProject(
            Compilation runTimeCompilation,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            IDiagnosticAdder diagnosticSink,
            out CompileTimeProject? project )
        {
            if ( !this.TryCreateCompileTimeCompilation( runTimeCompilation, referencedProjects, diagnosticSink, out var compileTimeCompilation ) )
            {
                project = null;

                return false;
            }

            if ( compileTimeCompilation == null )
            {
                // The run-time compilation does not contain compile-time classes, but it can have compile-time references.

                if ( referencedProjects.Count == 0 )
                {
                    project = null;
                }
                else
                {
                    project = CompileTimeProject.CreateEmpty( this._domain, runTimeCompilation.Assembly.Identity, referencedProjects );
                }

                return true;
            }
            else
            {
                if ( !this.TryEmit( compileTimeCompilation, diagnosticSink, out var compileTimeAssemblyStream ) )
                {
                    project = null;

                    return false;
                }

                var aspectType = compileTimeCompilation.GetTypeByMetadataName( typeof(IAspect).FullName );

                var manifest = new CompileTimeProjectManifest
                {
                    AssemblyName = compileTimeCompilation.Assembly.Identity.Name,
                    Version = compileTimeCompilation.Assembly.Identity.Version,
                    References = referencedProjects.Select( r => r.RunTimeIdentity.Name ).ToList(),
                    AspectTypes = compileTimeCompilation.Assembly
                        .GetTypes()
                        .Where( t => compileTimeCompilation.HasImplicitConversion( t, aspectType ) )
                        .Select( t => t.GetReflectionName() )
                        .ToList()
                };

                project = CompileTimeProject.Create(
                    this._domain,
                    runTimeCompilation.Assembly.Identity,
                    referencedProjects,
                    manifest,
                    compileTimeCompilation,
                    compileTimeAssemblyStream.ToArray() );

                return true;
            }
        }

        /// <summary>
        /// Prepares run-time assembly by making compile-time only methods throw <see cref="NotSupportedException"/>.
        /// </summary>
        public static CSharpCompilation PrepareRunTimeAssembly( CSharpCompilation compilation )
            => new PrepareRunTimeAssemblyRewriter( compilation ).VisitAllTrees( compilation );

        public bool TryCompileDeserializedProject(
            CompileTimeProjectManifest manifest,
            IReadOnlyList<SyntaxTree> syntaxTrees,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            IDiagnosticAdder diagnosticAdder,
            out Compilation compilation,
            out MemoryStream memoryStream )
        {
            compilation = this.CreateEmptyCompileTimeCompilation( manifest.AssemblyName.AssertNotNull(), referencedProjects )
                .AddSyntaxTrees( syntaxTrees );

            return this.TryEmit( compilation, diagnosticAdder, out memoryStream );
        }
    }
}