// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Globalization;

namespace Caravela.Framework.Impl.CompileTime.Serialization.Serializers
{
    internal sealed class DottedStringSerializer : IntrinsicSerializer<DottedString>
    {
        public override object Convert( object value, Type targetType )
        {
            return (DottedString)  System.Convert.ToString( value, CultureInfo.InvariantCulture);
        }
    }
}
