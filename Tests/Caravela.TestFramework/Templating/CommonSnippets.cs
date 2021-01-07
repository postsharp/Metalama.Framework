namespace Caravela.TestFramework.Templating
{
    public static class CommonSnippets
    {
        public const string CaravelaUsings = @"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.Framework.Aspects;
using Caravela.TestFramework.MetaModel;
using Caravela.Framework.Impl.Templating.MetaModel;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Caravela.Framework.Impl.Templating.TemplateHelper;
";
    }
}
