using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// This class is used at *run-time* by the generated template code. Do not remove or refactor
    /// without analysing impact on generated code.
    /// </summary>
    public static class TemplateHelper
    {
        private static readonly SyntaxAnnotation flattenBlockAnnotation = new SyntaxAnnotation("flatten");

        public static BlockSyntax WithFlattenBlockAnnotation(this BlockSyntax block) =>
            block.WithAdditionalAnnotations(flattenBlockAnnotation);

        public static bool HasFlattenBlockAnnotation(this BlockSyntax block) =>
            block.HasAnnotation(flattenBlockAnnotation);

        // ReSharper disable once UnusedMember.Global
        public static SeparatedSyntaxList<T> SeparatedList<T>(params T[] items) where T : SyntaxNode
            => SyntaxFactory.SeparatedList(items);


    }
}