using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Reactive;
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
        public dynamic This => new ThisDynamicMetaMember( !this.Method.IsStatic, this.Type );

        public TemplateContextImpl( IMethod method, IType type, ICompilation compilation )
        {
            this.Method = method;
            this.Type = type;
            this.Compilation = compilation;
        }

        private class ThisDynamicMetaMember : IDynamicMetaMemberDifferentiated
        {
            private readonly bool _allowExpression;
            private readonly IType _type;

            public ThisDynamicMetaMember( bool allowExpression, IType type )
            {
                this._allowExpression = allowExpression;
                this._type = type;
            }

            public RuntimeExpression CreateExpression()
            {
                if ( this._allowExpression )
                {
                    return new( ThisExpression(), this._type );
                }

                // TODO: diagnostic
                throw new InvalidOperationException( "Can't directly access 'this' on a static method." );
            }

            public RuntimeExpression CreateMemberAccessExpression( string member ) => new( IdentifierName( Identifier( member ) ) );
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

        public AdviceParameter( IParameter p ) => this._parameter = p;

        public RefKind RefKind => this._parameter.RefKind;

        public bool IsByRef => this._parameter.IsByRef;

        public bool IsRef => this._parameter.IsRef;

        public bool IsOut => this._parameter.IsOut;

        public bool IsParams => this._parameter.IsParams;

        public IType Type => this._parameter.Type;

        public string? Name => this._parameter.Name;

        public int Index => this._parameter.Index;

        public ICodeElement? ContainingElement => this._parameter.ContainingElement;

        public IReactiveCollection<IAttribute> Attributes => this._parameter.Attributes;

        public CodeElementKind ElementKind => this._parameter.ElementKind;

        public bool HasDefaultValue => this._parameter.HasDefaultValue;

        public object? DefaultValue => this._parameter.DefaultValue;

        public dynamic Value
        {
            get => new DynamicMetaMember( IdentifierName( this._parameter.Name! ), this._parameter.Type );
            set => throw new NotImplementedException();
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this._parameter.ToDisplayString( format, context );
    }

    internal class DynamicMetaMember : IDynamicMetaMember
    {
        private readonly ExpressionSyntax _expression;
        private readonly IType _type;

        public DynamicMetaMember( ExpressionSyntax expression, IType type )
        {
            this._expression = expression;
            this._type = type;
        }

        public RuntimeExpression CreateExpression() => new( this._expression, this._type );
    }
}
