// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class BuiltAttribute : BuiltDeclaration, IAttribute
    {
        public BuiltAttribute( AttributeBuilder builder, CompilationModel compilation ) : base( compilation, builder )
        {
            this.AttributeBuilder = builder;
        }

        IDeclaration IAttribute.ContainingDeclaration => this.ContainingDeclaration.AssertNotNull();

        public AttributeBuilder AttributeBuilder { get; }

        public override DeclarationBuilder Builder => this.AttributeBuilder;

        [Memo]
        public INamedType Type => this.Compilation.Factory.GetDeclaration( this.AttributeBuilder.Constructor.DeclaringType );

        [Memo]
        public IConstructor Constructor => this.Compilation.Factory.GetConstructor( this.AttributeBuilder.Constructor );

        [Memo]
        public ImmutableArray<TypedConstant> ConstructorArguments
            => this.AttributeBuilder.ConstructorArguments.Select( a => TypedConstant.Create( a.Value, this.GetCompilationModel().Factory.GetIType( a.Type ) ) )
                .ToImmutableArray();

        [Memo]
        public INamedArgumentList NamedArguments
            => new NamedArgumentList(
                this.AttributeBuilder.NamedArguments.SelectList(
                    a => new KeyValuePair<string, TypedConstant>(
                        a.Key,
                        TypedConstant.Create( a.Value.Value, this.GetCompilationModel().Factory.GetIType( a.Value.Type ) ) ) ) );

        int IAspectPredecessor.PredecessorDegree => 0;

        public IRef<IDeclaration> TargetDeclaration => this.AttributeBuilder.ContainingDeclaration.ToRef();

        ImmutableArray<AspectPredecessor> IAspectPredecessor.Predecessors => ImmutableArray<AspectPredecessor>.Empty;
    }
}