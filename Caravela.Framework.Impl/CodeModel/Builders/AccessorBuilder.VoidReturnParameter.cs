// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        private class VoidReturnParameter : ParameterBase
        {
            public VoidReturnParameter( AccessorBuilder accessor ) : base( accessor, -1 ) { }

            [Memo]
            public override IType ParameterType
            {
                get => this.Compilation.Factory.GetTypeByReflectionType( typeof(void) );
                set => throw new NotSupportedException( "Cannot directly change accessor's parameter type." );
            }

            public override RefKind RefKind
            {
                get => RefKind.None;
                set => throw new NotSupportedException( "Cannot directly change accessor's parameter reference kind." );
            }

            public override string Name => throw new NotSupportedException( "Cannot get the name of a return parameter." );

            public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
                => this.Accessor.ToDisplayString( format, context ) + "@<return>";
        }
    }
}