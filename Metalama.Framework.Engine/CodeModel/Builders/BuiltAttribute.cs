// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class BuiltAttribute : BuiltDeclaration, IAttribute
    {
        private readonly AttributeBuilder _attributeBuilder;

        public BuiltAttribute( AttributeBuilder builder, CompilationModel compilation ) : base( compilation, builder )
        {
            this._attributeBuilder = builder;
        }

        IDeclaration IAttribute.ContainingDeclaration => this.ContainingDeclaration.AssertNotNull();

        public override DeclarationBuilder Builder => this._attributeBuilder;

        [Memo]
        public INamedType Type => this.Compilation.Factory.GetDeclaration( this._attributeBuilder.Constructor.DeclaringType );

        [Memo]
        public IConstructor Constructor => this.Compilation.Factory.GetConstructor( this._attributeBuilder.Constructor );

        [Memo]
        public ImmutableArray<TypedConstant> ConstructorArguments
            => this._attributeBuilder.ConstructorArguments.Select( a => a.ForCompilation( this.GetCompilationModel() ) )
                .ToImmutableArray();

        [Memo]
        public INamedArgumentList NamedArguments
            => new NamedArgumentList(
                this._attributeBuilder.NamedArguments.SelectAsList(
                    a => new KeyValuePair<string, TypedConstant>(
                        a.Key,
                        a.Value.ForCompilation( this.GetCompilationModel() ) ) ) );

        int IAspectPredecessor.PredecessorDegree => 0;

        public IRef<IDeclaration> TargetDeclaration => this._attributeBuilder.ContainingDeclaration.ToRef();

        ImmutableArray<AspectPredecessor> IAspectPredecessor.Predecessors => ImmutableArray<AspectPredecessor>.Empty;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default ) => Enumerable.Empty<IDeclaration>();
    }
}