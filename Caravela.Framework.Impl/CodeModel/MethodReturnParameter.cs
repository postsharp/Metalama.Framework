using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Microsoft.CodeAnalysis;

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

        protected override Microsoft.CodeAnalysis.RefKind SymbolRefKind => this.DeclaringMethod.MethodSymbol.RefKind;

        public override IType ParameterType => this.DeclaringMethod.ReturnType;

        public override bool Equals( ICodeElement other ) => other is MethodReturnParameter methodReturnParameter &&
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