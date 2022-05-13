// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Advised;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal class AdvisedParameter : AdvisedDeclaration<IParameterImpl>, IAdvisedParameter
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

        private IExpression ToExpression()
            => new UserExpression(
                SyntaxFactory.IdentifierName( this.Underlying.Name ),
                this.Underlying.Type,
                TemplateExpansionContext.CurrentSyntaxGenerationContext,
                isReferenceable: true,
                isAssignable: true );
    }
}