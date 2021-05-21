// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    /// <summary>
    /// The base class for the read-only facade of introduced declarations, represented by <see cref="DeclarationBuilder"/>. Facades
    /// are consistent with the consuming <see cref="CompilationModel"/>, while builders are consistent with the producing <see cref="CompilationModel"/>. 
    /// </summary>
    internal abstract class BuiltDeclaration : IDeclarationInternal
    {
        protected BuiltDeclaration( CompilationModel compilation )
        {
            this.Compilation = compilation;
        }

        public CompilationModel Compilation { get; }

        public abstract DeclarationBuilder Builder { get; }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.Builder.ToDisplayString( format, context );

        public IDiagnosticLocation? DiagnosticLocation => this.Builder.DiagnosticLocation;

        DeclarationOrigin IDeclaration.Origin => DeclarationOrigin.Aspect;

        public IDeclaration? ContainingDeclaration => this.Builder.ContainingDeclaration;

        [Memo]
        public IAttributeList Attributes
            => new AttributeList(
                this,
                this.Builder.Attributes
                    .Select<AttributeBuilder, AttributeRef>( a => new AttributeRef( a ) ) );

        public DeclarationKind DeclarationKind => this.Builder.DeclarationKind;

        ICompilation ICompilationElement.Compilation => this.Compilation;

        protected IDeclaration GetForCompilation( CompilationModel compilation )
            => this.Compilation == compilation ? this : compilation.Factory.GetDeclaration( this.Builder );

        ISymbol? ISdkDeclaration.Symbol => null;

        public DeclarationRef<IDeclaration> ToRef() => DeclarationRef.FromBuilder( this.Builder );

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => this.Builder.DeclaringSyntaxReferences;
    }
}