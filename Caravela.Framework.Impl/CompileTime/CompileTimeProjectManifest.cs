// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// A serializable object that stores the manifest of a <see cref="CompileTimeProject"/>. 
    /// </summary>
    internal class CompileTimeProjectManifest
    {
        public CompileTimeProjectManifest(
            string assemblyName,
            IReadOnlyList<string> aspectTypes,
            IReadOnlyList<string>? references,
            ulong sourceHash,
            IReadOnlyList<CompileTimeFile> files )
        {
            this.AssemblyName = assemblyName;
            this.AspectTypes = aspectTypes;
            this.References = references;
            this.SourceHash = sourceHash;
            this.Files = files;
        }

        public string AssemblyName { get; }

        /// <summary>
        /// Gets the list of all aspect types (specified by fully qualified name) of the aspect library.
        /// </summary>
        public IReadOnlyList<string> AspectTypes { get; }

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

        public static bool TryDeserialize( Stream stream, [NotNullWhen( true )] out CompileTimeProjectManifest? manifest )
        {
            try
            {
                using var manifestReader = new StreamReader( stream, Encoding.UTF8 );
                var manifestJson = manifestReader.ReadToEnd();
                stream.Close();

                manifest = JsonConvert.DeserializeObject<CompileTimeProjectManifest>( manifestJson ).AssertNotNull();

                // Assert that files are properly deserialized.
                foreach ( var file in manifest.Files )
                {
                    if ( file.SourcePath == null! || file.TransformedPath == null! )
                    {
                        throw new AssertionFailedException( "Deserialization error." );
                    }
                }

                return true;
            }
            catch ( JsonReaderException )
            {
                manifest = null;

                return false;
            }
        }

        public void Serialize( Stream stream )
        {
            var manifestJson = JsonConvert.SerializeObject( this, Newtonsoft.Json.Formatting.Indented );
            using var manifestWriter = new StreamWriter( stream, Encoding.UTF8 );
            manifestWriter.Write( manifestJson );
        }
    }
}