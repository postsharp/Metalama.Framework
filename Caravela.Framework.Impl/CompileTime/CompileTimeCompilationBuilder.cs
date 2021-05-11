// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.Mapping;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Sdk;
using K4os.Hash.xxHash;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// This class is responsible for building a compile-time <see cref="Compilation"/> based on a run-time one.
    /// </summary>
    internal partial class CompileTimeCompilationBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CompileTimeDomain _domain;
        private readonly Dictionary<ulong, CompileTimeProject> _cache = new();
        private readonly IBuildOptions _buildOptions;

        private static string _buildId = AssemblyMetadataReader.GetInstance( typeof(CompileTimeCompilationBuilder).Assembly ).BuildId
                                         ?? Guid.NewGuid().ToString();

        public const string ResourceName = "Caravela.CompileTimeAssembly";

        public CompileTimeCompilationBuilder( IServiceProvider serviceProvider, CompileTimeDomain domain )
        {
            this._buildOptions = serviceProvider.GetService<IBuildOptions>();
            this._serviceProvider = serviceProvider;
            this._domain = domain;
        }

        private static ulong ComputeSourceHash( IReadOnlyList<SyntaxTree> compileTimeTrees )
        {
            XXH64 h = new();

            foreach ( var syntaxTree in compileTimeTrees )
            {
                h.Update( syntaxTree.GetText().GetChecksum() );
            }

            return h.Digest();
        }

        private static ulong ComputeProjectHash(
            IEnumerable<CompileTimeProject> referencedProjects,
            ulong sourceHash )
        {
            XXH64 h = new();
            h.Update( _buildId );

            foreach ( var reference in referencedProjects )
            {
                h.Update( reference.Hash );
            }

            h.Update( sourceHash );

            return h.Digest();
        }

        private bool TryCreateCompileTimeCompilation(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree> treesWithCompileTimeCode,
            IEnumerable<CompileTimeProject> referencedProjects,
            ulong hash,
            IDiagnosticAdder diagnosticSink,
            out Compilation? compileTimeCompilation,
            out ILocationAnnotationMap? locationAnnotationMap,
            out Dictionary<string, SyntaxTree>? syntaxTreeMap )
        {
            locationAnnotationMap = null;

            // If there is no compile-time tree, there is no need to do anything.
            if ( treesWithCompileTimeCode.Count == 0 )
            {
                compileTimeCompilation = null;
                syntaxTreeMap = null;
                return true;
            }

            var assemblyName = GetCompileTimeAssemblyName( runTimeCompilation.AssemblyName!, hash );
            compileTimeCompilation = this.CreateEmptyCompileTimeCompilation( assemblyName, referencedProjects );

            var templateCompiler = new TemplateCompiler( this._serviceProvider );

            var produceCompileTimeCodeRewriter = new ProduceCompileTimeCodeRewriter(
                runTimeCompilation,
                compileTimeCompilation,
                diagnosticSink,
                templateCompiler );

            // Creates the new syntax trees. Store them in a dictionary mapping the transformed trees to the source trees.
            var syntaxTrees = treesWithCompileTimeCode.Select(
                    t => (TransformedTree: CSharpSyntaxTree.Create(
                                  (CSharpSyntaxNode) produceCompileTimeCodeRewriter.Visit( t.GetRoot() ),
                                  CSharpParseOptions.Default,
                                  t.FilePath,
                                  Encoding.UTF8 )
                              .WithFilePath( GetTransformedFilePath( t.FilePath ) ),
                          SourceTree: t) )
                .ToList();

            syntaxTreeMap = syntaxTrees.ToDictionary( t => t.TransformedTree.FilePath, t => t.SourceTree );

            locationAnnotationMap = templateCompiler.LocationAnnotationMap;

            if ( !produceCompileTimeCodeRewriter.Success )
            {
                return false;
            }

            if ( !produceCompileTimeCodeRewriter.FoundCompileTimeCode )
            {
                compileTimeCompilation = null;

                return true;
            }

            compileTimeCompilation = compileTimeCompilation.AddSyntaxTrees( syntaxTrees.Select( t=>t.TransformedTree ) );

            compileTimeCompilation = new RemoveInvalidUsingRewriter( compileTimeCompilation ).VisitTrees( compileTimeCompilation );

            return true;
        }

        private static string GetTransformedFilePath( string originalFilePath )
        {
            // Find a decent and unique name.
            var transformedFileName = !string.IsNullOrWhiteSpace( originalFilePath )
                ? Path.GetFileNameWithoutExtension( originalFilePath ) 
                : "Anonymous";

            transformedFileName += "_" + HashUtilities.HashString( originalFilePath );
            transformedFileName += Path.GetExtension( originalFilePath );

            return transformedFileName;
        }

        internal static string GetCompileTimeAssemblyName( string runTimeAssemblyName, IEnumerable<CompileTimeProject> referencedProjects, ulong sourceHash )
        {
            var projectHash = ComputeProjectHash( referencedProjects, sourceHash );

            return GetCompileTimeAssemblyName( runTimeAssemblyName, projectHash );
        }

        private static string GetCompileTimeAssemblyName( string runTimeAssemblyName, ulong projectHash )
            => $"Caravela_{runTimeAssemblyName}_{projectHash:x16}";

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

        private static ImmutableArray<TextMap> CreateLocationMaps( Compilation compileTimeCompilation, ILocationAnnotationMap locationAnnotationMap )
            => compileTimeCompilation.SyntaxTrees.Select( t => TextMap.Create( t, locationAnnotationMap ) ).WhereNotNull().ToImmutableArray();

        private static void WriteLocationMaps( IEnumerable<TextMap> maps, string outputDirectory )
        {
            foreach ( var map in maps )
            {
                var filePath = Path.Combine( outputDirectory, Path.GetFileNameWithoutExtension( map.TargetPath ) + ".map" );

                using ( var writer = File.Create( filePath ) )
                {
                    map.Write( writer );
                }
            }
        }

        private bool TryEmit(
            Compilation compileTimeCompilation,
            IDiagnosticAdder diagnosticSink,
            IReadOnlyDictionary<string,SyntaxTree>? syntaxTreeMap,
            out IReadOnlyList<CompileTimeFile>? codeFiles )
        {
            var outputInfo = this.GetOutputPaths( compileTimeCompilation.AssemblyName! );

            var sourceFilesList = new List<CompileTimeFile>();
            
            DeleteOutputFiles();

            try
            {
                var buildOptions = this._serviceProvider.GetService<IBuildOptions>();
                var emitOptions = new EmitOptions( debugInformationFormat: DebugInformationFormat.PortablePdb );

                // Write the generated files to disk if we should.
                if ( !string.IsNullOrWhiteSpace( buildOptions.CompileTimeProjectDirectory ) )
                {
                    using var mutex = MutexHelper.CreateGlobalMutex( outputInfo.Directory );
                    mutex.WaitOne();

                    try
                    {
                        if ( !Directory.Exists( outputInfo.Directory ) )
                        {
                            Directory.CreateDirectory( outputInfo.Directory );
                        }

                        compileTimeCompilation =
                            compileTimeCompilation.WithOptions( compileTimeCompilation.Options.WithOptimizationLevel( OptimizationLevel.Debug ) );

                        foreach ( var compileTimeSyntaxTree in compileTimeCompilation.SyntaxTrees )
                        {

                            var transformedFileName = Path.Combine( outputInfo.Directory, compileTimeSyntaxTree.FilePath );

                            if ( syntaxTreeMap != null )
                            {
                                sourceFilesList.Add( new CompileTimeFile( transformedFileName, syntaxTreeMap[compileTimeSyntaxTree.FilePath] ) );
                            }

                            var path = Path.Combine( outputInfo.Directory, transformedFileName );
                            var text = compileTimeSyntaxTree.GetText();

                            // Write the file in a retry loop to handle locks. It seems there are still file lock issues
                            // despite the Mutex. 
                            RetryHelper.Retry(
                                () =>
                                {
                                    using ( var textWriter = new StreamWriter( path, false, Encoding.UTF8 ) )
                                    {
                                        text.Write( textWriter );
                                    }
                                } );

                            // Update the link to the file path.
                            var newTree = CSharpSyntaxTree.Create( (CSharpSyntaxNode) compileTimeSyntaxTree.GetRoot(), (CSharpParseOptions?) compileTimeSyntaxTree.Options, path, Encoding.UTF8 );
                            compileTimeCompilation = compileTimeCompilation.ReplaceSyntaxTree( compileTimeSyntaxTree, newTree );
                        }
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }

                using var peStream = File.Create( outputInfo.PE );
                using var pdbStream = File.Create( outputInfo.Pdb );

                var emitResult = compileTimeCompilation.Emit( peStream, pdbStream, options: emitOptions );

                diagnosticSink.ReportDiagnostics( emitResult.Diagnostics.Where( d => d.Severity >= DiagnosticSeverity.Error ) );

                if ( !emitResult.Success )
                {
                    DeleteOutputFiles();
                }

                codeFiles = sourceFilesList;
                return emitResult.Success;
            }
            catch
            {
                DeleteOutputFiles();

                throw;
            }

            void DeleteOutputFiles()
            {
                try
                {
                    if ( File.Exists( outputInfo.PE ) )
                    {
                        File.Delete( outputInfo.PE );
                    }

                    if ( File.Exists( outputInfo.Pdb ) )
                    {
                        File.Delete( outputInfo.Pdb );
                    }
                }
                catch ( IOException ) { }
            }
        }

        private static IReadOnlyList<SyntaxTree> GetCompileTimeSyntaxTrees( Compilation runTimeCompilation, IReadOnlyList<SyntaxTree>? compileTimeTreesHint )
        {
            List<SyntaxTree> compileTimeTrees = new();
            var classifier = SymbolClassifier.GetInstance( runTimeCompilation );
            
            var trees = compileTimeTreesHint ?? runTimeCompilation.SyntaxTrees;

            foreach ( var tree in trees )
            {
                FindCompileTimeCodeVisitor visitor = new( runTimeCompilation.GetSemanticModel( tree, true ), classifier );
                visitor.Visit( tree.GetRoot() );

                if ( visitor.HasCompileTimeCode )
                {
                    compileTimeTrees.Add( tree );
                }
            }

            return compileTimeTrees;
        }

        /// <summary>
        /// Tries to create a compile-time <see cref="Compilation"/> given a run-time <see cref="Compilation"/>.
        /// </summary>
        internal bool TryGetCompileTimeProject(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            IDiagnosticAdder diagnosticSink,
            out CompileTimeProject? project,
            bool cacheOnly )
            => this.TryGetCompileTimeProject(
                runTimeCompilation,
                GetCompileTimeSyntaxTrees( runTimeCompilation, compileTimeTreesHint ),
                referencedProjects,
                diagnosticSink,
                cacheOnly,
                out project );

      

        private bool TryGetCompileTimeProjectFromCache(
            Compilation runTimeCompilation,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            OutputPaths outputPaths,
            string compileTimeAssemblyName,
            ulong projectHash,
            out CompileTimeProject? project )
        {
            // Look in in-memory cache.
            if (this._cache.TryGetValue( projectHash, out project ))
            {
                return true;
            }
            
            // Look on disk.
            if (!File.Exists( outputPaths.PE ) || !File.Exists( outputPaths.Manifest ))
            {
                project = null;

                return false;
            }

            // Deserialize the manifest.
            if (CompileTimeProjectManifest.TryDeserialize( File.OpenRead( outputPaths.Manifest ), out var manifest ))
            {
                project = CompileTimeProject.Create(
                    this._domain,
                    runTimeCompilation.Assembly.Identity,
                    new AssemblyIdentity( compileTimeAssemblyName ),
                    referencedProjects,
                    manifest,
                    outputPaths.PE,
                    outputPaths.Directory,
                    TextMap.Read );
                
                this._cache.Add( projectHash, project );

                return true;
            }
            else
            {
                try
                {
                    File.Delete( outputPaths.Manifest );
                }
                catch (IOException) { }

                project = null;

                return false;
            }
        }

        private bool TryGetCompileTimeProject(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree> sourceTreesWithCompileTimeCode,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            IDiagnosticAdder diagnosticSink,
            bool cacheOnly,
            out CompileTimeProject? project )
        {
            // Check the in-process cache.
            var (sourceHash, projectHash, compileTimeAssemblyName, outputPaths) = 
                this.GetPreCacheProjectInfo( runTimeCompilation, sourceTreesWithCompileTimeCode, referencedProjects );

            if ( !this.TryGetCompileTimeProjectFromCache(
                runTimeCompilation,
                referencedProjects,
                outputPaths,
                compileTimeAssemblyName,
                projectHash,
                out project ) )
            {
                if ( cacheOnly )
                {
                    // We were asked to get cache projects only. Don't create it.
                    project = null;

                    return false;
                }
                
                // Generate the C# compilation.
                if ( !this.TryCreateCompileTimeCompilation(
                    runTimeCompilation,
                    sourceTreesWithCompileTimeCode,
                    referencedProjects,
                    projectHash,
                    diagnosticSink,
                    out var compileTimeCompilation,
                    out var locationAnnotationMap,
                    out var syntaxTreeMap) )
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
                        project = CompileTimeProject.CreateEmpty(
                            this._domain,
                            runTimeCompilation.Assembly.Identity,
                            new AssemblyIdentity( compileTimeAssemblyName ),
                            referencedProjects );
                    }

                    return true;
                }
                else
                {
                    if ( !this.TryEmit( compileTimeCompilation, diagnosticSink, syntaxTreeMap, out var sourceFiles ) )
                    {
                        project = null;

                        return false;
                    }

                    var locationMaps = CreateLocationMaps( compileTimeCompilation, locationAnnotationMap! );
                    WriteLocationMaps( locationMaps, outputPaths.Directory );

                    var aspectType = compileTimeCompilation.GetTypeByMetadataName( typeof(IAspect).FullName );

                    var manifest = new CompileTimeProjectManifest(
                        compileTimeCompilation.AssemblyName!,
                        compileTimeCompilation.Assembly
                            .GetTypes()
                            .Where( t => compileTimeCompilation.HasImplicitConversion( t, aspectType ) )
                            .Select( t => t.GetReflectionNameSafe() )
                            .ToList(),
                        referencedProjects.Select( r => r.RunTimeIdentity.GetDisplayName() ).ToList(),
                        sourceHash,
                        sourceFiles);

                    project = CompileTimeProject.Create(
                        this._domain,
                        runTimeCompilation.Assembly.Identity,
                        compileTimeCompilation.Assembly.Identity,
                        referencedProjects,
                        manifest,
                        outputPaths.PE,
                        outputPaths.Directory,
                        name => GetLocationMap( locationMaps, name ) );

                    using ( var manifestStream = File.Create( outputPaths.Manifest ) )
                    {
                        manifest.Serialize( manifestStream );
                    }
                }
                
                this._cache.Add( projectHash, project );
            }
            

            

            return true;
        }

        private (ulong sourceHash, ulong projectHash, string compileTimeAssemblyName, OutputPaths outputPaths) GetPreCacheProjectInfo(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree> sourceTreesWithCompileTimeCode,
            IReadOnlyList<CompileTimeProject> referencedProjects )
        {
            var sourceHash = ComputeSourceHash( sourceTreesWithCompileTimeCode );
            var projectHash = ComputeProjectHash( referencedProjects, sourceHash );

            var compileTimeAssemblyName = GetCompileTimeAssemblyName( runTimeCompilation.AssemblyName!, projectHash );
            var outputPaths = this.GetOutputPaths( compileTimeAssemblyName );

            return ( sourceHash, projectHash, compileTimeAssemblyName, outputPaths );
        }

        private static TextMap? GetLocationMap( ImmutableArray<TextMap> locationMaps, string name )
            => locationMaps.Where( m => name.EndsWith( m.TargetPath, StringComparison.OrdinalIgnoreCase ) )
                .OrderByDescending( m => m.TargetPath.Length )
                .FirstOrDefault();

        record OutputPaths(string Directory, string PE, string Pdb, string Manifest);
        
        private OutputPaths GetOutputPaths( string compileTimeAssemblyName )
        {
            var directory = Path.Combine( this._buildOptions.CacheDirectory, "CompileTimeAssemblies", compileTimeAssemblyName );
            var pe = Path.Combine( directory, compileTimeAssemblyName + ".dll" );
            var pdb = Path.ChangeExtension( pe, ".pdb" );
            var manifest = Path.ChangeExtension( pe, ".manifest" );

            return new (directory, pe, pdb, manifest);
        }

        /// <summary>
        /// Prepares run-time assembly by making compile-time only methods throw <see cref="NotSupportedException"/>.
        /// </summary>
        public static Compilation PrepareRunTimeAssembly( Compilation compilation )
            => new PrepareRunTimeAssemblyRewriter( compilation ).VisitTrees( compilation );

        /// <summary>
        /// Tries to compile (to a binary image) a project given its manifest and syntax trees. 
        /// </summary>
        public bool TryCompileDeserializedProject(
            string assemblyName,
            IReadOnlyList<SyntaxTree> syntaxTrees,
            IReadOnlyList<CompileTimeProject> referencedProjects,
            IDiagnosticAdder diagnosticAdder,
            out string assemblyPath,
            out string sourceDirectory )
        {
            var outputInfo = this.GetOutputPaths( assemblyName );

            var compilation = this.CreateEmptyCompileTimeCompilation( assemblyName, referencedProjects )
                .AddSyntaxTrees( syntaxTrees );

            assemblyPath = outputInfo.PE;
            sourceDirectory = outputInfo.Directory;

            return this.TryEmit( compilation, diagnosticAdder,null, out _ );
        }
    }
}