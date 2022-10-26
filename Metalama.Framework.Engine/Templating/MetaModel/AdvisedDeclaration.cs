// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    internal abstract class AdvisedDeclaration<T> : IDeclarationImpl
        where T : IDeclarationImpl
    {
        protected AdvisedDeclaration( T underlying )
        {
            this.Underlying = underlying;
        }

        protected T Underlying { get; }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.Underlying.ToDisplayString( format, context );

        public Location? DiagnosticLocation => this.Underlying.DiagnosticLocation;

        public ICompilation Compilation => this.Underlying.Compilation;

        IRef<IDeclaration> IDeclaration.ToRef() => this.Underlying.ToRef();

        public IAssembly DeclaringAssembly => this.Underlying.DeclaringAssembly;

        public DeclarationOrigin Origin => this.Underlying.Origin;

        public IDeclaration? ContainingDeclaration => this.Underlying.ContainingDeclaration;

        public IAttributeCollection Attributes => this.Underlying.Attributes;

        public DeclarationKind DeclarationKind => this.Underlying.DeclarationKind;

        public bool IsImplicitlyDeclared => this.Underlying.IsImplicitlyDeclared;

        public ISymbol? Symbol => this.Underlying.Symbol;

        public Ref<IDeclaration> ToRef() => this.Underlying.ToRef();

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => this.Underlying.DeclaringSyntaxReferences;

        public bool CanBeInherited => this.Underlying.CanBeInherited;

        public IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => this.Underlying.GetDerivedDeclarations();

        public bool Equals( IDeclaration? other ) => this.Underlying.Equals( other );

        public override string? ToString() => this.Underlying.ToString();

        public TExtension GetMetric<TExtension>()
            where TExtension : IMetric
            => this.Underlying.GetMetric<TExtension>();

        public IDeclaration OriginalDefinition => this.Underlying.OriginalDefinition;

        public SyntaxTree? PrimarySyntaxTree => this.Underlying.PrimarySyntaxTree;

        public override int GetHashCode() => this.Underlying.GetHashCode();
    }
}