// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.Impl.Templating.Mapping;
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

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// This class is responsible to cache and load compile-time projects. The caller must first call
    /// the <see cref="TryGetCompileTimeProject(Microsoft.CodeAnalysis.Compilation,Caravela.Framework.Impl.Diagnostics.IDiagnosticAdder,bool,out Caravela.Framework.Impl.CompileTime.CompileTimeProject?)"/> for each project with which the loader will be used.
    /// The generation of compile-time compilations itself is delegated to the <see cref="CompileTimeCompilationBuilder"/>
    /// class.
    /// </summary>
    internal sealed class CompileTimeProjectLoader : ICompileTimeTypeResolver
    {
        private readonly CompileTimeDomain _domain;
        private readonly CompileTimeCompilationBuilder _builder;
        private readonly IAssemblyLocator? _runTimeAssemblyLocator;
        private readonly SystemTypeResolver _systemTypeResolver = new();

        // Maps the identity of the run-time project to the compile-time project.
        private readonly Dictionary<AssemblyIdentity, CompileTimeProject?> _projects = new();

        public AttributeDeserializer AttributeDeserializer { get; }

        private CompileTimeProjectLoader( CompileTimeDomain domain, IServiceProvider serviceProvider )
        {
            this._domain = domain;
            this._builder = new CompileTimeCompilationBuilder( serviceProvider, domain );
            this._runTimeAssemblyLocator = serviceProvider.GetOptionalService<IAssemblyLocator>();

            this.AttributeDeserializer = new AttributeDeserializer( this );
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

        /// <summary>
        /// Gets a compile-time reflection <see cref="Type"/> given its Roslyn symbol.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="fallbackToMock">Determines whether a <see cref="CompileTimeType"/> must be returned
        /// when a compile-time does not exist.</param>
        /// <returns></returns>
        public Type? GetCompileTimeType( ITypeSymbol typeSymbol, bool fallbackToMock )
        {
            // Check if the type is a .NET system one.
            var systemType = this._systemTypeResolver.GetCompileTimeType( typeSymbol, false );

            if ( systemType != null )
            {
                return systemType;
            }

            // The type is not a system one. Check if it is a compile-time one.

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

            var compileTimeProject = this.GetCompileTimeProject( assemblySymbol.Identity );

            var result = compileTimeProject?.GetType( typeSymbol.GetReflectionNameSafe() );

            if ( result == null )
            {
                if ( fallbackToMock )
                {
                    result = CompileTimeType.Create( typeSymbol );
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

        /// <summary>
        /// Gets the <see cref="CompileTimeProject"/> for a given <see cref="AssemblyIdentity"/>,
        /// or <c>null</c> if it does not exist. 
        /// </summary>
        public CompileTimeProject? GetCompileTimeProject( AssemblyIdentity runTimeAssemblyIdentity )
        {
            // This method is a smell and should probably not exist.

            _ = this.TryGetCompileTimeProject( runTimeAssemblyIdentity, NullDiagnosticAdder.Instance, false, out var assembly );

            return assembly;
        }

        /// <summary>
        /// Tries to get the <see cref="CompileTimeProject"/> given its <see cref="AssemblyIdentity"/>.
        /// </summary>
        public bool TryGetCompileTimeProject(
            AssemblyIdentity runTimeAssemblyIdentity,
            IDiagnosticAdder diagnosticAdder,
            bool cacheOnly,
            out CompileTimeProject? compileTimeProject )
        {
            if ( this._projects.TryGetValue( runTimeAssemblyIdentity, out compileTimeProject ) )
            {
                return true;
            }
            else
            {
                MetadataReference? metadataReference = null;

                if ( this._runTimeAssemblyLocator?.TryFindAssembly( runTimeAssemblyIdentity, out metadataReference ) != true )
                {
                    diagnosticAdder.Report(
                        GeneralDiagnosticDescriptors.CannotFindCompileTimeAssembly.CreateDiagnostic(
                            Location.None,
                            runTimeAssemblyIdentity ) );

                    compileTimeProject = null;

                    return false;
                }

                return this.TryGetCompileTimeProject( metadataReference!, diagnosticAdder, cacheOnly, out compileTimeProject );
            }
        }


        /// <summary>
        /// Generates a <see cref="CompileTimeProject"/> for a given run-time <see cref="Compilation"/>.
        /// Referenced projects are loaded or generated as necessary. Note that other methods of this class do not
        /// generate projects, they will only ones that have been generated or loaded by this method.
        /// </summary>
        public bool TryGetCompileTimeProject(
            Compilation runTimeCompilation,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            IDiagnosticAdder diagnosticSink,
            bool cacheOnly,
            out CompileTimeProject? compileTimeProject )
        {
            if ( this._projects.TryGetValue( runTimeCompilation.Assembly.Identity, out compileTimeProject ) )
            {
                return true;
            }

            List<CompileTimeProject> referencedProjects = new();

            foreach ( var reference in runTimeCompilation.References )
            {
                if ( this.TryGetCompileTimeProject( reference, diagnosticSink, cacheOnly, out var referencedProject ) )
                {
                    if ( referencedProject != null )
                    {
                        referencedProjects.Add( referencedProject );
                    }
                }
                else
                {
                    compileTimeProject = null;

                    return false;
                }
            }

            if ( !this._builder.TryGetCompileTimeProject( runTimeCompilation, compileTimeTreesHint, referencedProjects, diagnosticSink, out compileTimeProject, cacheOnly ) )
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
            out CompileTimeProject? referencedProject )
        {
            switch ( reference )
            {
                case PortableExecutableReference { FilePath: { } filePath }:
                    return this.TryGetCompileTimeProject( filePath!, diagnosticSink, out referencedProject );

                case CompilationReference compilationReference:
                    return this.TryGetCompileTimeProject( compilationReference.Compilation,null, diagnosticSink, cacheOnly, out referencedProject );

                default:
                    throw new AssertionFailedException( $"Unexpected reference kind: {reference}." );
            }
        }

        private bool TryGetCompileTimeProject(
            string assemblyPath,
            IDiagnosticAdder diagnosticSink,
            out CompileTimeProject? compileTimeProject )
        {
            var assemblyIdentity = AssemblyName.GetAssemblyName( assemblyPath ).ToAssemblyIdentity();

            // If the assembly is a standard one, there is no need to analyze.
            if ( ReferenceAssemblyLocator.GetInstance().StandardAssemblyNames.Contains( assemblyIdentity.Name ) )
            {
                compileTimeProject = null;

                return true;
            }

            // Look in our cache.
            if ( this._projects.TryGetValue( assemblyIdentity, out compileTimeProject ) )
            {
                return true;
            }

            var resolver = new PathAssemblyResolver( new[] { typeof(object).Assembly.Location } );
            using var metadataLoadContext = new MetadataLoadContext( resolver, typeof(object).Assembly.GetName().Name );

            // LoadFromAssemblyPath throws for mscorlib
            if ( Path.GetFileNameWithoutExtension( assemblyPath ) == typeof(object).Assembly.GetName().Name )
            {
                compileTimeProject = null;

                return true;
            }

            var runtimeAssembly = metadataLoadContext.LoadFromAssemblyPath( assemblyPath );

            if ( !runtimeAssembly.GetManifestResourceNames().Contains( CompileTimeCompilationBuilder.ResourceName ) )
            {
                compileTimeProject = null;

                return true;
            }

            var stream = runtimeAssembly.GetManifestResourceStream( CompileTimeCompilationBuilder.ResourceName ).AssertNotNull();
            var assemblyName = runtimeAssembly.GetName();

            if ( !this.TryDeserializeCompileTimeProject(
                assemblyName.ToAssemblyIdentity(),
                stream,
                diagnosticSink,
                out compileTimeProject ) )
            {
                return false;
            }

            this._projects.Add( assemblyIdentity, compileTimeProject );

            return true;
        }

        private bool TryDeserializeCompileTimeProject(
            AssemblyIdentity assemblyIdentity,
            Stream resourceStream,
            IDiagnosticAdder diagnosticAdder,
            [NotNullWhen( true )] out CompileTimeProject? project )
        {
            using var archive = new ZipArchive( resourceStream, ZipArchiveMode.Read, true, Encoding.UTF8 );

            // Read manifest.
            var manifestEntry = archive.GetEntry( "manifest.json" ).AssertNotNull();

            if ( !CompileTimeProjectManifest.TryDeserialize( manifestEntry.Open(), out var manifest ) )
            {
                diagnosticAdder.Report(
                    GeneralDiagnosticDescriptors.InvalidCompileTimeProjectResource.CreateDiagnostic( Location.None, assemblyIdentity.ToString() ) );
                
                project = null;
                return false;
            }

            // Read source files.
            List<SyntaxTree> syntaxTrees = new();

            foreach ( var entry in archive.Entries.Where( e => Path.GetExtension( e.Name ) == ".cs" ) )
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

                    if ( !this.TryGetCompileTimeProject( referenceAssemblyIdentity, diagnosticAdder, false, out var referenceProject ) )
                    {
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
                assemblyIdentity.Name,
                syntaxTrees,
                referenceProjects,
                diagnosticAdder,
                out var assemblyPath,
                out var sourceDirectory ) )
            {
                project = null;

                return false;
            }

            // Compute the new hash.
            var compileTimeAssemblyName =
                CompileTimeCompilationBuilder.GetCompileTimeAssemblyName( manifest.AssemblyName, referenceProjects, manifest.SourceHash, null );

            project = CompileTimeProject.Create(
                this._domain,
                assemblyIdentity,
                new AssemblyIdentity( compileTimeAssemblyName ),
                referenceProjects,
                manifest,
                assemblyPath,
                sourceDirectory,
                TextMap.Read );

            return true;
        }
    }
}