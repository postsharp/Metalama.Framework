// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

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
    }
}