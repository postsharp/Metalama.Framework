// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Fabrics;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Metalama.Framework.Engine.Utilities.UserCode;

/// <summary>
/// Encapsulates an executable member of user code, which can be given as a reflection <see cref="MemberInfo"/>,
/// as a delegate, as an expression or as a symbol. This struct encapsulates the logic required to print error messages.
/// </summary>
internal readonly struct UserCodeMemberInfo : IFormattable
{
    private readonly object? _underlying;

    private UserCodeMemberInfo( object? underlying )
    {
        this._underlying = underlying;
    }

    public static UserCodeMemberInfo FromMemberInfo( MemberInfo member ) => new( member );

    public static UserCodeMemberInfo FromDelegate( Delegate member ) => new( member.Method );

    public static UserCodeMemberInfo FromSymbol( ISymbol? member ) => new( member );

    public Location? GetDiagnosticLocation()
        => this._underlying switch
        {
            Expression => null,
            ISymbol symbol => symbol.GetLocationForDiagnostic(),
            MemberInfo => null,
            null => null,
            _ => throw new AssertionFailedException( $"Unexpected underlying object {this._underlying.GetType()}." )
        };

    private static string GetTypeName( Type t ) => typeof(Fabric).IsAssignableFrom( t ) ? t.FullName! : t.Name;

    public override string ToString() => this.ToString( null, CultureInfo.InvariantCulture );

    public string ToString( string? format, IFormatProvider? formatProvider )
        => this._underlying switch
        {
            Expression expression => expression.ToString(),
            ISymbol symbol => MetalamaStringFormatter.Instance.Format( "", symbol, formatProvider ),
            MemberInfo member =>
                GetTypeName( member.DeclaringType! ) + "." + member.Name,
            null => "",
            _ => throw new AssertionFailedException( $"Unexpected underlying object {this._underlying.GetType()}." )
        };
}