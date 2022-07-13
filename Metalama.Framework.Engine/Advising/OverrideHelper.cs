// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advising
{
    internal static class OverrideHelper
    {
        public static IProperty OverrideProperty(
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
                
                AddTransformationsForStructConstructors(targetDeclaration.DeclaringType, advice, addTransformation);

                return promotedField;
            }
            else if ( targetDeclaration is IProperty property )
            {
                addTransformation( new OverridePropertyTransformation( advice, property, getTemplate, setTemplate, tags ) );

                if ( property.IsAutoPropertyOrField )
                {
                    AddTransformationsForStructConstructors(targetDeclaration.DeclaringType, advice, addTransformation);
                }

                return property;
            }
            else
            {
                throw new AssertionFailedException();
            }
        }
        
        public static void AddTransformationsForStructConstructors( INamedType type, Advice advice, Action<ITransformation> addTransformation )
        {
            if (type.TypeKind == TypeKind.Struct)
            {
                // If we are in a struct, make sure that all existing constructors call `this()`.

                foreach (var constructor in type.Constructors)
                {
                    if (!constructor.IsImplicit && constructor.InitializerKind == ConstructorInitializerKind.None)
                    {
                        addTransformation( new CallDefaultConstructorTransformation( advice, constructor ) );
                    }
                }
            }
        }
    }
}