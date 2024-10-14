// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Advising
{
    internal static class TemplateExtensions
    {
        public static (TemplateMember<IMethod>? Get, TemplateMember<IMethod>? Set) GetAccessorTemplates( this TemplateMember<IProperty>? propertyTemplate )
        {
            if ( propertyTemplate != null )
            {
                var templatePropertySymbol = (IPropertySymbol) propertyTemplate.DeclarationRef.Symbol;

                if ( !templatePropertySymbol.IsAutoProperty().GetValueOrDefault() )
                {
                    TemplateMember<IMethod>? GetAccessorTemplate( IMethodSymbol? accessor )
                    {
                        if ( accessor != null && propertyTemplate.TemplateClassMember.Accessors.TryGetValue(
                                accessor.MethodKind,
                                out var template ) )
                        {
                            return TemplateMemberFactory.Create<IMethod>(
                                accessor,
                                template,
                                propertyTemplate.TemplateProvider,
                                propertyTemplate.DeclarationRef.RefFactory,
                                propertyTemplate.Tags );
                        }
                        else
                        {
                            return null;
                        }
                    }

                    return (GetAccessorTemplate( templatePropertySymbol.GetMethod ),
                            GetAccessorTemplate( templatePropertySymbol.SetMethod ));
                }
            }

            return (default, default);
        }

        public static (TemplateMember<IMethod>? Add, TemplateMember<IMethod>? Remove) GetAccessorTemplates( this TemplateMember<IEvent>? eventTemplate )
        {
            if ( eventTemplate != null )
            {
                var templateEventSymbol = (IEventSymbol) eventTemplate.DeclarationRef.Symbol;

                if ( !templateEventSymbol.IsEventField().GetValueOrDefault() )
                {
                    TemplateMember<IMethod>? GetAccessorTemplate( IMethodSymbol? accessor )
                    {
                        if ( accessor != null && eventTemplate.TemplateClassMember.Accessors.TryGetValue(
                                accessor.MethodKind,
                                out var template ) )
                        {
                            return TemplateMemberFactory.Create<IMethod>(
                                accessor,
                                template,
                                eventTemplate.TemplateProvider,
                                eventTemplate.DeclarationRef.RefFactory,
                                eventTemplate.Tags );
                        }
                        else
                        {
                            return null;
                        }
                    }

                    return (GetAccessorTemplate( templateEventSymbol.AddMethod ),
                            GetAccessorTemplate( templateEventSymbol.RemoveMethod ));
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

        public static bool MustInterpretAsAsyncTemplate( this TemplateMember<IMethod> template )
            => ((IMethodSymbol) template.DeclarationRef.Symbol).IsAsyncSafe()
               || (template.SelectedTemplateKind == TemplateKind.Default && template.InterpretedTemplateKind.IsAsyncTemplate());

        public static bool MustInterpretAsAsyncIteratorTemplate( this TemplateMember<IMethod> template )
            => template.InterpretedTemplateKind.IsAsyncIteratorTemplate()
               && (((IMethodSymbol) template.DeclarationRef.Symbol).IsAsyncSafe() || template.SelectedTemplateKind == TemplateKind.Default);

        public static TemplateMember<IField>? GetInitializerTemplate( this TemplateMember<IField>? fieldTemplate )
        {
            // TODO 30576 - do not rely on syntax for templates.

            if ( fieldTemplate != null )
            {
                var templateName = TemplateNameHelper.GetCompiledTemplateName( fieldTemplate.DeclarationRef.Symbol );

                if ( fieldTemplate.TemplateClassMember.TemplateClass.Type.GetAnyMethod( templateName ) != null )
                {
                    return TemplateMemberFactory.Create<IField>(
                        fieldTemplate.DeclarationRef.Symbol,
                        fieldTemplate.TemplateClassMember,
                        fieldTemplate.TemplateProvider,
                        fieldTemplate.AdviceAttribute.AssertNotNull(),
                        fieldTemplate.DeclarationRef.RefFactory,
                        fieldTemplate.Tags,
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

        public static TemplateMember<IEvent>? GetInitializerTemplate( this TemplateMember<IEvent>? eventFieldTemplate )
        {
            // TODO 30576 - do not rely on syntax for templates.

            if ( eventFieldTemplate != null )
            {
                // Initializer template is compiled into a template for event.
                var templateName =
                    TemplateNameHelper.GetCompiledTemplateName( eventFieldTemplate.DeclarationRef.Symbol );

                if ( eventFieldTemplate.TemplateClassMember.TemplateClass.Type.GetAnyMethod( templateName ) != null )
                {
                    return TemplateMemberFactory.Create<IEvent>(
                        eventFieldTemplate.DeclarationRef.Symbol,
                        eventFieldTemplate.TemplateClassMember,
                        eventFieldTemplate.TemplateProvider,
                        eventFieldTemplate.DeclarationRef.RefFactory,
                        eventFieldTemplate.Tags,
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

        public static TemplateMember<IProperty>? GetInitializerTemplate( this TemplateMember<IProperty>? propertyTemplate )
        {
            if ( propertyTemplate != null )
            {
                // Initializer template is compiled into a template for property.
                var templateName = TemplateNameHelper.GetCompiledTemplateName( propertyTemplate.DeclarationRef.Symbol );

                if ( propertyTemplate.TemplateClassMember.TemplateClass.Type.GetAnyMethod( templateName ) != null )
                {
                    return TemplateMemberFactory.Create<IProperty>(
                        propertyTemplate.DeclarationRef.Symbol,
                        propertyTemplate.TemplateClassMember,
                        propertyTemplate.TemplateProvider,
                        propertyTemplate.DeclarationRef.RefFactory,
                        propertyTemplate.Tags,
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
    }
}