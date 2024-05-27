// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
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
        Advice advice,
        IFieldOrPropertyOrIndexer targetDeclaration,
        BoundTemplateMethod? getTemplate,
        BoundTemplateMethod? setTemplate,
        IObjectReader tags,
        Action<ITransformation> addTransformation )
    {
        if ( targetDeclaration is IField field )
        {
            var propertyBuilder = new PromotedField( serviceProvider, field, tags, advice );
            addTransformation( propertyBuilder.ToTransformation() );
            addTransformation( new OverridePropertyTransformation( advice, propertyBuilder, getTemplate, setTemplate, tags ) );

            AddTransformationsForStructField( targetDeclaration.DeclaringType, advice, addTransformation );

            return propertyBuilder;
        }
        else if ( targetDeclaration is IProperty property )
        {
            addTransformation( new OverridePropertyTransformation( advice, property, getTemplate, setTemplate, tags ) );

            if ( property.IsAutoPropertyOrField.GetValueOrDefault() )
            {
                AddTransformationsForStructField( targetDeclaration.DeclaringType, advice, addTransformation );
            }

            return property;
        }
        else
        {
            throw new AssertionFailedException( $"Unexpected declaration: '{targetDeclaration}'." );
        }
    }

    public static void AddTransformationsForStructField( INamedType type, Advice advice, Action<ITransformation> addTransformation )
    {
        if ( type.TypeKind is TypeKind.Struct or TypeKind.RecordStruct )
        {
            // If there is no 'this()' constructor, add one.
            if ( type.Constructors.FirstOrDefault() is { IsImplicitlyDeclared: true } implicitConstructor )
            {
                var constructorBuilder = new ConstructorBuilder( advice, type ) 
                { 
                    ReplacedImplicit = implicitConstructor.ToTypedRef(),
                    Accessibility = Accessibility.Public
                };

                addTransformation( constructorBuilder.ToTransformation() );
            }
        }
    }
}