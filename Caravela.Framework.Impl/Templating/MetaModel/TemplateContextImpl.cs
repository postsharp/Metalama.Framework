using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Reactive;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    class TemplateContextImpl : ITemplateContext
    {
        public IMethod Method { get; }

        [Memo]
        public IAdviceParameterList Parameters => new AdviceParameterList( this.Method );

        public IType Type { get; }

        public ICompilation Compilation { get; }

        public TemplateContextImpl( IMethod method, IType type, ICompilation compilation )
        {
            this.Method = method;
            this.Type = type;
            this.Compilation = compilation;
        }
    }

    class AdviceParameterList : IAdviceParameterList
    {
        private readonly AdviceParameter[] _parameters;

        public AdviceParameterList( IMethod method ) => this._parameters = method.Parameters.Select( p => new AdviceParameter( p ) ).ToArray();

        public IAdviceParameter this[int index] => this._parameters[index];

        public int Count => this._parameters.Length;

        public IEnumerator<IAdviceParameter> GetEnumerator() => ((IEnumerable<IAdviceParameter>) this._parameters).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    class AdviceParameter : IAdviceParameter
    {
        private readonly IParameter _parameter;

        public AdviceParameter( IParameter p ) => this._parameter = p;

        public IType Type => this._parameter.Type;

        public string? Name => this._parameter.Name;

        public int Index => this._parameter.Index;

        public ICodeElement? ContainingElement => this._parameter.ContainingElement;

        public IReactiveCollection<IAttribute> Attributes => this._parameter.Attributes;

        public CodeElementKind Kind => this._parameter.Kind;

        public dynamic Value
        {
            get => new DynamicMetaMember( SyntaxFactory.IdentifierName( this._parameter.Name! ) );
            set => throw new NotImplementedException();
        }
    }

    class DynamicMetaMember : IDynamicMetaMember
    {
        private readonly ExpressionSyntax _expression;

        public DynamicMetaMember( ExpressionSyntax expression ) => this._expression = expression;

        public ExpressionSyntax CreateExpression() => this._expression;
    }
}
