﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        private class PropertyGetReturnParameter : ParameterBase
        {
            public PropertyGetReturnParameter( AccessorBuilder accessor ) : base( accessor, -1 ) { }

            public override IType Type
            {
                get => ((IHasType) this.Accessor.ContainingMember).Type;

                set => throw new NotSupportedException( "Cannot directly change accessor's parameter type." );
            }

            public override RefKind RefKind
            {
                get
                    => this.Accessor.ContainingMember switch
                    {
                        PropertyBuilder propertyBuilder => propertyBuilder.RefKind,
                        FieldBuilder => RefKind.None,
                        _ => throw new AssertionFailedException( $"Unexpected containing member: '{this.Accessor.ContainingMember}'." )
                    };

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