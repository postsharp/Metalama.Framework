// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        private readonly Compilation? _compilation;
        private readonly CompileTimeProjectManifest? _manifest;
        private readonly byte[]? _assemblyImage;
        private Assembly? _assembly;

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
        public AssemblyIdentity? CompileTimeIdentity => this._compilation?.Assembly.Identity;

        /// <summary>
        /// Gets the list of aspect types (identified by their fully qualified reflection name) of the aspects
        /// declared in the current project.
        /// </summary>
        public IReadOnlyList<string> AspectTypes => this.AssertNotEmpty()._manifest!.AspectTypes ?? (IReadOnlyList<string>) Array.Empty<string>();

        /// <summary>
        /// Gets the list of compile-time projects referenced by the current project.
        /// </summary>
        public IReadOnlyList<CompileTimeProject> References { get; }

        /// <summary>
        /// Gets the list of syntax trees of the current project. These syntax trees are fully transformed
        /// and ready to be compiled.
        /// </summary>
        public IReadOnlyList<SyntaxTree> SyntaxTrees { get; }

        /// <summary>
        /// Gets a <see cref="MetadataReference"/> corresponding to the current project.
        /// </summary>
        /// <returns></returns>
        public CompilationReference ToMetadataReference() => this.AssertNotEmpty()._compilation!.ToMetadataReference();

        /// <summary>
        /// Gets the unique hash of the project, computed from the source code.
        /// </summary>
        public ulong Hash => this.AssertNotEmpty()._manifest!.Hash;

        /// <summary>
        /// Gets a value indicating whether the current project is empty, i.e. does not contain any source code. Note that
        /// an empty project can STILL contain <see cref="References"/>.
        /// </summary>
        public bool IsEmpty => this._compilation == null;

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
            IReadOnlyList<CompileTimeProject> references,
            CompileTimeProjectManifest? manifest,
            Compilation? compilation,
            byte[]? assemblyImage,
            IReadOnlyList<SyntaxTree>? syntaxTrees )
        {
            if ( compilation == null && references.Count == 0 )
            {
                // We should not create an instance in this case. This scenario is represented by `null`.
                throw new AssertionFailedException();
            }

            this.Domain = domain;
            this._compilation = compilation;
            this._assemblyImage = assemblyImage;
            this._manifest = manifest;
            this.RunTimeIdentity = runTimeIdentity;
            this.References = references;
            this.SyntaxTrees = syntaxTrees ?? Array.Empty<SyntaxTree>();
        }

        /// <summary>
        /// Creates a <see cref="CompileTimeProject"/> that includes source code.
        /// </summary>
        public static CompileTimeProject Create(
            CompileTimeDomain domain,
            AssemblyIdentity identity,
            IReadOnlyList<CompileTimeProject> references,
            CompileTimeProjectManifest manifest,
            Compilation compilation,
            byte[] assemblyImage,
            IReadOnlyList<SyntaxTree>? syntaxTrees )
            => new( domain, identity, references, manifest, compilation, assemblyImage, syntaxTrees );

        /// <summary>
        /// Creates a <see cref="CompileTimeProject"/> that does not include any source code.
        /// </summary>
        public static CompileTimeProject CreateEmpty(
            CompileTimeDomain domain,
            AssemblyIdentity identity,
            IReadOnlyList<CompileTimeProject>? references = null )
            => new( domain, identity, references ?? Array.Empty<CompileTimeProject>(), null, null, null, null );

        /// <summary>
        /// Serializes the current project (its manifest and source code) into a stream that can be embedded as a managed resource.
        /// </summary>
        /// <returns></returns>
        public MemoryStream Serialize()
        {
            this.AssertNotEmpty();

            MemoryStream stream = new();

            using ( var archive = new ZipArchive( stream, ZipArchiveMode.Create, true, Encoding.UTF8 ) )
            {
                // Write syntax trees.
                var index = 0;

                foreach ( var syntaxTree in this._compilation!.SyntaxTrees )
                {
                    index++;
                    var filePath = syntaxTree.FilePath;

                    if ( string.IsNullOrEmpty( filePath ) )
                    {
                        filePath = $"File{index}.cs";
                    }

                    var entry = archive.CreateEntry( filePath, CompressionLevel.Optimal );
                    using var entryWriter = new StreamWriter( entry.Open(), syntaxTree.GetText().Encoding );
                    syntaxTree.GetText().Write( entryWriter );
                }

                // Write manifest.
                var manifestJson = JsonConvert.SerializeObject( this._manifest!, Formatting.Indented );
                var manifestEntry = archive.CreateEntry( "manifest.json", CompressionLevel.Optimal );
                using var manifestWriter = new StreamWriter( manifestEntry.Open(), Encoding.UTF8 );
                manifestWriter.Write( manifestJson );
            }

            stream.Seek( 0, SeekOrigin.Begin );

            return stream;
        }

        /// <summary>
        /// Gets a compile-time reflection <see cref="Type"/> defined in the current project.
        /// </summary>
        /// <param name="reflectionName"></param>
        /// <returns></returns>
        public Type? GetType( string reflectionName ) => this.IsEmpty ? null : this.Assembly!.GetType( reflectionName, false );

        private void LoadAssembly()
        {
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

                this._assembly = this.Domain.GetOrLoadAssembly( this.CompileTimeIdentity!, this._assemblyImage! );
            }
        }

        private Assembly Assembly
        {
            get
            {
                this.AssertNotEmpty();

                this.LoadAssembly();

                return this._assembly!;
            }
        }

        public override string ToString() => this.RunTimeIdentity.ToString();
    }
}