// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal partial class AccessorBuilder
{
    internal abstract class ParameterBuilderBase : BaseParameterBuilder
    {
        protected AccessorBuilder Accessor { get; }

        protected ParameterBuilderBase( AccessorBuilder accessor, int index ) : base( accessor.ParentAdvice )
        {
            this.Accessor = accessor;
            this.Index = index;
        }

        public override TypedConstant? DefaultValue
        {
            get => null;
            set => throw new NotSupportedException( "Cannot directly set the default value of indexer accessor parameter, set the value on indexer itself." );
        }

        public override RefKind RefKind { get; set; }

        public override int Index { get; }

        public override bool IsParams => false;

        public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;

        public override IHasParameters DeclaringMember => this.Accessor;

        public override ParameterInfo ToParameterInfo()
        {
            throw new NotImplementedException();
        }

        public override bool IsReturnParameter => this.Index < 0;

        public override bool CanBeInherited => ((IDeclarationImpl) this.DeclaringMember).CanBeInherited;
    }
}