namespace Caravela.TestFramework.Templating
{
    public static class CommonSnippets
    {
        // TODO: Remove the namespaces that are required only in the compiled template. They should be added automatically during template compilation.
        public const string CaravelaUsings = @"
using Caravela.Framework.Aspects;
using static Caravela.Framework.Aspects.TemplateContext;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.Framework.Impl.Templating.MetaModel;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Caravela.Framework.Impl.Templating.TemplateSyntaxFactory;
";
    }
}
