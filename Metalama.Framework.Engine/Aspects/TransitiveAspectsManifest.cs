// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.CompileTime.Serialization;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Metalama.Framework.Engine.Aspects
{
    public sealed class TransitiveAspectsManifest : ITransitiveAspectsManifest
    {
        public ImmutableDictionary<string, IReadOnlyList<InheritableAspectInstance>> InheritableAspects { get; private set; }

        public ImmutableArray<TransitiveValidatorInstance> Validators { get; private set; }

        // Deserializer constructor.
        private TransitiveAspectsManifest()
        {
            this.InheritableAspects = null!;
        }

        private TransitiveAspectsManifest(
            ImmutableDictionary<string, IReadOnlyList<InheritableAspectInstance>> inheritableAspects,
            ImmutableArray<TransitiveValidatorInstance> validators )
        {
            this.InheritableAspects = inheritableAspects;
            this.Validators = validators;
        }

        public static TransitiveAspectsManifest Create(
            ImmutableArray<InheritableAspectInstance> inheritedAspect,
            ImmutableArray<TransitiveValidatorInstance> validators )
            => new(
                inheritedAspect.GroupBy( a => a.AspectClass )
                    .ToImmutableDictionary(
                        g => g.Key.FullName,
                        g => (IReadOnlyList<InheritableAspectInstance>) g.Select( i => new InheritableAspectInstance( i ) )
                            .ToList(),
                        StringComparer.Ordinal ),
                validators );

        private void Serialize( Stream stream, ProjectServiceProvider serviceProvider )
        {
            using var deflate = new DeflateStream( stream, CompressionLevel.Optimal, true );
            var formatter = CompileTimeSerializer.CreateSerializingInstance( serviceProvider );
            formatter.Serialize( this, deflate );
            deflate.Flush();
            stream.Flush();
        }

        public byte[] ToBytes( ProjectServiceProvider serviceProvider )
        {
            var stream = new MemoryStream();
            this.Serialize( stream, serviceProvider );

            return stream.ToArray();
        }

        internal ManagedResource ToResource( ProjectServiceProvider serviceProvider )
        {
            var bytes = this.ToBytes( serviceProvider );

            return new ManagedResource(
                CompileTimeConstants.InheritableAspectManifestResourceName,
                bytes,
                true );
        }

        public static TransitiveAspectsManifest Deserialize( Stream stream, ProjectServiceProvider serviceProvider )
        {
            using var deflate = new DeflateStream( stream, CompressionMode.Decompress );

            var formatter = CompileTimeSerializer.CreateDeserializingInstance( serviceProvider );

            return (TransitiveAspectsManifest) formatter.Deserialize( deflate ).AssertNotNull();
        }

        public IEnumerable<string> InheritableAspectTypes => this.InheritableAspects.Keys;

        public IEnumerable<InheritableAspectInstance> GetInheritableAspects( string aspectType ) => this.InheritableAspects[aspectType];

        // ReSharper disable once UnusedType.Local
        private class Serializer : ReferenceTypeSerializer
        {
            public override object CreateInstance( Type type, IArgumentsReader constructorArguments ) => new TransitiveAspectsManifest();

            public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
            {
                var instance = (TransitiveAspectsManifest) obj;
                initializationArguments.SetValue( nameof(instance.InheritableAspects), instance.InheritableAspects );
                initializationArguments.SetValue( nameof(instance.Validators), instance.Validators );
            }

            public override void DeserializeFields( object obj, IArgumentsReader initializationArguments )
            {
                var instance = (TransitiveAspectsManifest) obj;

                instance.InheritableAspects =
                    initializationArguments.GetValue<ImmutableDictionary<string, IReadOnlyList<InheritableAspectInstance>>>(
                        nameof(instance.InheritableAspects) )!;

                instance.Validators =
                    initializationArguments.GetValue<ImmutableArray<TransitiveValidatorInstance>>( nameof(instance.Validators) );
            }
        }
    }
}