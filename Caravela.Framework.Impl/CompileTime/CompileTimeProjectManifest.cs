// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
            string assemblyName,
            IReadOnlyList<string> aspectTypes,
            IReadOnlyList<string> compilerPlugIns,
            IReadOnlyList<string>? references,
            ulong sourceHash,
            IReadOnlyList<CompileTimeFile> files )
        {
            this.AssemblyName = assemblyName;
            this.AspectTypes = aspectTypes;
            this.CompilerPlugIns = compilerPlugIns;
            this.References = references;
            this.SourceHash = sourceHash;
            this.Files = files;
        }

        public string AssemblyName { get; }

        /// <summary>
        /// Gets the list of all aspect types (specified by fully qualified name) of the aspect library.
        /// </summary>
        public IReadOnlyList<string> AspectTypes { get; }

        public IReadOnlyList<string> CompilerPlugIns { get; }

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