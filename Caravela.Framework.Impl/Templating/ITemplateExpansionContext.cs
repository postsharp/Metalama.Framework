using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    internal interface ITemplateExpansionContext
    {
        ICodeElement TargetDeclaration { get; }

        object TemplateInstance { get; }

        IProceedImpl ProceedImplementation { get; }

        ICompilation Compilation { get; }

        ITemplateExpansionLexicalScope CurrentLexicalScope { get; }

        StatementSyntax CreateReturnStatement( ExpressionSyntax? returnExpression );
    }
}
