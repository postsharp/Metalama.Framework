// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal sealed class AdvisedParameter : AdvisedDeclaration<IParameterImpl>, IAdvisedParameter, IUserExpression
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

        public ref object? Value => ref RefHelper.Wrap( this.ToExpression() );

        private BuiltUserExpression ToExpression()
            => new(
                SyntaxFactory.IdentifierName( this.Underlying.Name ),
                this.Underlying.Type,
                isReferenceable: true,
                isAssignable: true );

        private ExpressionSyntax ToExpressionSyntax() => SyntaxFactory.IdentifierName( this.Underlying.Name );

        public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
            => new TypedExpressionSyntaxImpl(
                this.ToExpressionSyntax(),
                this.Type,
                syntaxGenerationContext );
    }
}