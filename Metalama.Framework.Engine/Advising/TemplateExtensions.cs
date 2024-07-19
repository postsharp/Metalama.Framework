// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;

namespace Metalama.Framework.Engine.Advising
{
    internal static class TemplateExtensions
    {
        public static (TemplateMember<IMethod>? Get, TemplateMember<IMethod>? Set) GetAccessorTemplates( this TemplateMember<IProperty>? propertyTemplate )
        {
            if ( propertyTemplate != null )
            {
                if ( !propertyTemplate.Declaration.IsAutoPropertyOrField.GetValueOrDefault() )
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

        public static (TemplateMember<IMethod>? Add, TemplateMember<IMethod>? Remove) GetAccessorTemplates( this TemplateMember<IEvent>? eventTemplate )
        {
            if ( eventTemplate != null )
            {
                if ( !eventTemplate.Declaration.IsEventField().GetValueOrDefault() )
                {
                    TemplateMember<IMethod>? GetAccessorTemplate( IMethod? accessor )
                    {
                        if ( accessor != null && eventTemplate.TemplateClassMember.Accessors.TryGetValue(
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

                    return (GetAccessorTemplate( eventTemplate.Declaration.AddMethod ),
                            GetAccessorTemplate( eventTemplate.Declaration.RemoveMethod ));
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
                var templateName = TemplateNameHelper.GetCompiledTemplateName( fieldTemplate.Declaration.AssertNotNull().GetSymbol().AssertSymbolNotNull() );

                if ( fieldTemplate.TemplateClassMember.TemplateClass.Type.GetAnyMethod( templateName ) != null )
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
                var templateName =
                    TemplateNameHelper.GetCompiledTemplateName( eventFieldTemplate.Declaration.AssertNotNull().GetSymbol().AssertSymbolNotNull() );

                if ( eventFieldTemplate.TemplateClassMember.TemplateClass.Type.GetAnyMethod( templateName ) != null )
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
                var templateName = TemplateNameHelper.GetCompiledTemplateName( propertyTemplate.Declaration.AssertNotNull().GetSymbol().AssertSymbolNotNull() );

                if ( propertyTemplate.TemplateClassMember.TemplateClass.Type.GetAnyMethod( templateName ) != null )
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