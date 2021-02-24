// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class BuiltAttribute : BuiltCodeElement, IAttribute
    {
        public BuiltAttribute( AttributeBuilder builder, CompilationModel compilation ) : base( compilation )
        {
            this.AttributeBuilder = builder;
        }

        public AttributeBuilder AttributeBuilder { get; }

        public override CodeElementBuilder Builder => this.AttributeBuilder;

        [Memo]
        public INamedType Type => this.Compilation.Factory.GetCodeElement( this.AttributeBuilder.Constructor.DeclaringType );

        [Memo]
        public IMethod Constructor => this.Compilation.Factory.GetMethod( this.AttributeBuilder.Constructor );

        public IReadOnlyList<TypedConstant> ConstructorArguments => this.AttributeBuilder.ConstructorArguments;

        public INamedArgumentList NamedArguments => this.AttributeBuilder.NamedArguments;
    }
}