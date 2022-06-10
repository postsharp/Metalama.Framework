// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Advices
{
    internal static class TemplateExtensions
    {
        public static (TemplateMember<IMethod> Get, TemplateMember<IMethod> Set) GetAccessorTemplates( this TemplateMember<IProperty> propertyTemplate )
        {
            if ( propertyTemplate.IsNotNull )
            {
                if ( !propertyTemplate.Declaration!.IsAutoPropertyOrField )
                {
                    TemplateMember<IMethod> GetAccessorTemplate( IMethod? accessor )
                    {
                        if ( accessor != null && propertyTemplate.TemplateClassMember.Accessors.TryGetValue(
                                accessor.GetSymbol()!.MethodKind,
                                out var template ) )
                        {
                            return TemplateMember.Create( accessor, template );
                        }
                        else
                        {
                            return default;
                        }
                    }

                    return (GetAccessorTemplate( propertyTemplate.Declaration!.GetMethod ),
                            GetAccessorTemplate( propertyTemplate.Declaration!.SetMethod ));
                }
            }

            return (default, default);
        }

        private static bool IsAsyncTemplate( this TemplateKind selectionKind )
            => selectionKind switch
            {
                TemplateKind.Async => true,
                TemplateKind.IAsyncEnumerable => true,
                TemplateKind.IAsyncEnumerator => true,
                _ => false
            };

        private static bool IsAsyncIteratorTemplate( this TemplateKind selectionKind )
            => selectionKind switch
            {
                TemplateKind.IAsyncEnumerable => true,
                TemplateKind.IAsyncEnumerator => true,
                _ => false
            };

        public static bool MustInterpretAsAsyncTemplate( this in TemplateMember<IMethod> template )
            => template.Declaration is { IsAsync: true }
               || (template.SelectedKind == TemplateKind.Default && template.InterpretedKind.IsAsyncTemplate());

        public static bool MustInterpretAsAsyncIteratorTemplate( this in TemplateMember<IMethod> template )
            => template.InterpretedKind.IsAsyncIteratorTemplate() && (template.Declaration!.IsAsync || template.SelectedKind == TemplateKind.Default);

        public static TemplateMember<IField> GetInitializerTemplate( this in TemplateMember<IField> fieldTemplate )
        {
            // TODO 30576 - do not rely on syntax for templates.

            if ( fieldTemplate.IsNotNull )
            {
                var fieldSyntax = (VariableDeclaratorSyntax?) fieldTemplate.Declaration!.GetPrimaryDeclarationSyntax();

                if ( fieldSyntax?.Initializer != null )
                {
                    return TemplateMember.Create(
                        fieldTemplate.Declaration,
                        fieldTemplate.TemplateClassMember,
                        fieldTemplate.TemplateAttribute.AssertNotNull(),
                        TemplateKind.InitializerExpression );
                }
                else
                {
                    return default;
                }
            }
            else
            {
                return default;
            }
        }

        public static TemplateMember<IEvent> GetInitializerTemplate( this in TemplateMember<IEvent> eventFieldTemplate )
        {
            // TODO 30576 - do not rely on syntax for templates.

            if ( eventFieldTemplate.IsNotNull
                 && eventFieldTemplate.Declaration!.GetPrimaryDeclarationSyntax() is VariableDeclaratorSyntax eventFieldSyntax )
            {
                if ( eventFieldSyntax.Initializer != null )
                {
                    return TemplateMember.Create( eventFieldTemplate.Declaration, eventFieldTemplate.TemplateClassMember, TemplateKind.InitializerExpression );
                }
                else
                {
                    return default;
                }
            }
            else
            {
                return default;
            }
        }

        public static TemplateMember<IProperty> GetInitializerTemplate( this in TemplateMember<IProperty> propertyTemplate )
        {
            // TODO 30576 - do not rely on syntax for templates.

            if ( propertyTemplate.IsNotNull )
            {
                var propertySyntax = (PropertyDeclarationSyntax?) propertyTemplate.Declaration!.GetPrimaryDeclarationSyntax();

                if ( propertySyntax?.Initializer != null )
                {
                    return TemplateMember.Create( propertyTemplate.Declaration, propertyTemplate.TemplateClassMember, TemplateKind.InitializerExpression );
                }
                else
                {
                    return default;
                }
            }
            else
            {
                return default;
            }
        }
    }
}