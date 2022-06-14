// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        internal abstract class ParameterBase : BaseParameterBuilder
        {
            protected AccessorBuilder Accessor { get; }

            protected ParameterBase( AccessorBuilder accessor, int index ) : base( accessor.ParentAdvice )
            {
                this.Accessor = accessor;
                this.Index = index;
            }

            public override TypedConstant DefaultValue
            {
                get => TypedConstant.Null;
                set
                    => throw new NotSupportedException(
                        "Cannot directly set the default value of indexer accessor parameter, set the value on indexer itself." );
            }

            public override RefKind RefKind { get; set; }

            public override int Index { get; }

            public override bool IsParams
            {
                get => false;
                set => throw new NotSupportedException();
            }

            public override IDeclaration? ContainingDeclaration => this.Accessor;

            public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;

            public override IHasParameters DeclaringMember => (IHasParameters) this.Accessor.ContainingDeclaration.AssertNotNull();

            public override ParameterInfo ToParameterInfo()
            {
                throw new NotImplementedException();
            }

            public override bool IsReturnParameter => this.Index < 0;

            public override bool CanBeInherited => ((IDeclarationImpl) this.DeclaringMember).CanBeInherited;
        }
    }
}