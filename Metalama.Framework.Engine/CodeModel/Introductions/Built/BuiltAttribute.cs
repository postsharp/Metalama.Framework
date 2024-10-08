// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal sealed class BuiltAttribute : BuiltDeclaration, IAttribute
{
    private readonly AttributeBuilderData _attributeBuilder;

    public BuiltAttribute( AttributeBuilderData builder, CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext )
    {
        this._attributeBuilder = builder;
    }

    IDeclaration IAttribute.ContainingDeclaration => this.ContainingDeclaration.AssertNotNull();

    [Memo]
    private IFullRef<IAttribute> Ref => this._attributeBuilder.ToRef();

    public IRef<IAttribute> ToRef() => this.Ref;

    private protected override IFullRef<IDeclaration> ToDeclarationRef() => this.Ref;

    public override DeclarationBuilderData BuilderData => this._attributeBuilder;

    [Memo]
    public INamedType Type => this.Constructor.DeclaringType;

    [Memo]
    public IConstructor Constructor => this.MapDeclaration( this._attributeBuilder.Constructor ).AssertNotNull();

    [Memo]
    public ImmutableArray<TypedConstant> ConstructorArguments
        => this._attributeBuilder.ConstructorArguments.Select( a => a.ForCompilation( this.Compilation ) )
            .ToImmutableArray();

    [Memo]
    public INamedArgumentList NamedArguments
        => new NamedArgumentList(
            this._attributeBuilder.NamedArguments.SelectAsArray(
                a => new KeyValuePair<string, TypedConstant>(
                    a.Key,
                    a.Value.ForCompilation( this.Compilation ) ) ) );

    int IAspectPredecessor.PredecessorDegree => 0;

    IRef<IDeclaration> IAspectPredecessor.TargetDeclaration => this._attributeBuilder.ContainingDeclaration;

    ImmutableArray<AspectPredecessor> IAspectPredecessor.Predecessors => ImmutableArray<AspectPredecessor>.Empty;

    public override bool CanBeInherited => false;

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default ) => [];
}