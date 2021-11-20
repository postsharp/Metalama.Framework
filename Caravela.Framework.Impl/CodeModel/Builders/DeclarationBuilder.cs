// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    /// <summary>
    /// Base class implementing <see cref="IDeclarationBuilder"/>. These classes are returned by introduction advices so the user can continue
    /// specifying the introduced declaration. They are bound to the <see cref="CompilationModel"/> that created them, but implement
    /// <see cref="ISdkRef{T}"/> so they can resolve, using <see cref="DeclarationFactory"/>, to the consuming <see cref="CompilationModel"/>.
    /// 
    /// </summary>
    internal abstract class DeclarationBuilder : IDeclarationBuilder, IDeclarationImpl, ITransformation
    {
        internal Advice ParentAdvice { get; }

        public DeclarationOrigin Origin => DeclarationOrigin.Aspect;

        public abstract IDeclaration? ContainingDeclaration { get; }

        IAttributeList IDeclaration.Attributes => this.Attributes;

        public AttributeBuilderList Attributes { get; } = new();

        public abstract DeclarationKind DeclarationKind { get; }

        ICompilation ICompilationElement.Compilation => this.Compilation;

        public CompilationModel Compilation => (CompilationModel?) this.ContainingDeclaration?.Compilation ?? throw new AssertionFailedException();

        public bool IsFrozen { get; private set; }

        Advice ITransformation.Advice => this.ParentAdvice;

        public DeclarationBuilder( Advice parentAdvice )
        {
            this.ParentAdvice = parentAdvice;
        }

        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );

        public void AddAttribute( AttributeConstruction attribute )
        {
            throw new NotImplementedException();
        }

        public void RemoveAttributes( INamedType type ) => throw new NotImplementedException();

        public virtual void Freeze() => this.IsFrozen = true;

        public IDiagnosticLocation? DiagnosticLocation => this.ContainingDeclaration?.DiagnosticLocation;

        public Ref<IDeclaration> ToRef() => Ref.FromBuilder( this );

        ISymbol? ISdkDeclaration.Symbol => null;

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
            => ((IDeclarationImpl?) this.ContainingDeclaration)?.DeclaringSyntaxReferences ?? ImmutableArray<SyntaxReference>.Empty;

        public abstract bool CanBeInherited { get; }

        public IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => throw new NotImplementedException();

        public override string ToString() => this.ToDisplayString( CodeDisplayFormat.MinimallyQualified );

        public IDeclaration OriginalDefinition => this;

        public IAssembly DeclaringAssembly => this.Compilation.DeclaringAssembly;
    }
}