// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Templating.Mapping;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Caravela.Framework.Impl.CompileTime
{
    
    /// <summary>
    /// Represents the compile-time project extracted from a run-time project, including its
    /// <see cref="System.Reflection.Assembly"/> allowing for execution, and metadata.
    /// </summary>
    internal sealed class CompileTimeProject
    {
        private readonly CompileTimeProjectManifest? _manifest;
        private readonly string? _compiledAssemblyPath;
        private readonly Func<string, TextMap?>? _getLocationMap;
        private Assembly? _assembly;

        /// <summary>
        /// Gets the full path of the directory containing the transformed source code (typically under a temp directory). 
        /// </summary>
        public string? Directory { get; }

        /// <summary>
        /// Gets the <see cref="CompileTimeDomain"/> to which the current project belong.
        /// </summary>
        public CompileTimeDomain Domain { get; }

        /// <summary>
        /// Gets the identity of the run-time assembly for which this compile-time project was created.
        /// </summary>
        public AssemblyIdentity RunTimeIdentity { get; }

        /// <summary>
        /// Gets the identity of the compile-time assembly, which is guaranteed to be unique in the
        /// current <see cref="Domain"/> for a given source code.
        /// </summary>
        public AssemblyIdentity? CompileTimeIdentity { get; }

        /// <summary>
        /// Gets the list of aspect types (identified by their fully qualified reflection name) of the aspects
        /// declared in the current project.
        /// </summary>
        public IReadOnlyList<string> AspectTypes => this._manifest?.AspectTypes ?? Array.Empty<string>();

        /// <summary>
        /// Gets the list of compile-time projects referenced by the current project.
        /// </summary>
        public IReadOnlyList<CompileTimeProject> References { get; }

        /// <summary>
        /// Gets the list of transformed code files in the current project. 
        /// </summary>
        public IReadOnlyList<CompileTimeFile> CodeFiles => this._manifest?.Files ?? Array.Empty<CompileTimeFile>();

        /// <summary>
        /// Gets a <see cref="MetadataReference"/> corresponding to the current project.
        /// </summary>
        /// <returns></returns>
        public MetadataReference ToMetadataReference() => MetadataReference.CreateFromFile( this.AssertNotEmpty()._compiledAssemblyPath! );

        /// <summary>
        /// Gets the unique hash of the project, computed from the source code.
        /// </summary>
        public ulong Hash => this._manifest?.SourceHash ?? 0;

        /// <summary>
        /// Gets a value indicating whether the current project is empty, i.e. does not contain any source code. Note that
        /// an empty project can STILL contain <see cref="References"/>.
        /// </summary>
        public bool IsEmpty => this._compiledAssemblyPath == null;

        /// <summary>
        /// Gets the CLR <see cref="System.Reflection.Assembly"/>, and loads it if necessary.
        /// </summary>
        private Assembly Assembly
        {
            get
            {
                this.LoadAssembly();

                return this._assembly!;
            }
        }

        private CompileTimeProject AssertNotEmpty()
        {
            if ( this.IsEmpty )
            {
                throw new InvalidOperationException();
            }

            return this;
        }

        private CompileTimeProject(
            CompileTimeDomain domain,
            AssemblyIdentity runTimeIdentity,
            AssemblyIdentity compileTimeIdentity,
            IReadOnlyList<CompileTimeProject> references,
            CompileTimeProjectManifest? manifest,
            string? compiledAssemblyPath,
            Func<string, TextMap?>? getLocationMap,
            string? directory )
        {
            this.Domain = domain;
            this._compiledAssemblyPath = compiledAssemblyPath;
            this._getLocationMap = getLocationMap;
            this.Directory = directory;
            this._manifest = manifest;
            this.RunTimeIdentity = runTimeIdentity;
            this.CompileTimeIdentity = compileTimeIdentity;
            this.References = references;
        }

        /// <summary>
        /// Creates a <see cref="CompileTimeProject"/> that includes source code.
        /// </summary>
        public static CompileTimeProject Create(
            CompileTimeDomain domain,
            AssemblyIdentity runTimeIdentity,
            AssemblyIdentity compileTimeIdentity,
            IReadOnlyList<CompileTimeProject> references,
            CompileTimeProjectManifest manifest,
            string? compiledAssemblyPath,
            string sourceDirectory,
            Func<string, TextMap?> getLocationMap )
            => new( domain, runTimeIdentity, compileTimeIdentity, references, manifest, compiledAssemblyPath, getLocationMap, sourceDirectory );

        /// <summary>
        /// Creates a <see cref="CompileTimeProject"/> that does not include any source code.
        /// </summary>
        public static CompileTimeProject CreateEmpty(
            CompileTimeDomain domain,
            AssemblyIdentity runTimeIdentity,
            AssemblyIdentity compileTimeIdentity,
            IReadOnlyList<CompileTimeProject>? references = null )
            => new( domain, runTimeIdentity, compileTimeIdentity, references ?? Array.Empty<CompileTimeProject>(), null, null, null, null );

        /// <summary>
        /// Serializes the current project (its manifest and source code) into a stream that can be embedded as a managed resource.
        /// </summary>
        public void Serialize( Stream stream )
        {
            this.AssertNotEmpty();

            using ( var archive = new ZipArchive( stream, ZipArchiveMode.Create, true, Encoding.UTF8 ) )
            {
                // Write syntax trees.
                
                foreach ( var sourceFile in this.CodeFiles! )
                {
                    var sourceText = File.ReadAllText( Path.Combine( this.Directory, sourceFile.TransformedPath ) );
                   
                    var entry = archive.CreateEntry( sourceFile.TransformedPath, CompressionLevel.Optimal );
                    using var entryWriter = new StreamWriter( entry.Open() );
                    entryWriter.Write( sourceText );
                }

                // Write manifest.
                var manifestEntry = archive.CreateEntry( "manifest.json", CompressionLevel.Optimal );
                var manifestStream = manifestEntry.Open();
                this._manifest!.Serialize( manifestStream );
            }
        }

        /// <summary>
        /// Returns a managed resource that contains the serialized project.
        /// </summary>
        /// <returns></returns>
        public ResourceDescription ToResource()
            => new(
                CompileTimeCompilationBuilder.ResourceName,
                () =>
                {
                    var stream = new MemoryStream();
                    this.Serialize( stream );
                    _ = stream.Seek( 0, SeekOrigin.Begin );

                    return stream;
                },
                true );

        /// <summary>
        /// Gets a compile-time reflection <see cref="Type"/> defined in the current project.
        /// </summary>
        /// <param name="reflectionName"></param>
        /// <returns></returns>
        public Type? GetType( string reflectionName ) => this.IsEmpty ? null : this.Assembly!.GetType( reflectionName, false );

        public CompileTimeFile? FindCodeFileFromTransformedPath( string transformedCodePath )
            => this.CodeFiles.Where( t => transformedCodePath.EndsWith( t.TransformedPath, StringComparison.OrdinalIgnoreCase ) )
                .OrderByDescending( t => t.TransformedPath.Length )
                .FirstOrDefault();

        private void LoadAssembly()
        {
            this.AssertNotEmpty();

            if ( this._assembly == null )
            {
                // We need to recursively load all dependent assemblies to prevent FileNotFoundException.

                foreach ( var reference in this.References )
                {
                    if ( !reference.IsEmpty )
                    {
                        reference.LoadAssembly();
                    }
                }

                this._assembly = this.Domain.GetOrLoadAssembly( this.CompileTimeIdentity!, this._compiledAssemblyPath! );
            }
        }

        public override string ToString() => this.RunTimeIdentity.ToString();

        public TextMap? GetTextMap( string csFilePath ) => this._getLocationMap?.Invoke( csFilePath );
    }
}