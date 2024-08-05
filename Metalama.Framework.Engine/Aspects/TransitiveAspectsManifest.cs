// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.CompileTime.Serialization;
using Metalama.Framework.Engine.HierarchicalOptions;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Options;
using Metalama.Framework.Serialization;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Metalama.Framework.Engine.Aspects;

public sealed class TransitiveAspectsManifest : ITransitiveAspectsManifest
{
    public ImmutableDictionary<string, IReadOnlyList<InheritableAspectInstance>> InheritableAspects { get; private set; }

    public ImmutableArray<TransitiveValidatorInstance> ReferenceValidators { get; private set; }

    // To levels of mapping of options: first option types, then target declaration.
    public ImmutableDictionary<HierarchicalOptionsKey, IHierarchicalOptions> InheritableOptions { get; private set; }

    public ImmutableDictionaryOfArray<SerializableDeclarationId, IAnnotation> Annotations { get; private set; }

    // Deserializer constructor.
    private TransitiveAspectsManifest()
    {
        this.InheritableAspects = null!;
        this.InheritableOptions = null!;
        this.Annotations = null!;
    }

    private TransitiveAspectsManifest(
        ImmutableDictionary<string, IReadOnlyList<InheritableAspectInstance>> inheritableAspects,
        ImmutableArray<TransitiveValidatorInstance> validators,
        ImmutableDictionary<HierarchicalOptionsKey, IHierarchicalOptions> options,
        ImmutableDictionaryOfArray<SerializableDeclarationId, IAnnotation> annotations )
    {
        this.InheritableAspects = inheritableAspects;
        this.ReferenceValidators = validators;
        this.InheritableOptions = options;
        this.Annotations = annotations;
    }

    public static TransitiveAspectsManifest Create(
        ImmutableArray<InheritableAspectInstance> inheritedAspect,
        ImmutableArray<TransitiveValidatorInstance> validators,
        ImmutableDictionary<HierarchicalOptionsKey, IHierarchicalOptions> options,
        ImmutableDictionaryOfArray<SerializableDeclarationId, IAnnotation> annotations )
        => new(
            inheritedAspect.GroupBy( a => a.AspectClass )
                .ToImmutableDictionary(
                    g => g.Key.FullName,
                    g => g.Select( i => new InheritableAspectInstance( i ) )
                        .ToReadOnlyList(),
                    StringComparer.Ordinal ),
            validators,
            options,
            annotations );

    private void Serialize( Stream stream, in ProjectServiceProvider serviceProvider, CompilationContext compilationContext )
    {
        using var deflate = new DeflateStream( stream, CompressionLevel.Optimal, true );
        var formatter = CompileTimeSerializer.CreateInstance( serviceProvider, compilationContext );
        formatter.Serialize( this, deflate );
        deflate.Flush();
        stream.Flush();
    }

    public byte[] ToBytes( in ProjectServiceProvider serviceProvider, CompilationContext compilationContext )
    {
        var stream = new MemoryStream();
        this.Serialize( stream, serviceProvider, compilationContext );

        return stream.ToArray();
    }

    internal ManagedResource ToResource( in ProjectServiceProvider serviceProvider, CompilationContext compilationContext )
    {
        var bytes = this.ToBytes( serviceProvider, compilationContext );

        return new ManagedResource(
            CompileTimeConstants.InheritableAspectManifestResourceName,
            bytes,
            true );
    }

    public static TransitiveAspectsManifest Deserialize(
        Stream stream,
        in ProjectServiceProvider serviceProvider,
        CompilationContext compilationContext,
        string? assemblyName )
    {
        using var deflate = new DeflateStream( stream, CompressionMode.Decompress );

        var formatter = CompileTimeSerializer.CreateInstance( serviceProvider, compilationContext );

        return (TransitiveAspectsManifest) formatter.Deserialize( deflate, assemblyName ).AssertNotNull();
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
            initializationArguments.SetValue( nameof(instance.ReferenceValidators), instance.ReferenceValidators );
            initializationArguments.SetValue( nameof(instance.InheritableOptions), instance.InheritableOptions );
            initializationArguments.SetValue( nameof(instance.Annotations), instance.Annotations.ToImmutableDictionary() );
        }

        public override void DeserializeFields( object obj, IArgumentsReader initializationArguments )
        {
            var instance = (TransitiveAspectsManifest) obj;

            instance.InheritableAspects =
                initializationArguments.GetValue<ImmutableDictionary<string, IReadOnlyList<InheritableAspectInstance>>>( nameof(instance.InheritableAspects) )!;

            instance.ReferenceValidators =
                initializationArguments.GetValue<ImmutableArray<TransitiveValidatorInstance>>( nameof(instance.ReferenceValidators) );

            instance.InheritableOptions =
                initializationArguments.GetValue<ImmutableDictionary<HierarchicalOptionsKey, IHierarchicalOptions>>( nameof(instance.InheritableOptions) )!;

            instance.Annotations =
                new ImmutableDictionaryOfArray<SerializableDeclarationId, IAnnotation>(
                    initializationArguments.GetValue<ImmutableDictionary<SerializableDeclarationId, ImmutableArray<IAnnotation>>>(
                        nameof(instance.Annotations) )! );
        }
    }
}