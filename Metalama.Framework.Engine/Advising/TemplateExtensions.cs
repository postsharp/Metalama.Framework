// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Engine.Advising
{
    internal static class TemplateExtensions
    {
        public static (TemplateMember<IMethod>? Get, TemplateMember<IMethod>? Set) GetAccessorTemplates( this TemplateMember<IProperty>? propertyTemplate )
        {
            if ( propertyTemplate != null )
            {
                if ( !propertyTemplate.Declaration.IsAutoPropertyOrField )
                {
                    TemplateMember<IMethod>? GetAccessorTemplate( IMethod? accessor )
                    {
                        if ( accessor != null && propertyTemplate.TemplateClassMember.Accessors.TryGetValue(
                                accessor.GetSymbol()!.MethodKind,
                                out var template ) )
                        {
                            return TemplateMemberFactory.Create( accessor, template );
                        }
                        else
                        {
                            return null;
                        }
                    }

                    return (GetAccessorTemplate( propertyTemplate.Declaration.GetMethod ),
                            GetAccessorTemplate( propertyTemplate.Declaration.SetMethod ));
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
            => template.Declaration is { IsAsync: true }
               || (template.SelectedKind == TemplateKind.Default && template.InterpretedKind.IsAsyncTemplate());

        public static bool MustInterpretAsAsyncIteratorTemplate( this TemplateMember<IMethod> template )
            => template.InterpretedKind.IsAsyncIteratorTemplate() && (template.Declaration.IsAsync || template.SelectedKind == TemplateKind.Default);

        public static TemplateMember<IField>? GetInitializerTemplate( this TemplateMember<IField>? fieldTemplate )
        {
            // TODO 30576 - do not rely on syntax for templates.

            if ( fieldTemplate != null )
            {
                var templateName = TemplateNameHelper.GetCompiledTemplateName( fieldTemplate.Declaration.AssertNotNull().GetSymbol().AssertNotNull() );

                if ( fieldTemplate.TemplateClassMember.TemplateClass.Type.GetMethod( templateName ) != null )
                {
                    return TemplateMemberFactory.Create(
                        fieldTemplate.Declaration,
                        fieldTemplate.TemplateClassMember,
                        fieldTemplate.AdviceAttribute.AssertNotNull(),
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
                var templateName = TemplateNameHelper.GetCompiledTemplateName( eventFieldTemplate.Declaration.AssertNotNull().GetSymbol().AssertNotNull() );

                if ( eventFieldTemplate.TemplateClassMember.TemplateClass.Type.GetMethod( templateName ) != null )
                {
                    return TemplateMemberFactory.Create(
                        eventFieldTemplate.Declaration,
                        eventFieldTemplate.TemplateClassMember,
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
                var templateName = TemplateNameHelper.GetCompiledTemplateName( propertyTemplate.Declaration.AssertNotNull().GetSymbol().AssertNotNull() );

                if ( propertyTemplate.TemplateClassMember.TemplateClass.Type.GetMethod( templateName ) != null )
                {
                    return TemplateMemberFactory.Create(
                        propertyTemplate.Declaration,
                        propertyTemplate.TemplateClassMember,
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