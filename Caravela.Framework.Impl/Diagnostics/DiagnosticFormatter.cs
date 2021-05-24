// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Linq;
using Accessibility = Caravela.Framework.Code.Accessibility;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Formats arguments passed to a diagnostic.
    /// </summary>
    internal sealed class DiagnosticFormatter : IFormatProvider, ICustomFormatter
    {
        public static readonly DiagnosticFormatter Instance = new();

        object? IFormatProvider.GetFormat( Type formatType ) => formatType == typeof(ICustomFormatter) ? this : null;

        public string Format( string format, object? arg, IFormatProvider formatProvider )
        {
            switch ( arg )
            {
                case IDisplayable displayable:
                    try
                    {
                        return displayable.ToDisplayString( CodeDisplayFormat.ShortDiagnosticMessage );
                    }
                    catch
                    {
                        try
                        {
                            return displayable.ToString();
                        }
                        catch
                        {
                            return displayable.GetType().Name;
                        }
                    }

                case DeclarationKind declarationKind:
                    switch ( declarationKind )
                    {
                        case DeclarationKind.GenericParameter:
                            return "generic parameter";

                        case DeclarationKind.ManagedResource:
                            return "managed resource";

                        case DeclarationKind.ReferencedAssembly:
                            return "reference assembly";

                        default:
                            return declarationKind.ToString().ToLowerInvariant();
                    }

                case Accessibility accessibility:
                    switch ( accessibility )
                    {
                        case Accessibility.Private:
                            return "private";

                        case Accessibility.ProtectedInternal:
                            return "protected internal";

                        case Accessibility.Protected:
                            return "protected";

                        case Accessibility.PrivateProtected:
                            return "private protected";

                        case Accessibility.Internal:
                            return "internal";

                        case Accessibility.Public:
                            return "public";

                        default:
                            return accessibility.ToString().ToLowerInvariant();
                    }

                case ISymbol symbol:
                    return symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat );

                case IFormattable formattable:
                    return formattable.ToString( format, CultureInfo.CurrentCulture );

                case string[] strings:
                    return string.Join( ", ", strings.Select( s => s == null ? null : "'" + s + "'" ) );

                case Array array:
                    return string.Join( ", ", ((object[]) array).Select( i => this.Format( "", i, formatProvider ) ) );

                default:
                    {
                        if ( arg != null )
                        {
                            return arg.ToString();
                        }

                        return string.Empty;
                    }
            }
        }
    }
}