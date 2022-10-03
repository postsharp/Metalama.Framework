// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Advising
{
    internal static class OverrideHelper
    {
        public static IProperty OverrideProperty(
            IServiceProvider serviceProvider,
            Advice advice,
            IFieldOrPropertyOrIndexer targetDeclaration,
            BoundTemplateMethod? getTemplate,
            BoundTemplateMethod? setTemplate,
            IObjectReader tags,
            Action<ITransformation> addTransformation )
        {
            if ( targetDeclaration is IField field )
            {
                var promotedField = new PromotedField( serviceProvider, advice, field, tags );
                addTransformation( promotedField );
                addTransformation( new OverridePropertyTransformation( advice, promotedField, getTemplate, setTemplate, tags ) );

                AddTransformationsForStructField( targetDeclaration.DeclaringType, advice, addTransformation );

                return promotedField;
            }
            else if ( targetDeclaration is IProperty property )
            {
                addTransformation( new OverridePropertyTransformation( advice, property, getTemplate, setTemplate, tags ) );

                if ( property.IsAutoPropertyOrField )
                {
                    AddTransformationsForStructField( targetDeclaration.DeclaringType, advice, addTransformation );
                }

                return property;
            }
            else
            {
                throw new AssertionFailedException();
            }
        }

        public static void AddTransformationsForStructField( INamedType type, Advice advice, Action<ITransformation> addTransformation )
        {
            if ( type.TypeKind is TypeKind.Struct or TypeKind.RecordStruct )
            {
                // If there is no 'this()' constructor, add one.
                if ( !type.Constructors.Any( c => !c.IsImplicitlyDeclared ) )
                {
                    addTransformation( new AddExplicitDefaultConstructorTransformation( advice, type ) );
                }
            }
        }
    }
}