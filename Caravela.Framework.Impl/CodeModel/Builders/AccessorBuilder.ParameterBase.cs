// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using System;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        // TODO: Move all types into separate files.

        private abstract class ParameterBase : DeclarationBuilder, IParameterBuilder
        {
            protected AccessorBuilder Accessor { get; }

            public ParameterBase( AccessorBuilder accessor, int index ) : base( accessor.ParentAdvice )
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

            public abstract IType ParameterType { get; set; }

            public abstract RefKind RefKind { get; set; }

            public abstract string Name { get; set; }

            public int Index { get; }

            public virtual bool IsParams => false;

            public override IDeclaration? ContainingDeclaration => this.Accessor;

            public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;

            public IMemberOrNamedType DeclaringMember => (IMemberOrNamedType) this.Accessor.ContainingDeclaration.AssertNotNull();

            IType IParameter.ParameterType => this.ParameterType;

            TypedConstant IParameter.DefaultValue => this.DefaultValue;

            [return: RunTimeOnly]
            public ParameterInfo ToParameterInfo()
            {
                throw new NotImplementedException();
            }
        }
    }
}