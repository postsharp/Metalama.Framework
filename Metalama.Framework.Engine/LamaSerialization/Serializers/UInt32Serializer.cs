// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Globalization;

namespace Metalama.Framework.Engine.CompileTime.Serialization.Serializers
{
    internal sealed class UInt32Serializer : IntrinsicSerializer<uint>
    {
        public override object Convert( object value, Type targetType )
        {
            return System.Convert.ToUInt32( value, CultureInfo.InvariantCulture );
        }
    }
}