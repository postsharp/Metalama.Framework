// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Linq;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.Diagnostics
{
    /// <summary>
    /// Formats arguments passed to a diagnostic.
    /// </summary>
    internal sealed class UserMessageFormatter : CultureInfo, ICustomFormatter
    {
        public static readonly UserMessageFormatter Instance = new();

        private UserMessageFormatter() : base( InvariantCulture.Name ) { }

        public override object? GetFormat( Type formatType ) => formatType == typeof(ICustomFormatter) ? this : base.GetFormat( formatType );

        public static string Format( FormattableString message ) => message.ToString( Instance );

        public string Format( string format, object? arg, IFormatProvider formatProvider )
        {
            try
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
                            return displayable.ToString();
                        }

                    case DeclarationKind declarationKind:
                        switch ( declarationKind )
                        {
                            case DeclarationKind.TypeParameter:
                                return "generic parameter";

                            case DeclarationKind.ManagedResource:
                                return "managed resource";

                            case DeclarationKind.AssemblyReference:
                                return "assembly reference";

                            case DeclarationKind.NamedType:
                                return "type";

                            default:
                                return declarationKind.ToString().ToLowerInvariant();
                        }

                    case Accessibility accessibility:
                        switch ( accessibility )
                        {
                            case Accessibility.ProtectedInternal:
                                return "protected internal";

                            case Accessibility.PrivateProtected:
                                return "private protected";

                            default:
                                return accessibility.ToString().ToLowerInvariant();
                        }

                    case ISymbol symbol:
                        return symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat );

                    case IFormattable formattable:
                        return formattable.ToString( format, this );

                    case string[] strings:
                        return string.Join( ", ", strings.Select( s => s == null ? null : "'" + s + "'" ) );

                    case Array array:
                        return string.Join( ", ", array.Cast<object>().Select( i => this.Format( "", i, formatProvider ) ) );
                }
            }
            catch
            {
                // Fall back.
            }

            try
            {
                return arg != null ? arg.ToString() : string.Empty;
            }
            catch
            {
                return arg!.GetType().Name;
            }
        }
    }
}