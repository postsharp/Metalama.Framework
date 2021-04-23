// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Caravela.Framework.Impl.CompileTime
{

    internal class CompileTimeProject
    {
        private readonly Compilation? _compilation;
        private readonly CompileTimeProjectManifest? _manifest;
        private readonly byte[]? _assemblyImage;
        private Assembly? _assembly;
        
        public CompileTimeDomain Domain { get; }

        public AssemblyIdentity RunTimeIdentity { get; }

        public AssemblyIdentity? CompileTimeIdentity => this._compilation?.Assembly.Identity;

        private CompileTimeProject(
            CompileTimeDomain domain,
            AssemblyIdentity runTimeIdentity,
            IReadOnlyList<CompileTimeProject> references,
            CompileTimeProjectManifest? manifest,
            Compilation? compilation,
            byte[]? assemblyImage)
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
        }

        public static CompileTimeProject Create(
            CompileTimeDomain domain,
            AssemblyIdentity identity,
            IReadOnlyList<CompileTimeProject> references,
            CompileTimeProjectManifest manifest,
            Compilation compilation,
            byte[] assemblyImage )
            => new( domain, identity, references, manifest, compilation, assemblyImage );

        public static CompileTimeProject CreateEmpty(
            CompileTimeDomain domain,
            AssemblyIdentity identity,
            IReadOnlyList<CompileTimeProject>? references = null )
            => new( domain, identity, references ?? Array.Empty<CompileTimeProject>(), null, null, null );

        public IEnumerable<string> AspectTypes => this.AssertNotEmpty()._manifest!.AspectTypes ?? Enumerable.Empty<string>();

        public IEnumerable<CompileTimeProject> References { get; }

        public CompilationReference ToMetadataReference() => this.AssertNotEmpty()._compilation!.ToMetadataReference();

        public ulong Hash => this.AssertNotEmpty()._manifest.Hash;

        public bool IsEmpty => this._compilation == null;

        private CompileTimeProject AssertNotEmpty()
        {
            if ( this.IsEmpty )
            {
                throw new InvalidOperationException();
            }

            return this;
        }

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

                return this._assembly;
            }
        }

        public override string ToString() => this.RunTimeIdentity.ToString();
    }
}