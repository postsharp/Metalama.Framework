namespace Caravela.TestFramework
{
    internal static class CommonSnippets
    {
        // TODO: The required namespaces should be added automatically during template compilation.
        public const string CaravelaUsings = @"
using System.Collections.Generic;

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
