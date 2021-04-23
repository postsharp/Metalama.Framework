// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Caravela.Framework.Impl.CompileTime
{
    internal sealed class CompileTimeAssemblyLoader : ICompileTimeTypeResolver
    {
        private readonly CompileTimeDomain _domain;
        private readonly CSharpCompilation _compilation;
        private readonly CompileTimeCompilationBuilder _builder;
        private readonly Dictionary<string, (AssemblyIdentity Identity, CompileTimeProject? Project)> _projects = new();
        private readonly AttributeDeserializer _attributeDeserializer;
        private readonly SystemTypeResolver _systemTypeResolver = new();

        private CompileTimeAssemblyLoader(
            CompileTimeDomain domain,
            IServiceProvider serviceProvider,
            CSharpCompilation compilation )
        {
            this._domain = domain;
            this._compilation = compilation;
            this._builder = new CompileTimeCompilationBuilder( serviceProvider, domain );

            this._attributeDeserializer = new AttributeDeserializer( this );
        }

        public static CompileTimeAssemblyLoader Create(
            CompileTimeDomain domain,
            IServiceProvider serviceProvider,
            CSharpCompilation compilation )
        {
            CompileTimeAssemblyLoader loader = new( domain, serviceProvider, compilation );

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

        public bool TryCreateAttributeInstance( IAttribute attribute, IDiagnosticAdder diagnosticAdder, [NotNullWhen( true )] out Attribute? attributeInstance )
        {
            return this._attributeDeserializer.TryCreateAttribute( attribute, diagnosticAdder, out attributeInstance );
        }

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

            var compileTimeProject = this.GetCompileTimeProject( assemblySymbol );

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

        public CompileTimeProject? GetCompileTimeProject( IAssemblySymbol assemblySymbol )
        {
            DiagnosticList diagnostics = new();

            if ( !this.TryGetCompileTimeProject( assemblySymbol, diagnostics, out var assembly ) )
            {
                throw new InvalidUserCodeException( "Cannot compile the compile-time project.", diagnostics.ToImmutableArray() );
            }

            return assembly;
        }

        public bool TryGetCompileTimeProject(
            IAssemblySymbol assemblySymbol,
            IDiagnosticAdder diagnosticSink,
            [NotNullWhen( true )] out CompileTimeProject? compileTimeProject )
        {
            // Take the compile-time assembly of project references, if any.

            if ( assemblySymbol is ISourceAssemblySymbol sourceAssemblySymbol )
            {
                return this.TryGetCompileTimeProject( sourceAssemblySymbol.Compilation, diagnosticSink, out compileTimeProject );
            }
            else
            {
                if ( this._compilation.GetMetadataReference( assemblySymbol ) is not { } reference )
                {
                    throw new InvalidOperationException( $"Could not find reference for assembly {assemblySymbol} in the current context." );
                }

                switch ( reference )
                {
                    case CompilationReference compilationReference:
                        return this.TryGetCompileTimeProject( compilationReference.Compilation, diagnosticSink, out compileTimeProject );

                    case PortableExecutableReference peReference:
                        if ( peReference.FilePath != null )
                        {
                            return this.TryGetCompileTimeProject( peReference.FilePath, diagnosticSink, out compileTimeProject );
                        }
                        else
                        {
                            break;
                        }
                }
            }

            compileTimeProject = null;

            return false;
        }

        private bool TryGetCompileTimeProject(
            Compilation runTimeCompilation,
            IDiagnosticAdder diagnosticSink,
            out CompileTimeProject? compileTimeProject )
        {
            if ( this._projects.TryGetValue( runTimeCompilation.AssemblyName.AssertNotNull(), out var cached ) )
            {
                if ( cached.Identity != runTimeCompilation.Assembly.Identity )
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
                        success = this.TryGetCompileTimeProject( compilationReference.Compilation, diagnosticSink, out referencedProject );

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
                if ( cached.Identity != assemblyIdentity )
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
    }
}