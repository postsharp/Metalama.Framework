﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.CompileTime.Manifest;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating.Mapping;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;

namespace Metalama.Framework.Engine.CompileTime;

internal sealed partial class CompileTimeProjectRepository
{
    /// <summary>
    /// Returns a new <see cref="CompileTimeProjectRepository"/>.
    /// </summary>
    public static CompileTimeProjectRepository? Create(
        CompileTimeDomain domain,
        ProjectServiceProvider serviceProvider,
        Compilation compilation,
        IDiagnosticAdder? diagnostics = null,
        bool cacheOnly = false,
        ProjectLicenseInfo? projectLicenseInfo = null,
        IReadOnlyList<SyntaxTree>? compileTimeTreesHint = null,
        CancellationToken cancellationToken = default )
    {
        diagnostics ??= NullDiagnosticAdder.Instance;

        var builder = new Builder( domain, serviceProvider, compilation );

        if ( !builder.TryBuild( compilation, projectLicenseInfo, compileTimeTreesHint, diagnostics, cacheOnly, cancellationToken, out var repository ) )
        {
            return null;
        }

        return repository;
    }

    // This class is made internal for tests only.
    internal sealed class Builder
    {
        private readonly CompileTimeCompilationBuilder _builder;
        private readonly CompileTimeProject _frameworkProject;
        private readonly ILogger _logger;

        private readonly ProjectServiceProvider _serviceProvider;

        // The dictionary may contain null values when the assembly does not reference Metalama.Framework.
        private readonly Dictionary<AssemblyIdentity, CompileTimeProject?> _projects = new();
        private readonly CompileTimeDomain _domain;
        private readonly IAssemblyLocator _runTimeAssemblyLocator;
        private readonly CacheableTemplateDiscoveryContextProvider _cacheableTemplateDiscoveryContextProvider;
        private readonly ClassifyingCompilationContextFactory _classifyingCompilationContextFactory;

        private static Compilation CreateEmptyCompilation( ProjectServiceProvider serviceProvider )
        {
            var assemblyLocator = serviceProvider.GetReferenceAssemblyLocator();

            return CSharpCompilation.Create( "empty", references: assemblyLocator.StandardCompileTimeMetadataReferences );
        }

        // This constructor is used in tests.
        public Builder(
            CompileTimeDomain domain,
            ProjectServiceProvider serviceProvider ) : this( domain, serviceProvider, CreateEmptyCompilation( serviceProvider ) ) { }

        public Builder(
            CompileTimeDomain domain,
            ProjectServiceProvider serviceProvider,
            Compilation compilation )
        {
            this._serviceProvider = serviceProvider;
            this._cacheableTemplateDiscoveryContextProvider = new CacheableTemplateDiscoveryContextProvider( compilation, serviceProvider );

            this._classifyingCompilationContextFactory = serviceProvider.GetRequiredService<ClassifyingCompilationContextFactory>();

            this._runTimeAssemblyLocator = serviceProvider.GetRequiredService<IAssemblyLocator>();
            this._domain = domain;
            this._logger = serviceProvider.GetLoggerFactory().CompileTime();

            this._frameworkProject = serviceProvider.Global.GetRequiredService<FrameworkCompileTimeProjectFactory>()
                .CreateFrameworkProject( serviceProvider, domain, compilation );

            this._projects.Add( this._frameworkProject.RunTimeIdentity, this._frameworkProject );
            this._builder = new CompileTimeCompilationBuilder( serviceProvider, domain );
        }

        public bool TryBuild(
            Compilation compilation,
            ProjectLicenseInfo? projectLicenseInfo,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            IDiagnosticAdder diagnosticSink,
            bool cacheOnly,
            CancellationToken cancellationToken,
            out CompileTimeProjectRepository? loader )
        {
            var compilationContext = this._classifyingCompilationContextFactory.GetInstance( compilation );

            if ( !this.TryGetCompileTimeProjectFromCompilation(
                    compilationContext,
                    projectLicenseInfo,
                    compileTimeTreesHint,
                    diagnosticSink,
                    cacheOnly,
                    cancellationToken,
                    out var compileTimeProject ) )
            {
                loader = null;

                return false;
            }

            if ( compileTimeProject == null )
            {
                throw new AssertionFailedException( $"Metalama is not enabled for the project '{compilation.AssemblyName}'." );
            }

            loader = new CompileTimeProjectRepository(
                this._domain,
                this._serviceProvider,
                this._projects,
                compileTimeProject );

            return true;
        }

