// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.LamaSerialization;
using Metalama.Framework.Serialization;
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

namespace Metalama.Framework.Engine.Aspects
{
    [Obfuscation( Exclude = true /* Serialization */ )]
    internal class TransitiveAspectsManifest : ITransitiveAspectsManifest
    {
        public static TransitiveAspectsManifest Empty { get; } =
            new( ImmutableDictionary<string, IReadOnlyList<InheritableAspectInstance>>.Empty );

        public ImmutableDictionary<string, IReadOnlyList<InheritableAspectInstance>> InheritableAspects { get; private set; }

        // Deserializer constructor.
        private TransitiveAspectsManifest()
        {
            this.InheritableAspects = null!;
        }

        public TransitiveAspectsManifest( ImmutableDictionary<string, IReadOnlyList<InheritableAspectInstance>> inheritableAspects )
        {
            this.InheritableAspects = inheritableAspects;
        }

        public static TransitiveAspectsManifest Create( ImmutableArray<InheritableAspectInstance> inheritedAspect, Compilation compilation )
            => new(
                inheritedAspect.GroupBy( a => a.AspectClass )
                    .ToImmutableDictionary(
                        g => g.Key.FullName,
                        g => (IReadOnlyList<InheritableAspectInstance>) g.Select(
                                i => new InheritableAspectInstance( i ) )
                            .ToList(),
                        StringComparer.Ordinal ) );

        private void Serialize( Stream stream, IServiceProvider serviceProvider )
        {
            using var deflate = new DeflateStream( stream, CompressionLevel.Optimal, true );
            var formatter = LamaFormatter.CreateSerializingInstance(serviceProvider);
            formatter.Serialize( this, deflate );
            deflate.Flush();
            stream.Flush();
        }

        public ManagedResource ToResource(IServiceProvider serviceProvider)
        {
            var stream = new MemoryStream();
            this.Serialize( stream, serviceProvider );
            var bytes = stream.ToArray();

            return new ManagedResource(
                CompileTimeConstants.InheritableAspectManifestResourceName,
                bytes,
                true );
        }

        public static TransitiveAspectsManifest Deserialize( Stream stream, IServiceProvider serviceProvider )
        {
            using var deflate = new DeflateStream( stream, CompressionMode.Decompress );

            var formatter = LamaFormatter.CreateDeserializingInstance( serviceProvider );
            
            return (TransitiveAspectsManifest) formatter.Deserialize( deflate ).AssertNotNull(  );
        }

        public IEnumerable<string> InheritableAspectTypes => this.InheritableAspects.Keys;

        public IEnumerable<InheritableAspectInstance> GetInheritedAspects( string aspectType ) => this.InheritableAspects[aspectType];

        private class Serializer : ReferenceTypeSerializer
        {
            public override object CreateInstance( Type type, IArgumentsReader constructorArguments ) => new TransitiveAspectsManifest();

            public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
            {
                var instance = (TransitiveAspectsManifest) obj;
                initializationArguments.SetValue( nameof(instance.InheritableAspects), instance.InheritableAspects );
            }

            public override void DeserializeFields( object obj, IArgumentsReader initializationArguments )
            {
                var instance = (TransitiveAspectsManifest) obj;
                instance.InheritableAspects = initializationArguments.GetValue<ImmutableDictionary<string, IReadOnlyList<InheritableAspectInstance>>>( nameof(instance.InheritableAspects) )!;
            }
        }
    }
}