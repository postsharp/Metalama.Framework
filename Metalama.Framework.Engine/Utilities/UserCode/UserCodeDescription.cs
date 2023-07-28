// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Fabrics;
using Microsoft.CodeAnalysis;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Metalama.Framework.Engine.Utilities.UserCode;

/// <summary>
/// Encapsulates a description of member of user code that can throw an exception.
/// The point of this type is to avoid formatting the string when there is no exception but to carry on sufficient information to
/// create this string if needed.
/// </summary>
internal readonly struct UserCodeDescription : IFormattable
{
    private readonly object? _arg1;
    private readonly object? _arg2;
    private readonly string _formattingString;

    private UserCodeDescription( string formattingString, object? arg1, object? arg2 )
    {
        this._formattingString = formattingString;
        this._arg1 = arg1;
        this._arg2 = arg2;
    }

    public static UserCodeDescription Create( string description ) => new( description, null, null );

    public static UserCodeDescription Create( string formattingString, object? arg1 ) => new( formattingString, arg1, null );

    public static UserCodeDescription Create( string formattingString, object? arg1, object? arg2 ) => new( formattingString, arg1, arg2 );

    private static string GetTypeName( Type t ) => typeof(Fabric).IsAssignableFrom( t ) ? t.FullName! : t.Name;

    public override string ToString() => this.ToString( null, MetalamaStringFormatter.Instance );

    private static string FormatArg( object? arg, IFormatProvider formatProvider )
        => arg switch
        {
            IDiagnosticSource diagnosticSource => diagnosticSource.DiagnosticSourceDescription,
            Expression expression => expression.ToString(),
            ISymbol symbol => MetalamaStringFormatter.Instance.Format( "", symbol, formatProvider ),
            Type type => type.Name,
            MemberInfo member =>
                GetTypeName( member.DeclaringType! ) + "." + member.Name,
            string s => s,
            null => "<null>",
            _ => arg.ToString() ?? arg.GetType().Name
        };

    public string ToString( string? format, IFormatProvider? formatProvider )
        => string.Format(
            formatProvider ?? MetalamaStringFormatter.Instance,
            this._formattingString,
            FormatArg( this._arg1, formatProvider ?? MetalamaStringFormatter.Instance ),
            FormatArg( this._arg2, formatProvider ?? MetalamaStringFormatter.Instance ) );
}