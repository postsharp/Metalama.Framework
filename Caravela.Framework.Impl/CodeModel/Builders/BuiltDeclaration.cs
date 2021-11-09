// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    /// <summary>
    /// The base class for the read-only facade of introduced declarations, represented by <see cref="DeclarationBuilder"/>. Facades
    /// are consistent with the consuming <see cref="CompilationModel"/>, while builders are consistent with the producing <see cref="CompilationModel"/>. 
    /// </summary>
    internal abstract class BuiltDeclaration : IDeclarationImpl
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

        public IAssembly DeclaringAssembly => this.Builder.DeclaringAssembly;

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

        protected IDeclaration GetForCompilation( ICompilation compilation ) => this.GetForCompilation( (CompilationModel) compilation );

        protected IDeclaration GetForCompilation( CompilationModel compilation )
            => this.Compilation == compilation ? this : compilation.Factory.GetDeclaration( this.Builder );

        ISymbol? ISdkDeclaration.Symbol => null;

        public Ref<IDeclaration> ToRef() => Ref.FromBuilder( this.Builder );

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => this.Builder.DeclaringSyntaxReferences;

        public bool CanBeInherited => this.Builder.CanBeInherited;

        public IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => throw new NotImplementedException();

        public override string ToString() => this.Builder.ToString();

        [Memo]
        public IDeclaration OriginalDefinition => this.Compilation.Factory.GetDeclaration( this.Builder.OriginalDefinition );
    }
}