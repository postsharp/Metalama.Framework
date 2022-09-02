// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Globalization;

namespace Metalama.Framework.Engine.LamaSerialization.Serializers
{
    internal sealed class SingleSerializer : IntrinsicSerializer<float>
    {
        public override object Convert( object value, Type targetType )
        {
            return System.Convert.ToSingle( value, CultureInfo.InvariantCulture );
        }
    }
}