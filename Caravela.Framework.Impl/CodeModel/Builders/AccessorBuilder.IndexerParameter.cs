// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        // ReSharper disable once UnusedType.Local
        // TODO: Use this type and remove the warning waiver.

        private class IndexerParameter : ParameterBase
        {
            public IndexerParameter( AccessorBuilder accessor, int index ) : base( accessor, index ) { }

            public override IType ParameterType
            {
                get => ((PropertyBuilder) this.Accessor._containingDeclaration).Parameters[this.Index].ParameterType;
                set => throw new NotSupportedException( "Cannot directly change accessor's parameter type." );
            }

            public override RefKind RefKind
            {
                get => ((PropertyBuilder) this.Accessor._containingDeclaration).Parameters[this.Index].RefKind;
                set => throw new NotSupportedException( "Cannot directly change accessor's parameter reference kind." );
            }

            public override bool IsParams => ((PropertyBuilder) this.Accessor._containingDeclaration).Parameters[this.Index].IsParams;

            public override string Name => throw new NotSupportedException( "Cannot get the name of a return parameter." );
        }
    }
}