// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Globalization;

namespace Metalama.Framework.Impl.CompileTime.Serialization.Serializers
{
    internal sealed class DottedStringSerializer : IntrinsicSerializer<DottedString>
    {
        public override object Convert( object value, Type targetType )
        {
            return (DottedString) System.Convert.ToString( value, CultureInfo.InvariantCulture );
        }
    }
}