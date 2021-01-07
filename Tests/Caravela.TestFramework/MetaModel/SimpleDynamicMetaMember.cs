using System;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.TestFramework.MetaModel
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