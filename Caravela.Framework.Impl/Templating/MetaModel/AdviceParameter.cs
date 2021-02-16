using System;
using System.Collections.Generic;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class AdviceParameter : IAdviceParameter
    {
        private readonly IParameter _parameter;

        public AdviceParameter( IParameter p )
        {
            this._parameter = p;
        }

        public RefKind RefKind => this._parameter.RefKind;

        public OptionalValue DefaultValue => this._parameter.DefaultValue;

        public bool IsParams => this._parameter.IsParams;

        public IType ParameterType => this._parameter.ParameterType;

        public string Name => this._parameter.Name.AssertNotNull();

        public int Index => this._parameter.Index;

        public ICodeElement? ContainingElement => this._parameter.ContainingElement;

        public IReadOnlyList<IAttribute> Attributes => this._parameter.Attributes;

        public CodeElementKind ElementKind => this._parameter.ElementKind;

        public ICompilation Compilation => throw new NotImplementedException();


        public dynamic Value
        {
            get => new DynamicMember( SyntaxFactory.IdentifierName( this._parameter.Name! ), this._parameter.ParameterType, true );
            set => throw new NotImplementedException();
        }



        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this._parameter.ToDisplayString( format, context );

        public bool Equals( ICodeElement other ) => throw new NotImplementedException();
    }
}