        // This method is only used in tests.
        internal bool TryGetCompileTimeProjectFromCompilation(
            Compilation compilation,
            ProjectLicenseInfo? projectLicenseInfo,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            IDiagnosticAdder diagnosticSink,
            bool cacheOnly,
            CancellationToken cancellationToken,
            out CompileTimeProject? compileTimeProject )
            => this.TryGetCompileTimeProjectFromCompilation(
                this._classifyingCompilationContextFactory.GetInstance( compilation ),
                projectLicenseInfo,
                compileTimeTreesHint,
                diagnosticSink,
                cacheOnly,
                cancellationToken,
                out compileTimeProject );

        /// <summary>
        /// Generates a <see cref="CompileTimeProject"/> for a given run-time <see cref="Compilation"/>.
        /// Referenced projects are loaded or generated as necessary. Note that other methods of this class do not
        /// generate projects, they will only ones that have been generated or loaded by this method.
        /// </summary>
        private bool TryGetCompileTimeProjectFromCompilation(
            ClassifyingCompilationContext compilationContext,
            ProjectLicenseInfo? projectLicenseInfo,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            IDiagnosticAdder diagnosticSink,
            bool cacheOnly,
            CancellationToken cancellationToken,
            out CompileTimeProject? compileTimeProject )
        {
            var runTimeCompilation = compilationContext.SourceCompilation;

            if ( this._projects.TryGetValue( runTimeCompilation.Assembly.Identity, out compileTimeProject ) )
            {
                return true;
            }

            List<CompileTimeProject> referencedProjects = new() { this._frameworkProject };

            foreach ( var reference in runTimeCompilation.References )
            {
                if ( this.TryGetCompileTimeProject(
                        reference,
                        diagnosticSink,
                        cacheOnly,
                        cancellationToken,
                        out var referencedProject ) )
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

                    this._logger.Warning?.Log(
                        $"The project reference from '{runTimeCompilation.AssemblyName}' to' {reference.Display}' could not be resolved." );

                    compileTimeProject = null;

                    return false;
                }
            }

