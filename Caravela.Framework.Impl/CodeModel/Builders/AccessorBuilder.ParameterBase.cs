// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using System;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        internal abstract class ParameterBase : DeclarationBuilder, IParameterBuilder
        {
            protected AccessorBuilder Accessor { get; }

            protected ParameterBase( AccessorBuilder accessor, int index ) : base( accessor.ParentAdvice )
            {
                this.Accessor = accessor;
                this.Index = index;
            }

            public virtual TypedConstant DefaultValue
            {
                get => TypedConstant.Null;
                set
                    => throw new NotSupportedException(
                        "Cannot directly set the default value of indexer accessor parameter, set the value on indexer itself." );
            }

            public abstract IType Type { get; set; }

            public abstract RefKind RefKind { get; set; }

            public abstract string Name { get; set; }

            public int Index { get; }

            public virtual bool IsParams => false;

            public override IDeclaration? ContainingDeclaration => this.Accessor;

            public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;

            public IMemberOrNamedType DeclaringMember => (IMemberOrNamedType) this.Accessor.ContainingDeclaration.AssertNotNull();

            TypedConstant IParameter.DefaultValue => this.DefaultValue;

            public ParameterInfo ToParameterInfo()
            {
                throw new NotImplementedException();
            }

            public bool IsReturnParameter => this.Index < 0;

            public override bool CanBeInherited => ((IDeclarationImpl) this.DeclaringMember).CanBeInherited;
        }
    }
}