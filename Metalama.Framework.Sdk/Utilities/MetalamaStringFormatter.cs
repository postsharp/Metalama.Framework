// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Linq;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.Utilities
{
    /// <summary>
    /// Formats arguments passed to a diagnostic.
    /// </summary>
    public abstract class MetalamaStringFormatter : CultureInfo, ICustomFormatter
    {
        private static MetalamaStringFormatter? _instance;

        public static MetalamaStringFormatter Instance => _instance ?? throw new InvalidOperationException( "The class has not been initialized." );

        internal static void Initialize( MetalamaStringFormatter impl ) => _instance = impl;

        private protected MetalamaStringFormatter() : base( InvariantCulture.Name ) { }

        public override object? GetFormat( Type? formatType ) => formatType == typeof(ICustomFormatter) ? this : base.GetFormat( formatType );

        public static string Format( FormattableString message ) => message.ToString( Instance );

        public abstract string Format( string format, object arg, IFormatProvider? formatProvider );
    }
}