// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Microsoft.CodeAnalysis;
using System.Linq;
using RefKind = Microsoft.CodeAnalysis.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class MethodReturnParameter : ReturnParameter
    {
        public Method DeclaringMethod { get; }

        public override IMember DeclaringMember => this.DeclaringMethod;

        public MethodReturnParameter( Method declaringMethod )
        {
            this.DeclaringMethod = declaringMethod;
        }

        protected override RefKind SymbolRefKind => this.DeclaringMethod.MethodSymbol.RefKind;

        public override IType ParameterType => this.DeclaringMethod.ReturnType;

        public override bool Equals( ICodeElement other )
            => other is MethodReturnParameter methodReturnParameter &&
               SymbolEqualityComparer.Default.Equals( this.DeclaringMethod.Symbol, methodReturnParameter.DeclaringMethod.Symbol );

        public override ISymbol? Symbol => null;

        public override CodeElementLink<ICodeElement> ToLink() => CodeElementLink.ReturnParameter( this.DeclaringMethod.MethodSymbol );

        [Memo]
        public override IAttributeList Attributes
            => new AttributeList(
                this.DeclaringMethod.MethodSymbol.GetReturnTypeAttributes()
                    .Select( a => new AttributeLink( a, this.ToLink() ) ),
                (CompilationModel) this.Compilation );
    }
}