// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Globalization;

namespace Metalama.Framework.Engine.LamaSerialization.Serializers
{
    internal sealed class DoubleSerializer : IntrinsicSerializer<double>
    {
        public override object Convert( object value, Type targetType )
        {
            return System.Convert.ToDouble( value, CultureInfo.InvariantCulture );
        }
    }
}