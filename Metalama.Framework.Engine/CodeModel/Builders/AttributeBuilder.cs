// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class AttributeBuilder : DeclarationBuilder, IAttributeImpl
{
    private readonly IAttributeData _attributeConstruction;

    public AttributeBuilder( Advice advice, IDeclaration containingDeclaration, IAttributeData attributeConstruction ) : base( advice )
    {
        this._attributeConstruction = attributeConstruction;
        this.ContainingDeclaration = containingDeclaration;
    }

    string IDisplayable.ToDisplayString( CodeDisplayFormat? format, CodeDisplayContext? context ) => throw new NotImplementedException();

    public override bool CanBeInherited => false;

    public override IDeclaration ContainingDeclaration { get; }

    IDeclaration IDeclaration.ContainingDeclaration => this.ContainingDeclaration;

    IAttributeCollection IDeclaration.Attributes => AttributeCollection.Empty;

    public override DeclarationKind DeclarationKind => DeclarationKind.Attribute;

    public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        => this._attributeConstruction.ToString() ?? "";

    public INamedType Type => this.Constructor.DeclaringType;

    public IConstructor Constructor => this._attributeConstruction.Constructor;

    public ImmutableArray<TypedConstant> ConstructorArguments => this._attributeConstruction.ConstructorArguments;

    public INamedArgumentList NamedArguments => this._attributeConstruction.NamedArguments;

    public FormattableString FormatPredecessor( ICompilation compilation ) => $"attribute of type '{this.Type}' on '{this.ContainingDeclaration}'";

    Location? IAspectPredecessorImpl.GetDiagnosticLocation( Compilation compilation ) => null;

    int IAspectPredecessorImpl.TargetDeclarationDepth => this.ContainingDeclaration.Depth + 1;

    [Memo]
    public override SyntaxTree PrimarySyntaxTree
        => this.ContainingDeclaration.GetPrimarySyntaxTree() ?? this.Compilation.PartialCompilation.SyntaxTreeForCompilationLevelAttributes;

    public ITransformation ToTransformation() => new IntroduceAttributeTransformation( this.ParentAdvice, this );

    int IAspectPredecessor.PredecessorDegree => 0;

    IRef<IDeclaration> IAspectPredecessor.TargetDeclaration => this.ContainingDeclaration.ToRef();

    ImmutableArray<AspectPredecessor> IAspectPredecessor.Predecessors => ImmutableArray<AspectPredecessor>.Empty;
}