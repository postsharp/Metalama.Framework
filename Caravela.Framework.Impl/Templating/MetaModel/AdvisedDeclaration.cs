// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.InternalInterfaces;
using Caravela.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal abstract class AdvisedDeclaration<T> : IDeclarationInternal
        where T : IDeclarationInternal
    {
        protected AdvisedDeclaration( T underlying )
        {
            this.Underlying = underlying;
        }

        protected T Underlying { get; }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.Underlying.ToDisplayString( format, context );

        public IDiagnosticLocation? DiagnosticLocation => this.Underlying.DiagnosticLocation;

        public ICompilation Compilation => this.Underlying.Compilation;

        public DeclarationOrigin Origin => this.Underlying.Origin;

        public IDeclaration? ContainingDeclaration => this.Underlying.ContainingDeclaration;

        public IAttributeList Attributes => this.Underlying.Attributes;

        public DeclarationKind DeclarationKind => this.Underlying.DeclarationKind;

        public ISymbol? Symbol => this.Underlying.Symbol;

        public DeclarationRef<IDeclaration> ToRef() => this.Underlying.ToRef();

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => this.Underlying.DeclaringSyntaxReferences;
    }
}