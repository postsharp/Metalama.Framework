// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Globalization;

namespace Caravela.Framework
{
    public static class CaravelaServices
    {
        public static IFormatProvider FormatProvider { get; private set; } = CultureInfo.InvariantCulture;

        internal static void Initialize( IFormatProvider formatProvider )
        {
            FormatProvider = formatProvider;
        }
    }
}