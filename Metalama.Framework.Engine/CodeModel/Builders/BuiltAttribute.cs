// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities;
using System;
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
            => this.AttributeBuilder.ConstructorArguments.Select(
                    a => new TypedConstant(
                        this.GetCompilationModel().Factory.GetIType( a.Type ),
                        a.Value ) )
                .ToImmutableArray();

        [Memo]
        public ImmutableArray<KeyValuePair<string, TypedConstant>> NamedArguments
            => this.AttributeBuilder.NamedArguments.Select(
                    a => new KeyValuePair<string, TypedConstant>(
                        a.Key,
                        new TypedConstant(
                            this.GetCompilationModel().Factory.GetIType( a.Value.Type ),
                            a.Value.Value ) ) )
                .ToImmutableArray();

        IType IHasType.Type => this.Type;

        public FormattableString FormatPredecessor() => this.AttributeBuilder.FormatPredecessor();
    }
}