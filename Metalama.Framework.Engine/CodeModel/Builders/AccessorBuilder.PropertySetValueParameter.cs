// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal partial class AccessorBuilder
    {
        private class PropertySetValueParameter : ParameterBase
        {
            public PropertySetValueParameter( AccessorBuilder accessor, int index ) : base( accessor, index ) { }

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
                        _ => throw new AssertionFailedException()
                    };

                set => throw new NotSupportedException( "Cannot directly change accessor's parameter reference kind." );
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