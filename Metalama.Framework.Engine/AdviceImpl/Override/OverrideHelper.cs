﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Linq;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.AdviceImpl.Override;

internal static class OverrideHelper
{
    public static IProperty OverrideProperty(
        ProjectServiceProvider serviceProvider,
        AspectLayerInstance aspectLayerInstance,
        IFieldOrPropertyOrIndexer targetDeclaration,
        BoundTemplateMethod? getTemplate,
        BoundTemplateMethod? setTemplate,
        IObjectReader tags,
        Action<ITransformation> addTransformation )
    {
        switch ( targetDeclaration )
        {
            case IField { OverridingProperty: { } overridingProperty }:
                return OverrideProperty( serviceProvider, aspectLayerInstance, overridingProperty, getTemplate, setTemplate, tags, addTransformation );

            case IField field:
                {
                    var propertyBuilder = PromotedFieldBuilder.Create( serviceProvider, field, tags, aspectLayerInstance );
                    propertyBuilder.Freeze();

                    addTransformation( propertyBuilder.ToTransformation() );
                    addTransformation( new OverridePropertyTransformation( aspectLayerInstance, propertyBuilder.ToRef(), getTemplate, setTemplate, tags ) );

                    AddTransformationsForStructField( targetDeclaration.DeclaringType, aspectLayerInstance, addTransformation );

                    return propertyBuilder;
                }

            case IProperty property:
                {
                    addTransformation( new OverridePropertyTransformation( aspectLayerInstance, property.ToFullRef(), getTemplate, setTemplate, tags ) );

                    if ( property.IsAutoPropertyOrField.GetValueOrDefault() )
                    {
                        AddTransformationsForStructField( targetDeclaration.DeclaringType, aspectLayerInstance, addTransformation );
                    }

                    return property;
                }

            default:
                throw new AssertionFailedException( $"Unexpected declaration: '{targetDeclaration}'." );
        }
    }

    public static void AddTransformationsForStructField( INamedType type, AspectLayerInstance aspectLayerInstance, Action<ITransformation> addTransformation )
    {
        if ( type.TypeKind is TypeKind.Struct or TypeKind.RecordStruct )
        {
            // If there is no 'this()' constructor, add one.
            if ( type.Constructors.FirstOrDefault() is { IsImplicitlyDeclared: true } implicitConstructor )
            {
                var constructorBuilder = new ConstructorBuilder( aspectLayerInstance, type )
                {
                    ReplacedImplicitConstructor = implicitConstructor, Accessibility = Accessibility.Public
                };

                addTransformation( constructorBuilder.ToTransformation() );
            }
        }
    }
}