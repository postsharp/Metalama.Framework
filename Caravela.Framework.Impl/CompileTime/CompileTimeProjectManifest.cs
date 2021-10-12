﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Fabrics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// A serializable object that stores the manifest of a <see cref="CompileTimeProject"/>. 
    /// </summary>
    [Obfuscation( Exclude = true /* JSON */ )]
    internal class CompileTimeProjectManifest
    {
        public CompileTimeProjectManifest(
            string runTimeAssemblyIdentity,
            string compileTimeAssemblyName,
            IReadOnlyList<string> aspectTypes,
            IReadOnlyList<string> plugInTypes,
            IReadOnlyList<string> fabricTypes,
            IReadOnlyList<string> transitiveFabricTypes,
            IReadOnlyList<string>? references,
            ulong sourceHash,
            IReadOnlyList<CompileTimeFile> files )
        {
            this.RunTimeAssemblyIdentity = runTimeAssemblyIdentity;
            this.CompileTimeAssemblyName = compileTimeAssemblyName;
            this.AspectTypes = aspectTypes;
            this.PlugInTypes = plugInTypes;
            this.FabricTypes = fabricTypes;
            this.TransitiveFabricTypes = transitiveFabricTypes;
            this.References = references;
            this.SourceHash = sourceHash;
            this.Files = files;
        }

        public string RunTimeAssemblyIdentity { get; }
        public string CompileTimeAssemblyName { get; }

        /// <summary>
        /// Gets the list of all aspect types (specified by fully qualified name) of the aspect library.
        /// </summary>
        public IReadOnlyList<string> AspectTypes { get; }

        /// <summary>
        /// Gets the list of types that are exported using the <c>CompilerPlugin</c> attribute.
        /// </summary>
        public IReadOnlyList<string> PlugInTypes { get; }

        /// <summary>
        /// Gets the list of types that implement the <see cref="IFabric"/> interface, but the <see cref="ITransitiveProjectFabric"/>.
        /// </summary>
        public IReadOnlyList<string> FabricTypes { get; }

        /// <summary>
        /// Gets the list of types that implement the <see cref="ITransitiveProjectFabric"/> interface.
        /// </summary>
        public IReadOnlyList<string> TransitiveFabricTypes { get; }

        /// <summary>
        /// Gets the name of all project references (a fully-qualified assembly identity) of the compile-time project.
        /// </summary>
        public IReadOnlyList<string>? References { get; }

        /// <summary>
        /// Gets a unique hash of the source code and its dependencies.
        /// </summary>
        public ulong SourceHash { get; }

        /// <summary>
        /// Gets the list of code files.
        /// </summary>
        public IReadOnlyList<CompileTimeFile> Files { get; }

        public static CompileTimeProjectManifest Deserialize( Stream stream )
        {
            using var manifestReader = new StreamReader( stream, Encoding.UTF8 );
            var manifestJson = manifestReader.ReadToEnd();
            stream.Close();

            var manifest = JsonConvert.DeserializeObject<CompileTimeProjectManifest>( manifestJson ).AssertNotNull();

            // Assert that files are properly deserialized.
            foreach ( var file in manifest.Files )
            {
                if ( file.SourcePath == null! || file.TransformedPath == null! )
                {
                    throw new AssertionFailedException( "Deserialization error." );
                }
            }

            return manifest;
        }

        public void Serialize( Stream stream )
        {
            var manifestJson = JsonConvert.SerializeObject( this, Newtonsoft.Json.Formatting.Indented );
            using var manifestWriter = new StreamWriter( stream, Encoding.UTF8 );
            manifestWriter.Write( manifestJson );
        }
    }
}