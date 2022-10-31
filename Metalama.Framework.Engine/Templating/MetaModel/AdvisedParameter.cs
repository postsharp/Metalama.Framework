// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal class AdvisedParameter : AdvisedDeclaration<IParameterImpl>, IAdvisedParameter, IUserExpression
    {
        public AdvisedParameter( IParameter p ) : base( (IParameterImpl) p ) { }

        public RefKind RefKind => this.Underlying.RefKind;

        public TypedConstant? DefaultValue => this.Underlying.DefaultValue;

        public bool IsParams => this.Underlying.IsParams;

        public IHasParameters DeclaringMember => this.Underlying.DeclaringMember;

        public ParameterInfo ToParameterInfo() => this.Underlying.ToParameterInfo();

        public bool IsReturnParameter => this.Underlying.IsReturnParameter;

        public IType ParameterType => this.Underlying.Type;

        public string Name => this.Underlying.Name.AssertNotNull();

        public int Index => this.Underlying.Index;

        public IType Type => this.Underlying.Type;

        public bool IsAssignable => true;

        public object? Value
        {
            get => this.ToExpression();
            set => throw new NotSupportedException();
        }

        private BuiltUserExpression ToExpression()
            => new(
                SyntaxFactory.IdentifierName( this.Underlying.Name ),
                this.Underlying.Type,
                isReferenceable: true,
                isAssignable: true );

        public ExpressionSyntax ToExpressionSyntax( SyntaxGenerationContext syntaxGenerationContext ) => SyntaxFactory.IdentifierName( this.Underlying.Name );

        public TypedExpressionSyntax ToTypedExpressionSyntax( SyntaxGenerationContext syntaxGenerationContext )
            => new(
                this.ToExpressionSyntax( syntaxGenerationContext ),
                this.Type,
                syntaxGenerationContext );
    }
}