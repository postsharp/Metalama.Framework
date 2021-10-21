// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Caravela.Framework.Impl.Aspects
{
    [Obfuscation( Exclude = true /* JSON */ )]
    internal class InheritableAspectsManifest : IInheritableAspectsManifest
    {
        public static InheritableAspectsManifest Empty { get; } =
            new( ImmutableDictionary<string, IReadOnlyList<string>>.Empty );

        public IReadOnlyDictionary<string, IReadOnlyList<string>> InheritableAspects { get; }

        public InheritableAspectsManifest( IReadOnlyDictionary<string, IReadOnlyList<string>> inheritableAspects )
        {
            this.InheritableAspects = inheritableAspects;
        }

        public static InheritableAspectsManifest Create( IEnumerable<IAspectInstance> inheritedAspect )
            => new(
                inheritedAspect.GroupBy( a => a.AspectClass )
                    .ToDictionary(
                        g => g.Key.FullName,
                        g => (IReadOnlyList<string>) g.Select(
                                i => DocumentationCommentId.CreateDeclarationId( i.TargetDeclaration.GetSymbol().AssertNotNull( "TODO" ) ) )
                            .ToList(),
                        StringComparer.Ordinal ) );

        private void Serialize( Stream stream )
        {
            using var deflate = new DeflateStream( stream, CompressionLevel.Optimal, true );
            var manifestJson = JsonConvert.SerializeObject( this, Newtonsoft.Json.Formatting.Indented );
            using var manifestWriter = new StreamWriter( deflate, Encoding.UTF8, 8196, true );
            manifestWriter.Write( manifestJson );
            manifestWriter.Flush();
            deflate.Flush();
            stream.Flush();
        }

        public ManagedResource ToResource()
        {
            var stream = new MemoryStream();
            this.Serialize( stream );
            var bytes = stream.ToArray();

            return new ManagedResource(
                CompileTimeConstants.InheritableAspectManifestResourceName,
                bytes,
                true );
        }

        public static InheritableAspectsManifest Deserialize( Stream stream )
        {
            using var deflate = new DeflateStream( stream, CompressionMode.Decompress );
            using var manifestReader = new StreamReader( deflate, Encoding.UTF8 );
            var manifestJson = manifestReader.ReadToEnd();
            stream.Close();

            return JsonConvert.DeserializeObject<InheritableAspectsManifest>( manifestJson ).AssertNotNull();
        }

        public IEnumerable<string> InheritableAspectTypes => this.InheritableAspects.Keys;

        public IEnumerable<string> GetInheritableAspectTargets( string aspectType ) => this.InheritableAspects[aspectType];
    }
}