            if ( !this._builder.TryGetCompileTimeProject(
                    compilationContext,
                    projectLicenseInfo,
                    compileTimeTreesHint,
                    referencedProjects,
                    diagnosticSink,
                    cacheOnly,
                    out compileTimeProject,
                    cancellationToken ) )
            {
                this._logger.Warning?.Log( $"TryGetCompileTimeProject failed." );

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
                    return this.TryGetCompileTimeProjectFromPath(
                        filePath,
                        diagnosticSink,
                        cacheOnly,
                        cancellationToken,
                        out referencedProject );

                case CompilationReference compilationReference:
                    var compilationContext = this._classifyingCompilationContextFactory.GetInstance( compilationReference.Compilation );

                    return this.TryGetCompileTimeProjectFromCompilation(
                        compilationContext,
                        null,
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
            bool cacheOnly,
            CancellationToken cancellationToken,
            out CompileTimeProject? compileTimeProject )
        {
            if ( !File.Exists( assemblyPath ) )
            {
                this._logger.Warning?.Log( $"The file '{assemblyPath}' does not exist." );

                compileTimeProject = null;

                return false;
            }

            var assemblyIdentity = MetadataReferenceCache.GetAssemblyName( assemblyPath ).ToAssemblyIdentity();

            // If the assembly is a standard one, there is no need to analyze.
            if ( this._serviceProvider.GetReferenceAssemblyLocator().StandardAssemblyNames.Contains( assemblyIdentity.Name ) )
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

            // Performance trick: do not analyze system assemblies.
            var assemblyFileName = Path.GetFileNameWithoutExtension( assemblyPath );

            if ( assemblyFileName.Equals( "System", StringComparison.OrdinalIgnoreCase ) ||
                 assemblyFileName.StartsWith( "System.", StringComparison.OrdinalIgnoreCase ) ||
                 assemblyFileName.StartsWith( "Microsoft.CodeAnalysis", StringComparison.OrdinalIgnoreCase ) )
            {
                goto finish;
            }

            if ( !MetadataReader.TryGetMetadata( assemblyPath, out var metadataInfo ) )
            {
                goto finish;
            }

            if ( metadataInfo.Resources.TryGetValue( CompileTimeConstants.CompileTimeProjectResourceName, out var resourceBytes ) )
            {
                this._cacheableTemplateDiscoveryContextProvider.OnPortableExecutableReferenceDiscovered();

                var assemblyName = MetadataReferenceCache.GetAssemblyName( assemblyPath );

                if ( !this.TryDeserializeCompileTimeProject(
                        assemblyName.ToAssemblyIdentity(),
                        new MemoryStream( resourceBytes ),
                        diagnosticSink,
                        cacheOnly,
                        this._cacheableTemplateDiscoveryContextProvider,
                        out compileTimeProject,
                        cancellationToken ) )
                {
                    this._logger.Warning?.Log( $"TryDeserializeCompileTimeProject failed." );

                    // Coverage: ignore

                    return false;
                }
            }
            else if ( metadataInfo.HasCompileTimeAttribute )
            {
                // We have an assembly that a [assembly: CompileTime] attribute but has no embedded compile-time project.
                // This is typically the case of public assemblies of weaver-based aspects or services.
                // These projects need to be included as compile-time projects. They typically have MetalamaRemoveCompileTimeOnlyCode=false.
                if ( !CompileTimeProject.TryCreateUntransformed(
                        this._serviceProvider,
                        this._domain,
                        assemblyIdentity,
                        assemblyPath,
                        this._cacheableTemplateDiscoveryContextProvider,
                        out compileTimeProject ) )
                {
                    this._logger.Trace?.Log(
                        $"The assembly '{assemblyIdentity}' will not be included in the compile-time compilation despite having an [assembly: CompileTime] attribute "
                        +
                        "because it has no compile-time embedded resource and it is not loaded as an analyzer." );
                }
            }

        finish:
            this._projects.Add( assemblyIdentity, compileTimeProject );

            return true;
        }

        private bool TryDeserializeCompileTimeProject(
            AssemblyIdentity runTimeAssemblyIdentity,
            Stream resourceStream,
            IDiagnosticAdder diagnosticAdder,
            bool cacheOnly,
            CacheableTemplateDiscoveryContextProvider? cacheableTemplateDiscoveryContextProvider,
            [NotNullWhen( true )] out CompileTimeProject? project,
            CancellationToken cancellationToken )
        {
            using var archive = new ZipArchive( resourceStream, ZipArchiveMode.Read, true, Encoding.UTF8 );

            // Read manifest.
            var manifestEntry = archive.GetEntry( "manifest.json" ).AssertNotNull();

            var manifest = CompileTimeProjectManifest.Deserialize( manifestEntry.Open() );

            // Check the manifest version.
            if ( manifest.ManifestVersion != CompileTimeProjectManifest.CurrentManifestVersion )
            {
                diagnosticAdder.Report(
                    GeneralDiagnosticDescriptors.DependencyMustBeRecompiled.CreateRoslynDiagnostic(
                        null,
                        (runTimeAssemblyIdentity, manifest.MetalamaVersion) ) );

                project = null;

                return false;
            }

            // Read source files.
            var parseOptions = SupportedCSharpVersions.DefaultParseOptions;

            List<SyntaxTree> syntaxTrees = new();

            foreach ( var entry in archive.Entries.Where( e => string.Equals( Path.GetExtension( e.Name ), ".cs", StringComparison.OrdinalIgnoreCase ) ) )
            {
                using var sourceReader = new StreamReader( entry.Open(), Encoding.UTF8 );
                var sourceText = sourceReader.ReadToEnd();
                var syntaxTree = CSharpSyntaxTree.ParseText( sourceText, parseOptions ).WithFilePath( entry.FullName );
                syntaxTrees.Add( syntaxTree );
            }

            // Resolve references.
            List<CompileTimeProject> referenceProjects = new();

            if ( manifest.References != null )
            {
                foreach ( var referenceSerializedIdentity in manifest.References )
                {
                    var referenceAssemblyIdentity = new AssemblyName( referenceSerializedIdentity ).ToAssemblyIdentity();

                    if ( !this.TryGetCompileTimeProject(
                            referenceAssemblyIdentity,
                            diagnosticAdder,
                            cacheOnly,
                            cancellationToken,
                            out var referenceProject ) )
                    {
                        // Coverage: ignore
                        // (this happens when the project reference could not be resolved.)

                        project = null;

                        this._logger.Warning?.Log(
                            $"TryDeserializeCompileTimeProject('{runTimeAssemblyIdentity}'): processing of reference '{referenceAssemblyIdentity}' failed." );

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
                    string.IsNullOrEmpty( manifest.TargetFramework ) ? null : new FrameworkName( manifest.TargetFramework ),
                    syntaxTrees,
                    manifest.SourceHash,
                    manifest.RedistributionLicenseKey,
                    referenceProjects,
                    diagnosticAdder,
                    cancellationToken,
                    out var assemblyPath,
                    out var sourceDirectory ) )
            {
                // Coverage: ignore
                // (this happens when the compile-time could not be compiled into a binary assembly.)

                this._logger.Warning?.Log( $"TryDeserializeCompileTimeProject('{runTimeAssemblyIdentity}'): TryCompileDeserializedProject failed'." );

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
                TextMapFile.ReadForSource,
                cacheableTemplateDiscoveryContextProvider );

            return true;
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

                return this.TryGetCompileTimeProject(
                    metadataReference,
                    diagnosticAdder,
                    cacheOnly,
                    cancellationToken,
                    out compileTimeProject );
            }
        }
    }
}