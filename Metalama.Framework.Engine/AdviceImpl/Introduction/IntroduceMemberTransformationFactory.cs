// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.AdviceImpl.Attributes;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.Transformations;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal static class IntroduceMemberTransformationFactory
{
    public static IInjectMemberTransformation CreateTransformation( this PropertyBuilder propertyBuilder, TemplateMember<IProperty>? template = null )
    {
        Invariant.Assert( propertyBuilder.OriginalField == null );

        return new IntroducePropertyTransformation( propertyBuilder.AspectLayerInstance, propertyBuilder.Immutable, template );
    }

    public static ITransformation CreateTransformation( this AttributeBuilder attributeBuilder )
        => new IntroduceAttributeTransformation( attributeBuilder.AspectLayerInstance, attributeBuilder.Immutable );

    public static IInjectMemberTransformation CreateTransformation( this ConstructorBuilder constructorBuilder )
        => constructorBuilder.IsStatic
            ? new IntroduceStaticConstructorTransformation( constructorBuilder.AspectLayerInstance, constructorBuilder.Immutable )
            : new IntroduceConstructorTransformation( constructorBuilder.AspectLayerInstance, constructorBuilder.Immutable );

    public static IInjectMemberTransformation CreateTransformation( this EventBuilder eventBuilder, TemplateMember<IEvent>? template = null )
        => new IntroduceEventTransformation( eventBuilder.AspectLayerInstance, eventBuilder.Immutable, template );

    public static IInjectMemberTransformation CreateTransformation( this FieldBuilder fieldBuilder, TemplateMember<IField>? template = null )
        => new IntroduceFieldTransformation( fieldBuilder.AspectLayerInstance, fieldBuilder.Immutable, template );

    public static IInjectMemberTransformation CreateTransformation( this IndexerBuilder indexerBuilder )
        => new IntroduceIndexerTransformation( indexerBuilder.AspectLayerInstance, indexerBuilder.Immutable );

    public static IntroduceNamedTypeTransformation CreateTransformation( this NamedTypeBuilder namedTypeBuilder )
        => new( namedTypeBuilder.AspectLayerInstance, namedTypeBuilder.Immutable );

    public static IntroduceNamespaceTransformation CreateTransformation( this NamespaceBuilder namespaceBuilder )
        => new( namespaceBuilder.AspectLayerInstance, namespaceBuilder.Immutable );
}