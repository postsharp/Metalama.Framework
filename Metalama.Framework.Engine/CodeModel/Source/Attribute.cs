﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Helpers;
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

namespace Metalama.Framework.Engine.CodeModel.Source;

internal sealed class Attribute : IAttributeImpl
{
    public Attribute( AttributeData data, CompilationModel compilation, IDeclaration containingDeclaration )
    {
        // Note that Roslyn can return an AttributeData that does not belong to the same compilation
        // as the parent symbol, probably because of some bug or optimisation.

        this.AttributeData = data;
        this.Compilation = compilation;
        this.ContainingDeclaration = containingDeclaration;
    }

    public CompilationModel Compilation { get; }

    public ICompilationElement? Translate(
        CompilationModel newCompilation,
        IGenericContext? genericContext = null,
        Type? interfaceType = null )
        => throw new NotImplementedException();

    public AttributeData AttributeData { get; }

    [Memo]
    private IRef<IAttribute> Ref => new SymbolAttributeRef( this.AttributeData, this.ContainingDeclaration.ToRef(), this.Compilation.CompilationContext );

    IRef<IAttribute> IAttribute.ToRef() => this.Ref;

    IRef<IDeclaration> IDeclaration.ToRef() => this.Ref;

    public SerializableDeclarationId ToSerializableId() => throw new NotSupportedException();

    public IAssembly DeclaringAssembly => this.ContainingDeclaration.DeclaringAssembly;

    IDeclarationOrigin IDeclaration.Origin => this.ContainingDeclaration.Origin;

    public IDeclaration ContainingDeclaration { get; }

    IAttributeCollection IDeclaration.Attributes => AttributeCollection.Empty;

    public DeclarationKind DeclarationKind => DeclarationKind.Attribute;

    bool IDeclaration.IsImplicitlyDeclared => false;

    int IDeclaration.Depth => this.GetDepthImpl();

    public bool BelongsToCurrentProject => this.ContainingDeclaration.BelongsToCurrentProject;

    [Memo]
    public ImmutableArray<SourceReference> Sources
        => ((IDeclarationImpl) this).DeclaringSyntaxReferences.SelectAsImmutableArray(
            sr => new SourceReference( sr.GetSyntax(), SourceReferenceImpl.Instance ) );

    public IGenericContext GenericContext => this.ContainingDeclaration.GenericContext;

    ICompilation ICompilationElement.Compilation => this.Compilation;

    [Memo]
    public INamedType Type
        => this.Compilation.Factory.GetNamedType(
            this.AttributeData.AttributeClass.AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedAttributeTypes ) );

    [Memo]
    public IConstructor Constructor
        => this.Compilation.Factory.GetConstructor(
            this.AttributeData.AttributeConstructor.AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedAttributeTypes ) );

    [Memo]
    public ImmutableArray<TypedConstant> ConstructorArguments
        => this.AttributeData.ConstructorArguments.Select( x => x.ToOurTypedConstant( this.Compilation ) ).ToImmutableArray();

    [Memo]
    public INamedArgumentList NamedArguments
        => new NamedArgumentList(
            this.AttributeData.NamedArguments
                .Select( kvp => new KeyValuePair<string, TypedConstant>( kvp.Key, kvp.Value.ToOurTypedConstant( this.Compilation ) ) )
                .ToReadOnlyList() );

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

    ImmutableArray<SyntaxReference> IDeclarationImpl.DeclaringSyntaxReferences
        => this.AttributeData.ApplicationSyntaxReference != null
            ? ImmutableArray.Create( this.AttributeData.ApplicationSyntaxReference )
            : ImmutableArray<SyntaxReference>.Empty;

    bool IDeclarationImpl.CanBeInherited => false;

    SyntaxTree? IDeclarationImpl.PrimarySyntaxTree => this.AttributeData.ApplicationSyntaxReference?.SyntaxTree;

    IEnumerable<IDeclaration> IDeclarationImpl.GetDerivedDeclarations( DerivedTypesOptions options ) => Enumerable.Empty<IDeclaration>();

    public bool Equals( IDeclaration? other ) => other is Attribute attribute && this.AttributeData == attribute.AttributeData;

    public override bool Equals( object? obj ) => obj is Attribute attribute && this.Equals( attribute );

    public override int GetHashCode() => this.AttributeData.GetHashCode();

    int IAspectPredecessor.PredecessorDegree => 0;

    ImmutableArray<SyntaxTree> IAspectPredecessorImpl.PredecessorTreeClosure
        => this.GetPrimarySyntaxTree() is { } tree ? ImmutableArray.Create( tree ) : ImmutableArray<SyntaxTree>.Empty;
}