// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
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
    /// the <see cref="TryGenerateCompileTimeProject"/> for each project with which the loader will be used.
    /// The generation of compile-time compilations itself is delegated to the <see cref="CompileTimeCompilationBuilder"/>
    /// class.
    /// </summary>
    internal sealed class CompileTimeProjectLoader : ICompileTimeTypeResolver
    {
        private readonly CompileTimeDomain _domain;
        private readonly CompileTimeCompilationBuilder _builder;
        private readonly Dictionary<string, (AssemblyIdentity RunTimeIdentity, CompileTimeProject? Project)> _projects = new();

        public AttributeDeserializer AttributeDeserializer { get; }

        private readonly SystemTypeResolver _systemTypeResolver = new();

        private CompileTimeProjectLoader( CompileTimeDomain domain, IServiceProvider serviceProvider )
        {
            this._domain = domain;
            this._builder = new CompileTimeCompilationBuilder( serviceProvider, domain );

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

            var result = compileTimeProject?.GetType( typeSymbol.GetReflectionName() );

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

        /// <summary>
        /// Gets the <see cref="CompileTimeProject"/> for a given <see cref="AssemblyIdentity"/>,
        /// or <c>null</c> if it does not exist. 
        /// </summary>
        public CompileTimeProject? GetCompileTimeProject( AssemblyIdentity runTimeAssemblyIdentity )
        {
            _ = this.TryGetCompileTimeProject( runTimeAssemblyIdentity, out var assembly );

            return assembly;
        }

        /// <summary>
        /// Tries to get the <see cref="CompileTimeProject"/> given its <see cref="AssemblyIdentity"/>.
        /// </summary>
        public bool TryGetCompileTimeProject(
            AssemblyIdentity runTimeAssemblyIdentity,
            out CompileTimeProject? compileTimeProject )
        {
            if ( this._projects.TryGetValue( runTimeAssemblyIdentity.Name, out var cached )
                 && cached.RunTimeIdentity == runTimeAssemblyIdentity )
            {
                compileTimeProject = cached.Project;

                return true;
            }
            else
            {
                compileTimeProject = null;

                return false;
            }
        }

        /// <summary>
        /// Tries to get the <see cref="CompileTimeProject"/> given the non-qualified assembly name.
        /// </summary>
        public bool TryGetCompileTimeProject( string referenceName, out CompileTimeProject? project )
        {
            if ( !this._projects.TryGetValue( referenceName, out var cached ) )
            {
                project = null;

                return false;
            }
            else
            {
                project = cached.Project;

                return true;
            }
        }

        /// <summary>
        /// Generates a <see cref="CompileTimeProject"/> for a given run-time <see cref="Compilation"/>.
        /// Referenced projects are loaded or generated as necessary. Note that other methods of this class do not
        /// generate projects, they will only ones that have been generated or loaded by this method.
        /// </summary>
        public bool TryGenerateCompileTimeProject(
            Compilation runTimeCompilation,
            IDiagnosticAdder diagnosticSink,
            out CompileTimeProject? compileTimeProject )
        {
            if ( this._projects.TryGetValue( runTimeCompilation.AssemblyName.AssertNotNull(), out var cached ) )
            {
                if ( cached.RunTimeIdentity != runTimeCompilation.Assembly.Identity )
                {
                    throw new AssertionFailedException();
                }

                compileTimeProject = cached.Project;

                return true;
            }

            List<CompileTimeProject> referencedProjects = new();

            foreach ( var reference in runTimeCompilation.References )
            {
                bool success;
                CompileTimeProject? referencedProject;

                switch ( reference )
                {
                    case PortableExecutableReference { FilePath: { } filePath }:
                        success = this.TryGetCompileTimeProject( filePath!, diagnosticSink, out referencedProject );

                        break;

                    case CompilationReference compilationReference:
                        success = this.TryGenerateCompileTimeProject( compilationReference.Compilation, diagnosticSink, out referencedProject );

                        break;

                    default:
                        throw new AssertionFailedException( $"Unexpected reference kind: {reference}." );
                }

                if ( success )
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

            if ( !this._builder.TryCreateCompileTimeProject( runTimeCompilation, referencedProjects, diagnosticSink, out compileTimeProject ) )
            {
                compileTimeProject = null;

                return false;
            }

            this._projects.Add( runTimeCompilation.AssemblyName!, (runTimeCompilation.Assembly.Identity, compileTimeProject) );

            return true;
        }

        private bool TryGetCompileTimeProject(
            string assemblyPath,
            IDiagnosticAdder diagnosticSink,
            out CompileTimeProject? compileTimeProject )
        {
            var assemblyIdentity = AssemblyName.GetAssemblyName( assemblyPath ).ToAssemblyIdentity();

            if ( this._projects.TryGetValue( assemblyIdentity.Name, out var cached ) )
            {
                if ( cached.RunTimeIdentity != assemblyIdentity )
                {
                    throw new AssertionFailedException();
                }

                compileTimeProject = cached.Project;

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

            this._projects.Add( assemblyIdentity.Name, (assemblyIdentity, compileTimeProject) );

            return true;
        }

        private bool TryDeserializeCompileTimeProject(
            AssemblyIdentity assemblyIdentity,
            Stream stream,
            IDiagnosticAdder diagnosticAdder,
            [NotNullWhen( true )] out CompileTimeProject? project )
        {
            using var archive = new ZipArchive( stream, ZipArchiveMode.Read, true, Encoding.UTF8 );

            // Read manifest.
            var manifestEntry = archive.GetEntry( "manifest.json" ).AssertNotNull();
            using var manifestReader = new StreamReader( manifestEntry.Open(), Encoding.UTF8 );
            var manifestJson = manifestReader.ReadToEnd();
            var manifest = JsonConvert.DeserializeObject<CompileTimeProjectManifest>( manifestJson ).AssertNotNull();

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
                foreach ( var referenceName in manifest.References )
                {
                    if ( !this.TryGetCompileTimeProject( referenceName, out var referenceProject ) )
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

            if ( !this._builder.TryCompileDeserializedProject(
                assemblyIdentity.Name,
                manifest,
                syntaxTrees,
                referenceProjects,
                diagnosticAdder,
                out var compilation,
                out var memoryStream ) )
            {
                project = null;

                return false;
            }

            project = CompileTimeProject.Create( this._domain, assemblyIdentity, referenceProjects, manifest, compilation, memoryStream.ToArray(), null );

            return true;
        }
    }
}