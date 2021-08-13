// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        private class EventValueParameter : ParameterBase
        {
            public EventValueParameter( AccessorBuilder accessor ) : base( accessor, 0 ) { }

            public override IType ParameterType
            {
                get => ((EventBuilder) this.Accessor._containingDeclaration).EventType;
                set => throw new NotSupportedException( "Cannot directly change accessor's value parameter type." );
            }

            public override RefKind RefKind
            {
                get => RefKind.None;
                set => throw new NotSupportedException( "Cannot directly change accessor's value parameter reference kind." );
            }

            public override string Name
            {
                get => "value";
                set => throw new NotSupportedException( "Cannot set the name of a value parameter." );
            }

            public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
                => this.Accessor.ToDisplayString( format, context ) + "@value";
        }
    }
}