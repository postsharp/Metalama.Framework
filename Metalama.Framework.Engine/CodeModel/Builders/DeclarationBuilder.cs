// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel.Builders
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

        protected DeclarationBuilder( Advice parentAdvice )
        {
            this.ParentAdvice = parentAdvice;
        }

        public abstract string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null );

        public void AddAttribute( AttributeConstruction attribute ) => this.Attributes.Add( new AttributeBuilder( this, attribute ) );

        public void RemoveAttributes( INamedType type ) => this.Attributes.RemoveAll( a => a.Type.Is( type ) );

        public virtual void Freeze() => this.IsFrozen = true;

        public Ref<IDeclaration> ToRef() => Ref.FromBuilder( this );

        IRef<IDeclaration> IDeclaration.ToRef() => this.ToRef();

        ISymbol? ISdkDeclaration.Symbol => null;

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
            => ((IDeclarationImpl?) this.ContainingDeclaration)?.DeclaringSyntaxReferences ?? ImmutableArray<SyntaxReference>.Empty;

        public abstract bool CanBeInherited { get; }

        public IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => throw new NotImplementedException();

        public override string ToString() => this.ToDisplayString( CodeDisplayFormat.MinimallyQualified );

        public IDeclaration OriginalDefinition => this;

        public IAssembly DeclaringAssembly => this.Compilation.DeclaringAssembly;

        // TODO: should we locate diagnostic on the aspect attribute?
        public Location? DiagnosticLocation => null;

        public TExtension GetMetric<TExtension>()
            where TExtension : IMetric
            => this.GetCompilationModel().MetricManager.GetMetric<TExtension>( this );
    }
}