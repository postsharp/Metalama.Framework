// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Caravela.Framework.Impl.Aspects
{
    [Obfuscation( Exclude = true /* JSON */ )]
    internal class InheritedAspectsManifest
    {
        public IReadOnlyDictionary<string, IReadOnlyList<string>> InheritedAspects { get; }

        public InheritedAspectsManifest( IReadOnlyDictionary<string, IReadOnlyList<string>> inheritedAspects )
        {
            this.InheritedAspects = inheritedAspects;
        }

        public static InheritedAspectsManifest Create( IEnumerable<IAspectInstance> inheritedAspect )
            => new(
                inheritedAspect.GroupBy( a => a.AspectClass )
                    .ToDictionary(
                        g => g.Key.FullName,
                        g => (IReadOnlyList<string>) g.Select(
                                i => DocumentationCommentId.CreateReferenceId( i.TargetDeclaration.GetSymbol().AssertNotNull( "TODO" ) ) )
                            .ToList(),
                        StringComparer.Ordinal ) );

        private void Serialize( Stream stream )
        {
            var deflate = new DeflateStream( stream, CompressionLevel.Optimal );
            var manifestJson = JsonConvert.SerializeObject( this, Newtonsoft.Json.Formatting.Indented );
            using var manifestWriter = new StreamWriter( deflate, Encoding.UTF8 );
            manifestWriter.Write( manifestJson );
        }

        public ResourceDescription ToResource()
            => new(
                CompileTimeConstants.InheritedAspectManifestResourceName,
                () =>
                {
                    var stream = new MemoryStream();
                    this.Serialize( stream );
                    _ = stream.Seek( 0, SeekOrigin.Begin );

                    return stream;
                },
                true );

        public static InheritedAspectsManifest Deserialize( Stream stream )
        {
            using var deflate = new DeflateStream( stream, CompressionMode.Decompress );
            using var manifestReader = new StreamReader( deflate, Encoding.UTF8 );
            var manifestJson = manifestReader.ReadToEnd();
            stream.Close();

            return JsonConvert.DeserializeObject<InheritedAspectsManifest>( manifestJson ).AssertNotNull();
        }
    }
}