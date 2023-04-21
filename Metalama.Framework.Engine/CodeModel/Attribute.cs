// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class Attribute : IAttributeImpl
    {
        private readonly CompilationModel _compilation;

        public Attribute( AttributeData data, CompilationModel compilation, IDeclaration containingDeclaration )
        {
            this.AttributeData = data;
            this._compilation = compilation;
            this.ContainingDeclaration = containingDeclaration;
        }

        public AttributeData AttributeData { get; }

        IRef<IDeclaration> IDeclaration.ToRef() => new AttributeRef( this.AttributeData, ((IDeclarationImpl) this.ContainingDeclaration).ToRef() );

        public SerializableDeclarationId ToSerializableId() => throw new NotSupportedException();

        public IAssembly DeclaringAssembly => this.ContainingDeclaration.DeclaringAssembly;

        IDeclarationOrigin IDeclaration.Origin => this.ContainingDeclaration.Origin;

        public IDeclaration ContainingDeclaration { get; }

        IAttributeCollection IDeclaration.Attributes => AttributeCollection.Empty;

        public DeclarationKind DeclarationKind => DeclarationKind.Attribute;

        bool IDeclaration.IsImplicitlyDeclared => false;

        int IDeclaration.Depth => this.GetDepthImpl();

        public bool BelongsToCurrentProject => this.ContainingDeclaration.BelongsToCurrentProject;

        public ICompilation Compilation => this.Constructor.Compilation;

        [Memo]
        public INamedType Type => this._compilation.Factory.GetNamedType( this.AttributeData.AttributeClass.AssertNotNull() );

        [Memo]
        public IConstructor Constructor => this._compilation.Factory.GetConstructor( this.AttributeData.AttributeConstructor.AssertNotNull() );

        [Memo]
        public ImmutableArray<TypedConstant> ConstructorArguments => this.AttributeData.ConstructorArguments.Select( this.Translate ).ToImmutableArray();

        [Memo]
        public INamedArgumentList NamedArguments
            => new NamedArgumentList(
                this.AttributeData.NamedArguments.Select( kvp => new KeyValuePair<string, TypedConstant>( kvp.Key, this.Translate( kvp.Value ) ) )
                    .ToReadOnlyList() );

        private TypedConstant Translate( Microsoft.CodeAnalysis.TypedConstant constant )
        {
            var type = this._compilation.Factory.GetIType( constant.Type.AssertNotNull() );

            var value = constant.Kind switch
            {
                TypedConstantKind.Primitive or TypedConstantKind.Enum => constant.Value,
                TypedConstantKind.Type => constant.Value == null ? null : this._compilation.Factory.GetIType( (ITypeSymbol) constant.Value ),
                TypedConstantKind.Array => constant.Values.Select( this.Translate ).ToImmutableArray(),
                _ => throw new ArgumentException( nameof(constant) )
            };

            return TypedConstant.CreateUnchecked( value, type );
        }

        public override string? ToString() => this.AttributeData.ToString();

        T IMeasurableInternal.GetMetric<T>() => throw new NotSupportedException();

        public FormattableString FormatPredecessor( ICompilation compilation ) => $"the attribute of type '{this.Type}' on '{this.ContainingDeclaration}'";

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => throw new NotImplementedException();

        IDeclaration IDeclaration.ContainingDeclaration => this.ContainingDeclaration;

        Location? IAspectPredecessorImpl.GetDiagnosticLocation( Compilation compilation ) => this.DiagnosticLocation;

        int IAspectPredecessorImpl.TargetDeclarationDepth => this.ContainingDeclaration.Depth;

        IRef<IDeclaration> IAspectPredecessor.TargetDeclaration => this.ContainingDeclaration.ToRef();

        ImmutableArray<AspectPredecessor> IAspectPredecessor.Predecessors => ImmutableArray<AspectPredecessor>.Empty;

        public Location? DiagnosticLocation => this.AttributeData.GetDiagnosticLocation();

        ISymbol? ISdkDeclaration.Symbol => null;

        IDeclaration IDeclarationInternal.OriginalDefinition => this;

        ImmutableArray<SyntaxReference> IDeclarationImpl.DeclaringSyntaxReferences
            => this.AttributeData.ApplicationSyntaxReference != null
                ? ImmutableArray.Create( this.AttributeData.ApplicationSyntaxReference )
                : ImmutableArray<SyntaxReference>.Empty;

        bool IDeclarationImpl.CanBeInherited => false;

        SyntaxTree? IDeclarationImpl.PrimarySyntaxTree => this.AttributeData.ApplicationSyntaxReference?.SyntaxTree;

        IEnumerable<IDeclaration> IDeclarationImpl.GetDerivedDeclarations( DerivedTypesOptions options ) => Enumerable.Empty<IDeclaration>();

        Ref<IDeclaration> IDeclarationImpl.ToRef() => throw new NotSupportedException( "Attribute is represented by an AttributeRef." );

        public bool Equals( IDeclaration? other ) => other is Attribute attribute && this.AttributeData == attribute.AttributeData;

        public override bool Equals( object? obj ) => obj is Attribute attribute && this.Equals( attribute );

        public override int GetHashCode() => this.AttributeData.GetHashCode();

        int IAspectPredecessor.PredecessorDegree => 0;
    }
}