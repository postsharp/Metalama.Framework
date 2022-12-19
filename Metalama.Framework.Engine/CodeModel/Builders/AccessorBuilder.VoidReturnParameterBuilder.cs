﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities;
using System;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        private sealed class VoidReturnParameterBuilder : ParameterBuilderBase
        {
            public VoidReturnParameterBuilder( AccessorBuilder accessor ) : base( accessor, -1 ) { }

            [Memo]
            public override IType Type
            {
                get => this.Compilation.Factory.GetTypeByReflectionType( typeof(void) );
                set => throw new NotSupportedException( "Cannot directly change accessor's parameter type." );
            }

            public override RefKind RefKind
            {
                get => RefKind.None;
                set => throw new NotSupportedException( "Cannot directly change accessor's parameter reference kind." );
            }

            public override string Name
            {
                get => throw new NotSupportedException( "Cannot get the name of a return parameter." );
                set => throw new NotSupportedException( "Cannot set the name of a return parameter." );
            }

            public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
                => this.Accessor.ToDisplayString( format, context ) + "@<return>";
        }
    }
}