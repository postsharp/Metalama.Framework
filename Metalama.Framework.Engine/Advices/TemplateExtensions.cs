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
        public static (TemplateMember<IMethod> Get, TemplateMember<IMethod> Set) GetAccessorTemplates( this in TemplateMember<IProperty> propertyTemplate )
        {
            if ( propertyTemplate.IsNotNull )
            {
                if ( !propertyTemplate.Declaration!.IsAutoPropertyOrField )
                {
                    return (TemplateMember.Create( propertyTemplate.Declaration!.GetMethod, propertyTemplate.TemplateInfo ),
                            TemplateMember.Create( propertyTemplate.Declaration!.SetMethod, propertyTemplate.TemplateInfo ));
                }
            }

            return (default, default);
        }

        public static TemplateMember<IField> GetInitializerTemplate( this in TemplateMember<IField> fieldTemplate )
        {
            if ( fieldTemplate.IsNotNull )
            {
                var fieldSyntax = (VariableDeclaratorSyntax) fieldTemplate.Declaration!.GetPrimaryDeclaration().AssertNotNull();

                if ( fieldSyntax.Initializer != null )
                {
                    return TemplateMember.Create( fieldTemplate.Declaration, fieldTemplate.TemplateInfo, TemplateKind.InitializerExpression );
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
            if ( eventFieldTemplate.IsNotNull
                 && eventFieldTemplate.Declaration!.GetPrimaryDeclaration().AssertNotNull() is VariableDeclaratorSyntax eventFieldSyntax )
            {
                if ( eventFieldSyntax.Initializer != null )
                {
                    return TemplateMember.Create( eventFieldTemplate.Declaration, eventFieldTemplate.TemplateInfo, TemplateKind.InitializerExpression );
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
            if ( propertyTemplate.IsNotNull )
            {
                var propertySyntax = (PropertyDeclarationSyntax) propertyTemplate.Declaration!.GetPrimaryDeclaration().AssertNotNull();

                if ( propertySyntax.Initializer != null )
                {
                    return TemplateMember.Create( propertyTemplate.Declaration, propertyTemplate.TemplateInfo, TemplateKind.InitializerExpression );
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

        private static bool IsAsync( this TemplateKind selectionKind )
            => selectionKind switch
            {
                TemplateKind.Async => true,
                TemplateKind.IAsyncEnumerable => true,
                TemplateKind.IAsyncEnumerator => true,
                _ => false
            };

        private static bool IsAsyncIterator( this TemplateKind selectionKind )
            => selectionKind switch
            {
                TemplateKind.IAsyncEnumerable => true,
                TemplateKind.IAsyncEnumerator => true,
                _ => false
            };

        public static bool MustInterpretAsAsync( this in TemplateMember<IMethod> template )
            => template.Declaration is { IsAsync: true }
               || (template.SelectedKind == TemplateKind.Default && template.InterpretedKind.IsAsync());

        public static bool MustInterpretAsAsyncIterator( this in TemplateMember<IMethod> template )
            => template.InterpretedKind.IsAsyncIterator() && (template.Declaration!.IsAsync || template.SelectedKind == TemplateKind.Default);
    }
}