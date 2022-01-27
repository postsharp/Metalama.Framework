// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating.Mapping;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// This class is responsible to cache and load compile-time projects. The caller must first call
    /// the <see cref="TryGetCompileTimeProjectFromCompilation"/> for each project with which the loader will be used.
    /// The generation of compile-time compilations itself is delegated to the <see cref="CompileTimeCompilationBuilder"/>
    /// class.
    /// </summary>
    internal sealed class CompileTimeProjectLoader : CompileTimeTypeResolver, IService
    {
        private readonly CompileTimeDomain _domain;
        private readonly IServiceProvider _serviceProvider;
        private readonly CompileTimeCompilationBuilder _builder;
        private readonly IAssemblyLocator _runTimeAssemblyLocator;
        private readonly SystemTypeResolver _systemTypeResolver;
        private readonly CompileTimeProject _frameworkProject;
        private readonly ILogger _logger;

        // Maps the identity of the run-time project to the compile-time project.
        private readonly Dictionary<AssemblyIdentity, CompileTimeProject?> _projects = new();

        public AttributeDeserializer AttributeDeserializer { get; }

        private CompileTimeProjectLoader( CompileTimeDomain domain, IServiceProvider serviceProvider ) : base( serviceProvider )
        {
            this._domain = domain;
            this._serviceProvider = serviceProvider;
            this._builder = new CompileTimeCompilationBuilder( serviceProvider, domain );
            this._runTimeAssemblyLocator = serviceProvider.GetRequiredService<IAssemblyLocator>();
            this._logger = serviceProvider.GetLoggerFactory().CompileTime();
            this.AttributeDeserializer = new AttributeDeserializer( serviceProvider, this );
            this._systemTypeResolver = serviceProvider.GetRequiredService<SystemTypeResolver>();
            this._frameworkProject = CompileTimeProject.CreateFrameworkProject( serviceProvider, domain );
            this._projects.Add( this._frameworkProject.RunTimeIdentity, this._frameworkProject );
        }

        /// <summary>
        /// Returns a new <see cref="CompileTimeProjectLoader"/>.
        /// </summary>
        public static CompileTimeProjectLoader Create(
            CompileTimeDomain domain,
            IServiceProvider serviceProvider )
        {
            CompileTimeProjectLoader loader = new( domain, serviceProvider );

            return loader;
        }

        /// <summary>
        /// Gets a compile-time reflection <see cref="Type"/> given its Roslyn symbol.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Type? GetCompileTimeNamedType( INamedTypeSymbol typeSymbol, CancellationToken cancellationToken = default )
        {
            // Check if the type is a .NET system one.
            var systemType = this._systemTypeResolver.GetCompileTimeType( typeSymbol, false, cancellationToken );

            if ( systemType != null )
            {
                return systemType;
            }

            // The type is not a system one. Check if it is a compile-time one.
            if ( !this.Cache.TryGetValue( typeSymbol, out var type ) )
            {
                var assemblySymbol = typeSymbol.ContainingAssembly;

                var compileTimeProject = this.GetCompileTimeProject( assemblySymbol.Identity, cancellationToken );

                var reflectionName = typeSymbol.GetReflectionName();

                if ( reflectionName == null )
                {
                    return null;
                }

                type = compileTimeProject?.GetTypeOrNull( reflectionName );

                this.Cache.Add( typeSymbol, type );
            }

            return type;
        }

        /// <summary>
        /// Gets the <see cref="CompileTimeProject"/> for a given <see cref="AssemblyIdentity"/>,
        /// or <c>null</c> if it does not exist. 
        /// </summary>
        private CompileTimeProject? GetCompileTimeProject( AssemblyIdentity runTimeAssemblyIdentity, CancellationToken cancellationToken )
        {
            // This method is a smell and should probably not exist.

            _ = this.TryGetCompileTimeProject( runTimeAssemblyIdentity, NullDiagnosticAdder.Instance, false, cancellationToken, out var assembly );

            return assembly;
        }

        /// <summary>
        /// Tries to get the <see cref="CompileTimeProject"/> given its <see cref="AssemblyIdentity"/>.
        /// </summary>
        private bool TryGetCompileTimeProject(
            AssemblyIdentity runTimeAssemblyIdentity,
            IDiagnosticAdder diagnosticAdder,
            bool cacheOnly,
            CancellationToken cancellationToken,
            out CompileTimeProject? compileTimeProject )
        {
            if ( this._projects.TryGetValue( runTimeAssemblyIdentity, out compileTimeProject ) )
            {
                return true;
            }
            else
            {
                if ( this._runTimeAssemblyLocator.TryFindAssembly( runTimeAssemblyIdentity, out var metadataReference ) != true )
                {
                    var diagnostic = GeneralDiagnosticDescriptors.CannotFindCompileTimeAssembly.CreateRoslynDiagnostic(
                        Location.None,
                        runTimeAssemblyIdentity );

                    diagnosticAdder.Report( diagnostic );
                    this._logger.Warning?.Log( diagnostic.ToString() );

                    compileTimeProject = null;

                    return false;
                }

                return this.TryGetCompileTimeProject( metadataReference, diagnosticAdder, cacheOnly, cancellationToken, out compileTimeProject );
            }
        }

        /// <summary>
        /// Generates a <see cref="CompileTimeProject"/> for a given run-time <see cref="Compilation"/>.
        /// Referenced projects are loaded or generated as necessary. Note that other methods of this class do not
        /// generate projects, they will only ones that have been generated or loaded by this method.
        /// </summary>
        public bool TryGetCompileTimeProjectFromCompilation(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            IDiagnosticAdder diagnosticSink,
            bool cacheOnly,
            CancellationToken cancellationToken,
            out CompileTimeProject? compileTimeProject )
        {
            if ( this._projects.TryGetValue( runTimeCompilation.Assembly.Identity, out compileTimeProject ) )
            {
                return true;
            }

            List<CompileTimeProject> referencedProjects = new() { this._frameworkProject };

            foreach ( var reference in runTimeCompilation.References )
            {
                if ( this.TryGetCompileTimeProject( reference, diagnosticSink, cacheOnly, cancellationToken, out var referencedProject ) )
                {
                    if ( referencedProject != null )
                    {
                        referencedProjects.Add( referencedProject );
                    }
                }
                else
                {
                    // Coverage: ignore
                    // (this happens when the project reference could not be resolved.)

                    this._logger.Warning?.Log( $"The project reference from '{runTimeCompilation}' to' {reference}' could not be resolved." );
                    compileTimeProject = null;

                    return false;
                }
            }

            if ( !this._builder.TryGetCompileTimeProject(
                    runTimeCompilation,
                    compileTimeTreesHint,
                    referencedProjects,
                    diagnosticSink,
                    cacheOnly,
                    cancellationToken,
                    out compileTimeProject ) )
            {
                compileTimeProject = null;

                return false;
            }

            this._projects.Add( runTimeCompilation.Assembly.Identity, compileTimeProject );

            return true;
        }

        private bool TryGetCompileTimeProject(
            MetadataReference reference,
            IDiagnosticAdder diagnosticSink,
            bool cacheOnly,
            CancellationToken cancellationToken,
            out CompileTimeProject? referencedProject )
        {
            switch ( reference )
            {
                case PortableExecutableReference { FilePath: { } filePath }:
                    return this.TryGetCompileTimeProjectFromPath( filePath, diagnosticSink, cancellationToken, out referencedProject );

                case CompilationReference compilationReference:
                    return this.TryGetCompileTimeProjectFromCompilation(
                        compilationReference.Compilation,
                        null,
                        diagnosticSink,
                        cacheOnly,
                        cancellationToken,
                        out referencedProject );

                default:
                    throw new AssertionFailedException( $"Unexpected reference kind: {reference}." );
            }
        }

        private bool TryGetCompileTimeProjectFromPath(
            string assemblyPath,
            IDiagnosticAdder diagnosticSink,
            CancellationToken cancellationToken,
            out CompileTimeProject? compileTimeProject )
        {
            if ( !File.Exists( assemblyPath ) )
            {
                this._logger.Warning?.Log( $"The file '{assemblyPath}' does not exist." );
                
                compileTimeProject = null;
                return false;
            }
            
            var assemblyIdentity = AssemblyName.GetAssemblyName( assemblyPath ).ToAssemblyIdentity();

            // If the assembly is a standard one, there is no need to analyze.
            if ( this._serviceProvider.GetRequiredService<ReferenceAssemblyLocator>().StandardAssemblyNames.Contains( assemblyIdentity.Name ) )
            {
                compileTimeProject = null;

                return true;
            }

            // Look in our cache.
            if ( this._projects.TryGetValue( assemblyIdentity, out compileTimeProject ) )
            {
                return true;
            }

            // LoadFromAssemblyPath throws for mscorlib
            if ( Path.GetFileNameWithoutExtension( assemblyPath ) == typeof(object).Assembly.GetName().Name )
            {
                goto finish;
            }

            if ( !ManagedResourceReader.TryGetCompileTimeResource( assemblyPath, out var resources )
                 || !resources.TryGetValue( CompileTimeConstants.CompileTimeProjectResourceName, out var resourceBytes ) )
            {
                goto finish;
            }

            var assemblyName = AssemblyName.GetAssemblyName( assemblyPath );

            if ( !this.TryDeserializeCompileTimeProject(
                    assemblyName.ToAssemblyIdentity(),
                    new MemoryStream( resourceBytes ),
                    diagnosticSink,
                    cancellationToken,
                    out compileTimeProject ) )
            {
                // Coverage: ignore

                return false;
            }

        finish:
            this._projects.Add( assemblyIdentity, compileTimeProject );

            return true;
        }

        private bool TryDeserializeCompileTimeProject(
            AssemblyIdentity runTimeAssemblyIdentity,
            Stream resourceStream,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out CompileTimeProject? project )
        {
            using var archive = new ZipArchive( resourceStream, ZipArchiveMode.Read, true, Encoding.UTF8 );

            // Read manifest.
            var manifestEntry = archive.GetEntry( "manifest.json" ).AssertNotNull();

            var manifest = CompileTimeProjectManifest.Deserialize( manifestEntry.Open() );

            // Read source files.
            List<SyntaxTree> syntaxTrees = new();

            foreach ( var entry in archive.Entries.Where( e => string.Equals( Path.GetExtension( e.Name ), ".cs", StringComparison.OrdinalIgnoreCase ) ) )
            {
                using var sourceReader = new StreamReader( entry.Open(), Encoding.UTF8 );
                var sourceText = sourceReader.ReadToEnd();
                var syntaxTree = CSharpSyntaxTree.ParseText( sourceText, CSharpParseOptions.Default ).WithFilePath( entry.FullName );
                syntaxTrees.Add( syntaxTree );
            }

            // Resolve references.
            List<CompileTimeProject> referenceProjects = new();

            if ( manifest.References != null )
            {
                foreach ( var referenceSerializedIdentity in manifest.References )
                {
                    var referenceAssemblyIdentity = new AssemblyName( referenceSerializedIdentity ).ToAssemblyIdentity();

                    if ( !this.TryGetCompileTimeProject( referenceAssemblyIdentity, diagnosticAdder, false, cancellationToken, out var referenceProject ) )
                    {
                        // Coverage: ignore
                        // (this happens when the project reference could not be resolved.)

                        project = null;

                        return false;
                    }

                    if ( referenceProject != null )
                    {
                        referenceProjects.Add( referenceProject );
                    }
                }
            }

            // Deserialize the project.
            if ( !this._builder.TryCompileDeserializedProject(
                    runTimeAssemblyIdentity.Name,
                    syntaxTrees,
                    manifest.SourceHash,
                    referenceProjects,
                    diagnosticAdder,
                    cancellationToken,
                    out var assemblyPath,
                    out var sourceDirectory ) )
            {
                // Coverage: ignore
                // (this happens when the compile-time could not be compiled into a binary assembly.)

                project = null;

                return false;
            }

            // Compute the new hash.

            project = CompileTimeProject.Create(
                this._serviceProvider,
                this._domain,
                runTimeAssemblyIdentity,
                new AssemblyIdentity( manifest.CompileTimeAssemblyName ),
                referenceProjects,
                manifest,
                assemblyPath,
                sourceDirectory,
                TextMapFile.ReadForSource );

            return true;
        }
    }
}