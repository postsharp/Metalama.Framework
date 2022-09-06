﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        private class EventValueParameter : ParameterBase
        {
            public EventValueParameter( AccessorBuilder accessor ) : base( accessor, 0 ) { }

            public override IType Type
            {
                get => ((EventBuilder) this.Accessor.ContainingMember).Type;
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

            public override SyntaxTree? PrimarySyntaxTree => this.Accessor.PrimarySyntaxTree;
        }
    }
}