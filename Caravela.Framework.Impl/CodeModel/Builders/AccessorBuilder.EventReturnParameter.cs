// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        private class EventReturnParameter : ParameterBase
        {
            public EventReturnParameter( AccessorBuilder accessor ) : base( accessor, -1 ) { }

            public override IType ParameterType
            {
                get => this.Compilation.Factory.GetSpecialType( SpecialType.Void );
                set => throw new NotSupportedException( "Cannot change event accessor's return parameter type." );
            }

            public override RefKind RefKind
            {
                get => RefKind.None;
                set => throw new NotSupportedException( "Cannot change event accessor's return parameter reference kind." );
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