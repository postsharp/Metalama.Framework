// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Advised;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class AdviceParameter : AdviceDeclaration<IParameter>, IAdviceParameter
    {
        public AdviceParameter( IParameter p ) : base( p ) { }

        public RefKind RefKind => this.Underlying.RefKind;

        public TypedConstant DefaultValue => this.Underlying.DefaultValue;

        public bool IsParams => this.Underlying.IsParams;

        public IMemberOrNamedType DeclaringMember => this.Underlying.DeclaringMember;

        public ParameterInfo ToParameterInfo() => this.Underlying.ToParameterInfo();

        public IType ParameterType => this.Underlying.ParameterType;

        public string Name => this.Underlying.Name.AssertNotNull();

        public int Index => this.Underlying.Index;

        DeclarationOrigin IDeclaration.Origin => this.Underlying.Origin;

        public dynamic Value
        {
            get => new DynamicExpression( SyntaxFactory.IdentifierName( this.Underlying.Name! ), this.Underlying.ParameterType, true );
            set => throw new NotImplementedException();
        }
    }
}