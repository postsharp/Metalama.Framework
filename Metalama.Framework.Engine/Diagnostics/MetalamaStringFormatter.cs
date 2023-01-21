// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Utilities.Roslyn;
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
    public sealed class MetalamaStringFormatter : CultureInfo, ICustomFormatter
    {
        internal static readonly MetalamaStringFormatter Instance = new();

        private MetalamaStringFormatter() : base( InvariantCulture.Name ) { }

        public override object? GetFormat( Type? formatType ) => formatType == typeof(ICustomFormatter) ? this : base.GetFormat( formatType );

        public static string Format( FormattableString message ) => message.ToString( Instance );

        public string Format( string? format, object? arg, IFormatProvider? formatProvider )
        {
            try
            {
                switch ( arg )
                {
                    case null:
                        return "";

                    case TemplatingScope templatingScope:
                        return templatingScope.ToDisplayString();

                    case ExecutionScope executionScope:
                        switch ( executionScope )
                        {
                            case ExecutionScope.RunTime:
                                return TemplatingScope.RunTimeOnly.ToDisplayString();

                            case ExecutionScope.CompileTime:
                                return TemplatingScope.CompileTimeOnly.ToDisplayString();

                            case ExecutionScope.RunTimeOrCompileTime:
                                return TemplatingScope.RunTimeOrCompileTime.ToDisplayString();

                            default:
                                return executionScope.ToString();
                        }

                    case IDisplayable displayable:
                        try
                        {
                            return displayable.ToDisplayString( CodeDisplayFormat.ShortDiagnosticMessage );
                        }
                        catch
                        {
                            return displayable.ToString() ?? "";
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

                            case DeclarationKind.Compilation:
                                return "project";

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

                    case SymbolKind symbolKind:
                        return symbolKind.ToDisplayName();

                    case ISymbol symbol:
                        return symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat );

                    case IFormattable formattable:
                        return formattable.ToString( format, this );

                    case Type type:
                        return type.Name;

                    case string?[] strings:
                        return string.Join( ", ", strings.SelectAsEnumerable( s => s == null ? null : "'" + s + "'" ) );

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
                return arg != null ? arg.ToString() ?? string.Empty : string.Empty;
            }
            catch
            {
                return arg!.GetType().Name;
            }
        }
    }
}