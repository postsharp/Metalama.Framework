// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.AdviceImpl.Attributes;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class AttributeBuilder : DeclarationBuilder, IAttributeImpl
{
    private AttributeRef? _attributeRef;

    internal IAttributeData AttributeConstruction { get; }

    public AttributeBuilder( Advice advice, IDeclaration containingDeclaration, IAttributeData attributeConstruction ) : base( advice )
    {
        this.AttributeConstruction = attributeConstruction;
        this.ContainingDeclaration = containingDeclaration;
    }

    string IDisplayable.ToDisplayString( CodeDisplayFormat? format, CodeDisplayContext? context ) => throw new NotImplementedException();

    public override bool CanBeInherited => false;

    public override IDeclaration ContainingDeclaration { get; }

    IRef<IAttribute> IAttribute.ToRef() => this.ToAttributeRef();

    IDeclaration IDeclaration.ContainingDeclaration => this.ContainingDeclaration;

    IAttributeCollection IDeclaration.Attributes => AttributeCollection.Empty;

    public override DeclarationKind DeclarationKind => DeclarationKind.Attribute;
    
    public INamedType Type => this.Constructor.DeclaringType;

    public IConstructor Constructor => this.AttributeConstruction.Constructor;

    public ImmutableArray<TypedConstant> ConstructorArguments => this.AttributeConstruction.ConstructorArguments;

    public INamedArgumentList NamedArguments => this.AttributeConstruction.NamedArguments;

    public FormattableString FormatPredecessor( ICompilation compilation ) => $"attribute of type '{this.Type}' on '{this.ContainingDeclaration}'";

    Location? IAspectPredecessorImpl.GetDiagnosticLocation( Compilation compilation ) => null;

    int IAspectPredecessorImpl.TargetDeclarationDepth => this.ContainingDeclaration.Depth + 1;

    [Memo]
    public override SyntaxTree PrimarySyntaxTree
        => this.ContainingDeclaration.GetPrimarySyntaxTree() ?? this.Compilation.PartialCompilation.SyntaxTreeForCompilationLevelAttributes;

    public ITransformation ToTransformation()
    {
        this.Freeze();

        return new IntroduceAttributeTransformation( this.ParentAdvice, this );
    }

    int IAspectPredecessor.PredecessorDegree => 0;

    IRef<IDeclaration> IAspectPredecessor.TargetDeclaration => this.ContainingDeclaration.ToRef();

    ImmutableArray<AspectPredecessor> IAspectPredecessor.Predecessors => ImmutableArray<AspectPredecessor>.Empty;

    ImmutableArray<SyntaxTree> IAspectPredecessorImpl.PredecessorTreeClosure => ImmutableArray<SyntaxTree>.Empty;

    public AttributeRef ToAttributeRef() => this._attributeRef ??= new BuilderAttributeRef( this );

    public override IRef<IDeclaration> ToDeclarationRef() => this.ToAttributeRef();
}