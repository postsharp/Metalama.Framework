// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Advices
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