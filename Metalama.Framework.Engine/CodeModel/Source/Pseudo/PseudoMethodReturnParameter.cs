// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RefKind = Microsoft.CodeAnalysis.RefKind;

namespace Metalama.Framework.Engine.CodeModel.Source.Pseudo
{
    internal sealed class PseudoMethodReturnParameter : PseudoReturnParameter
    {
        private readonly IMethodSymbol _methodSymbol;

        private SourceMethod DeclaringMethod { get; }

        public override IHasParameters DeclaringMember => this.DeclaringMethod;

        public PseudoMethodReturnParameter( SourceMethod declaringMethod, IMethodSymbol methodSymbol )
        {
            this._methodSymbol = methodSymbol;
            this.DeclaringMethod = declaringMethod;
        }

        protected override RefKind SymbolRefKind => this._methodSymbol.RefKind;

        public override IType Type => this.DeclaringMethod.ReturnType;

        public override bool Equals( IDeclaration? other )
            => other is PseudoMethodReturnParameter methodReturnParameter &&
               this._methodSymbol.Equals( methodReturnParameter._methodSymbol );

        public override bool IsImplicitlyDeclared => this.DeclaringMethod.IsImplicitlyDeclared;

        public override ImmutableArray<SourceReference> Sources => ImmutableArray<SourceReference>.Empty;

        public override ISymbol? Symbol => null;

        internal override ICompilationElement? Translate(
            CompilationModel newCompilation,
            IGenericContext? genericContext = null,
            Type? interfaceType = null )
            => ((IMethod?) this.DeclaringMethod.Translate( newCompilation, genericContext ))?.ReturnParameter;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default )
            => this.DeclaringMethod.GetDerivedDeclarations( options ).Select( d => ((IMethod) d).ReturnParameter );

        [Memo]
        public override IAttributeCollection Attributes
            => new AttributeCollection(
                this,
                this._methodSymbol.GetReturnTypeAttributes()
                    .Select( a => new SymbolAttributeRef( a, this.ToFullDeclarationRef(), this.Compilation.RefFactory ) )
                    .ToReadOnlyList() );

        public override bool IsReturnParameter => true;

        public override SyntaxTree? PrimarySyntaxTree => ((IDeclarationImpl) this.DeclaringMember).PrimarySyntaxTree;
    }
}