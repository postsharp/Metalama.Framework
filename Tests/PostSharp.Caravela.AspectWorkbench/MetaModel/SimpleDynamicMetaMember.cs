using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.AspectWorkbench
{
    internal class SimpleDynamicMetaMember : IDynamicMetaMember
    {
        private readonly Func<ExpressionSyntax> _func;

        public SimpleDynamicMetaMember(Func<ExpressionSyntax> func)
        {
            this._func = func;
        }

        public ExpressionSyntax CreateExpression() => this._func();
    }
}