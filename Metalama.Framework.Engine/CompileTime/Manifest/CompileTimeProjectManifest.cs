﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text;

namespace Metalama.Framework.Engine.CompileTime.Manifest
{
    /// <summary>
    /// A serializable object that stores the manifest of a <see cref="CompileTimeProject"/>. 
    /// </summary>
    [JsonObject( ItemNullValueHandling = NullValueHandling.Ignore )]
    internal sealed class CompileTimeProjectManifest
    {
        public CompileTimeProjectManifest(
            string runTimeAssemblyIdentity,
            string compileTimeAssemblyName,
            string targetFramework,
            IReadOnlyList<string> aspectTypes,
            IReadOnlyList<string> plugInTypes,
            IReadOnlyList<string> fabricTypes,
            IReadOnlyList<string> transitiveFabricTypes,
            IReadOnlyList<string> otherTemplateTypes,
            IReadOnlyList<string>? references,
            TemplateProjectManifest? templates,
            string? redistributionLicenseKey,
            ulong sourceHash,
            IReadOnlyList<CompileTimeFile> files,
            string? metalamaVersion = null,
            int manifestVersion = 0 )
        {
            this.RunTimeAssemblyIdentity = runTimeAssemblyIdentity;
            this.CompileTimeAssemblyName = compileTimeAssemblyName;
            this.TargetFramework = targetFramework;
            this.AspectTypes = aspectTypes;
            this.PlugInTypes = plugInTypes;
            this.FabricTypes = fabricTypes;
            this.TransitiveFabricTypes = transitiveFabricTypes;
            this.OtherTemplateTypes = otherTemplateTypes;
            this.References = references;
            this.RedistributionLicenseKey = redistributionLicenseKey;
            this.SourceHash = sourceHash;
            this.Files = files;
            this.Templates = templates;
            this.MetalamaVersion = AssemblyMetadataReader.GetInstance( typeof(CompileTimeProjectManifest).Assembly ).PackageVersion.AssertNotNull();
            this.ManifestVersion = manifestVersion == 0 ? CurrentManifestVersion : manifestVersion;

#if DEBUG

            // Validate that we got a valid target framework.
            if ( !string.IsNullOrEmpty( targetFramework ) )
            {
                _ = new FrameworkName( targetFramework );
            }
#endif
        }

        public string RunTimeAssemblyIdentity { get; }

        public string CompileTimeAssemblyName { get; }

        public string TargetFramework { get; }

        /// <summary>
        /// Gets the version of Metalama that created the compile-time project.
        /// </summary>
        public string MetalamaVersion { get; }

        public int ManifestVersion { get; }

        public const int CurrentManifestVersion = 1;

        /// <summary>
        /// Gets the list of all aspect types (specified by fully qualified name) of the aspect library.
        /// </summary>
        public IReadOnlyList<string> AspectTypes { get; }

        /// <summary>
        /// Gets the list of all template types (specified by fully qualified name) that are neither aspects nor fabrics in the aspect library.
        /// </summary>
        public IReadOnlyList<string> OtherTemplateTypes { get; }

        /// <summary>
        /// Gets the list of types that are exported using the <c>CompilerPlugin</c> attribute.
        /// </summary>
        public IReadOnlyList<string> PlugInTypes { get; }

        /// <summary>
        /// Gets the list of types that implement the <see cref="Metalama.Framework.Fabrics.Fabric"/> interface, but the <see cref="Metalama.Framework.Fabrics.TransitiveProjectFabric"/>.
        /// </summary>
        public IReadOnlyList<string> FabricTypes { get; }

        /// <summary>
        /// Gets the list of types that implement the <see cref="Metalama.Framework.Fabrics.TransitiveProjectFabric"/> interface.
        /// </summary>
        public IReadOnlyList<string> TransitiveFabricTypes { get; }

        public TemplateProjectManifest? Templates { get; }

        /// <summary>
        /// Gets the name of all project references (a fully-qualified assembly identity) of the compile-time project.
        /// </summary>
        public IReadOnlyList<string>? References { get; }

        public string? RedistributionLicenseKey { get; }

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

            var manifest = FromJson( manifestJson );

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

        public static CompileTimeProjectManifest FromJson( string json ) => JsonConvert.DeserializeObject<CompileTimeProjectManifest>( json ).AssertNotNull();

        public void Serialize( Stream stream )
        {
            var manifestJson = this.ToJson();
            using var manifestWriter = new StreamWriter( stream, Encoding.UTF8 );
            manifestWriter.Write( manifestJson );
        }

        public string ToJson() => JsonConvert.SerializeObject( this, Newtonsoft.Json.Formatting.Indented );
    }
}