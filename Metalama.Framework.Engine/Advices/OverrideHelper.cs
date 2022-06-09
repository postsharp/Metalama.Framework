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
            IServiceProvider serviceProvider,
            Advice advice,
            IFieldOrPropertyOrIndexer targetDeclaration,
            BoundTemplateMethod getTemplate,
            BoundTemplateMethod setTemplate,
            IObjectReader tags )
        {
            if ( targetDeclaration is IField field )
            {
                var promotedField = new PromotedField( serviceProvider, advice, field, tags );

                return new ITransformation[] { promotedField, new OverridePropertyTransformation( advice, promotedField, getTemplate, setTemplate, tags ) };
            }
            else if ( targetDeclaration is IProperty property )
            {
                return new ITransformation[] { new OverridePropertyTransformation( advice, property, getTemplate, setTemplate, tags ) };
            }
            else
            {
                throw new AssertionFailedException();
            }
        }
    }
}