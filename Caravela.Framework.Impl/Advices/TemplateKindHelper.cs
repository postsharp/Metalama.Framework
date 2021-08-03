using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.Advices
{
    internal static class TemplateKindHelper
    {
        public static bool IsAsyncTask( this TemplateSelectionKind selectionKind )
            => selectionKind switch
            {
                TemplateSelectionKind.Async => true,
                _ => false
            };

        public static bool IsAsync( this TemplateSelectionKind selectionKind )
            => selectionKind switch
            {
                TemplateSelectionKind.Async => true,
                TemplateSelectionKind.IAsyncEnumerable => true,
                TemplateSelectionKind.IAsyncEnumerator => true,
                _ => false
            };

        public static bool IsIterator( this TemplateSelectionKind selectionKind )
            => selectionKind switch
            {
                TemplateSelectionKind.IEnumerable => true,
                TemplateSelectionKind.IEnumerator => true,
                TemplateSelectionKind.IAsyncEnumerable => true,
                TemplateSelectionKind.IAsyncEnumerator => true,
                _ => false
            };

        public static bool MustInterpretAsAsync( this in Template<IMethod> template )
            => template.Declaration!.IsAsync
               || (template.SelectedKind == TemplateSelectionKind.Default && template.InterpretedKind.IsAsync());
    }
}