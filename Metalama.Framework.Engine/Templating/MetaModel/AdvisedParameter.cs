// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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

        public TypedConstant DefaultValue => this.Underlying.DefaultValue;

        public bool IsParams => this.Underlying.IsParams;

        public IMember DeclaringMember => this.Underlying.DeclaringMember;

        public ParameterInfo ToParameterInfo() => this.Underlying.ToParameterInfo();

        public bool IsReturnParameter => this.Underlying.IsReturnParameter;

        public IType ParameterType => this.Underlying.Type;

        public string Name => this.Underlying.Name.AssertNotNull();

        public int Index => this.Underlying.Index;

        DeclarationOrigin IDeclaration.Origin => this.Underlying.Origin;

        public IType Type => this.Underlying.Type;

        public bool IsAssignable => true;

        public object? Value
        {
            get => this.ToExpression();
            set => throw new NotSupportedException();
        }

        private UserExpression ToExpression()
            => new(
                SyntaxFactory.IdentifierName( this.Underlying.Name ),
                this.Underlying.Type,
                isReferenceable: true,
                isAssignable: true );

        public ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext ) => SyntaxFactory.IdentifierName( this.Underlying.Name );

        public RunTimeTemplateExpression ToRunTimeTemplateExpression( SyntaxGenerationContext syntaxGenerationContext )
            => new(
                this.ToSyntax( syntaxGenerationContext ),
                this.Type,
                syntaxGenerationContext );
    }
}