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
        public static IPropertyOrIndexer OverrideProperty(
            IServiceProvider serviceProvider,
            Advice advice,
            IFieldOrPropertyOrIndexer targetDeclaration,
            BoundTemplateMethod getTemplate,
            BoundTemplateMethod setTemplate,
            IObjectReader tags,
            Action<ITransformation> addTransformation )
        {
            if ( targetDeclaration is IField field )
            {
                var promotedField = new PromotedField( serviceProvider, advice, field, tags );
                addTransformation( promotedField );
                addTransformation( new OverridePropertyTransformation( advice, promotedField, getTemplate, setTemplate, tags ) );

                return promotedField;
            }
            else if ( targetDeclaration is IProperty property )
            {
                addTransformation( new OverridePropertyTransformation( advice, property, getTemplate, setTemplate, tags ) );

                return property;
            }
            else
            {
                throw new AssertionFailedException();
            }
        }
    }
}