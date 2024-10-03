// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Builders.Built;

internal sealed class BuiltAttribute : BuiltDeclaration, IAttribute
{
    private readonly AttributeBuilder _attributeBuilder;

    public BuiltAttribute( AttributeBuilder builder, CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext )
    {
        this._attributeBuilder = builder;
    }

    IDeclaration IAttribute.ContainingDeclaration => this.ContainingDeclaration.AssertNotNull();

    [Memo]
    private IRef<IAttribute> Ref => this._attributeBuilder.ToAttributeRef(); // TODO generic

    public IRef<IAttribute> ToRef() => this.Ref;

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    public override DeclarationBuilder Builder => this._attributeBuilder;

    [Memo]
    public INamedType Type => this.MapType( this._attributeBuilder.Type );

    [Memo]
    public IConstructor Constructor
        => this.Compilation.Factory.Translate( this._attributeBuilder.Constructor, genericContext: this.GenericContext ).AssertNotNull();

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

    IRef<IDeclaration> IAspectPredecessor.TargetDeclaration => this._attributeBuilder.ContainingDeclaration.ToRef();

    ImmutableArray<AspectPredecessor> IAspectPredecessor.Predecessors => ImmutableArray<AspectPredecessor>.Empty;

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default ) => Enumerable.Empty<IDeclaration>();
}