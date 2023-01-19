// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using RefKind = Microsoft.CodeAnalysis.RefKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class MethodReturnParameter : ReturnParameter
    {
        public Method DeclaringMethod { get; }

        public override IHasParameters DeclaringMember => this.DeclaringMethod;

        public MethodReturnParameter( Method declaringMethod )
        {
            this.DeclaringMethod = declaringMethod;
        }

        protected override RefKind SymbolRefKind => this.DeclaringMethod.MethodSymbol.RefKind;

        public override IType Type => this.DeclaringMethod.ReturnType;

        public override bool Equals( IDeclaration? other )
            => other is MethodReturnParameter methodReturnParameter &&
               SymbolEqualityComparer.Default.Equals( this.DeclaringMethod.Symbol, methodReturnParameter.DeclaringMethod.Symbol );

        public override bool IsImplicitlyDeclared => this.DeclaringMethod.IsImplicitlyDeclared;

        public override ISymbol? Symbol => null;

        internal override Ref<IDeclaration> ToRef() => Ref.ReturnParameter( this.DeclaringMethod.MethodSymbol, this.GetCompilationModel().RoslynCompilation );

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default )
            => this.DeclaringMember.GetContainedDeclarations().Select( d => ((IMethod) d).ReturnParameter );

        [Memo]
        public override IDeclaration OriginalDefinition => ((Method) this.DeclaringMember.GetOriginalDefinition()).ReturnParameter;

        [Memo]
        public override IAttributeCollection Attributes
            => new AttributeCollection(
                this,
                this.DeclaringMethod.MethodSymbol.GetReturnTypeAttributes()
                    .Select( a => new AttributeRef( a, this.ToRef() ) )
                    .ToList() );

        public override bool IsReturnParameter => true;

        public override SyntaxTree? PrimarySyntaxTree => ((IDeclarationImpl) this.DeclaringMember).PrimarySyntaxTree;
    }
}