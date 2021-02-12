using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class TemplateContextImpl : ITemplateContext
    {
        public IMethod Method { get; }

        [Memo]
        public IAdviceParameterList Parameters => new AdviceParameterList( this.Method );

        public IType Type { get; }

        public ICompilation Compilation { get; }

        // TODO: when possible, this vanishes (e.g. `target.This.Property` is compiled to just `Property`); fix it so that it produces `this` or the name of the type, depending on whether the member on the right is static
        public dynamic This => new ThisDynamicMetaMember( !this.Method.IsStatic );

        public TemplateContextImpl( IMethod method, IType type, ICompilation compilation )
        {
            this.Method = method;
            this.Type = type;
            this.Compilation = compilation;
        }

        private class ThisDynamicMetaMember : IDynamicMetaMemberDifferentiated
        {
            private readonly bool _allowExpression;

            public ThisDynamicMetaMember( bool allowExpression ) => this._allowExpression = allowExpression;

            public ExpressionSyntax CreateExpression()
            {
                if ( this._allowExpression )
                {
                    return ThisExpression();
                }

                // TODO: diagnostic
                throw new InvalidOperationException( "Can't directly access 'this' on a static method." );
            }

            public ExpressionSyntax CreateMemberAccessExpression( string member ) => IdentifierName( Identifier( member ) );
        }
    }

    internal class AdviceParameterList : IAdviceParameterList
    {
        private readonly AdviceParameter[] _parameters;

        public AdviceParameterList( IMethod method ) => this._parameters = method.Parameters.Select( p => new AdviceParameter( p ) ).ToArray();

        public IAdviceParameter this[int index] => this._parameters[index];

        public int Count => this._parameters.Length;

        public IEnumerator<IAdviceParameter> GetEnumerator() => ((IEnumerable<IAdviceParameter>) this._parameters).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    internal class AdviceParameter : IAdviceParameter
    {
        private readonly IParameter _parameter;

        public AdviceParameter( IParameter p )
        {
            this._parameter = p;
        }

        public RefKind RefKind => this._parameter.RefKind;

        public IType Type => this._parameter.Type;

        public string? Name => this._parameter.Name;

        public int Index => this._parameter.Index;

        public OptionalValue DefaultValue => throw new NotImplementedException();

        public ICodeElement? ContainingElement => this._parameter.ContainingElement;

        public IReadOnlyList<IAttribute> Attributes => this._parameter.Attributes;

        public CodeElementKind ElementKind => this._parameter.ElementKind;

        public dynamic Value
        {
            get => new DynamicMetaMember( IdentifierName( this._parameter.Name! ) );
            set => throw new NotImplementedException();
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this._parameter.ToDisplayString( format, context );

        public bool Equals( ICodeElement other ) => throw new NotImplementedException();
    }

    internal class DynamicMetaMember : IDynamicMetaMember
    {
        private readonly ExpressionSyntax _expression;

        public DynamicMetaMember( ExpressionSyntax expression ) => this._expression = expression;

        public ExpressionSyntax CreateExpression() => this._expression;
    }
}
