// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;

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
            if ( fieldTemplate.IsNotNull )
            {
                // Initializer template is compiled into a template for the field.
                var templateName = TemplateNameHelper.GetCompiledTemplateName( fieldTemplate.Declaration.AssertNotNull().GetSymbol().AssertNotNull() );

                if ( fieldTemplate.TemplateClassMember.TemplateClass.AspectType.GetMethod( templateName ) != null )
                {
                    return TemplateMember.Create( fieldTemplate.Declaration, fieldTemplate.TemplateClassMember, TemplateKind.InitializerExpression );
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
            if ( eventFieldTemplate.IsNotNull )
            {
                // Initializer template is compiled into a template for event.
                var templateName = TemplateNameHelper.GetCompiledTemplateName( eventFieldTemplate.Declaration.AssertNotNull().GetSymbol().AssertNotNull() );

                if ( eventFieldTemplate.TemplateClassMember.TemplateClass.AspectType.GetMethod( templateName ) != null )
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
            if ( propertyTemplate.IsNotNull )
            {
                // Initializer template is compiled into a template for property.
                var templateName = TemplateNameHelper.GetCompiledTemplateName( propertyTemplate.Declaration.AssertNotNull().GetSymbol().AssertNotNull() );

                if ( propertyTemplate.TemplateClassMember.TemplateClass.AspectType.GetMethod( templateName ) != null )
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