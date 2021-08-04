// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Advices
{
    internal static class TemplateKindHelper
    {
        public static bool IsAsyncTask( this TemplateKind selectionKind )
            => selectionKind switch
            {
                TemplateKind.Async => true,
                _ => false
            };

        public static bool IsAsync( this TemplateKind selectionKind )
            => selectionKind switch
            {
                TemplateKind.Async => true,
                TemplateKind.IAsyncEnumerable => true,
                TemplateKind.IAsyncEnumerator => true,
                _ => false
            };

        public static bool IsAsyncIterator( this TemplateKind selectionKind )
            => selectionKind switch
            {
                TemplateKind.IAsyncEnumerable => true,
                TemplateKind.IAsyncEnumerator => true,
                _ => false
            };

        public static bool IsIterator( this TemplateKind selectionKind )
            => selectionKind switch
            {
                TemplateKind.IEnumerable => true,
                TemplateKind.IEnumerator => true,
                TemplateKind.IAsyncEnumerable => true,
                TemplateKind.IAsyncEnumerator => true,
                _ => false
            };

        public static bool MustInterpretAsAsync( this in Template<IMethod> template )
            => template.Declaration is { IsAsync: true }
               || (template.SelectedKind == TemplateKind.Default && template.InterpretedKind.IsAsync());

        public static bool MustInterpretAsAsyncIterator( this in Template<IMethod> template )
            => template.InterpretedKind.IsAsyncIterator() && (template.Declaration!.IsAsync || template.SelectedKind == TemplateKind.Default);
    }
}