// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advices
{
    internal static class OverrideHelper
    {
        public static ITransformation[] OverrideProperty(
            Advice advice,
            IFieldOrPropertyOrIndexer targetDeclaration, 
            TemplateMember<IMethod> getTemplate,
            TemplateMember<IMethod> setTemplate,
            Func<TemplateMember<IMethod>, IMethod?, BoundTemplateMethod> templateBinder,          
            IObjectReader tags )
        {
            if ( targetDeclaration is IField field )
            {
                var promotedField = new PromotedField( advice, field, tags );

                if ( field.Writeability == Writeability.ConstructorOnly && setTemplate.IsNotNull )
                {
                    // Privately writeable property is a transformation that adds a private setter to a get-only property.
                    var writeableProperty = new PrivatelyWriteableProperty( advice, promotedField, tags );

                    var boundGetTemplate = templateBinder( getTemplate, writeableProperty.GetMethod );
                    var boundSetTemplate = templateBinder( setTemplate, writeableProperty.SetMethod );

                    return new ITransformation[] 
                    {
                        promotedField,
                        writeableProperty,
                        new OverridePropertyTransformation( advice, writeableProperty, boundGetTemplate, boundSetTemplate, tags ),
                    };
                }
                else
                {
                    var boundGetTemplate = templateBinder( getTemplate, promotedField.GetMethod );
                    var boundSetTemplate = templateBinder( setTemplate, promotedField.SetMethod );

                    return new ITransformation[]
                    {
                        promotedField,
                        new OverridePropertyTransformation( advice, promotedField, boundGetTemplate, boundSetTemplate, tags ),
                    };
                }
            }
            else if ( targetDeclaration is IProperty property )
            {
                if ( property.Writeability == Writeability.ConstructorOnly && setTemplate.IsNotNull )
                {
                    var writeableProperty = new PrivatelyWriteableProperty( advice, property, tags );

                    var boundGetTemplate = templateBinder( getTemplate, writeableProperty.GetMethod );
                    var boundSetTemplate = templateBinder( setTemplate, writeableProperty.SetMethod );

                    return new ITransformation[]
                    {
                        writeableProperty,
                        new OverridePropertyTransformation( advice, writeableProperty, boundGetTemplate, boundSetTemplate, tags ),
                    };
                }
                else
                {
                    var boundGetTemplate = templateBinder( getTemplate, targetDeclaration.GetMethod );
                    var boundSetTemplate = templateBinder( setTemplate, targetDeclaration.SetMethod );

                    return new ITransformation[]
                    {
                        new OverridePropertyTransformation( advice, property, boundGetTemplate, boundSetTemplate, tags ),
                    };
                }
            }
            else
            {
                throw new AssertionFailedException();
            }
        }
    }
}