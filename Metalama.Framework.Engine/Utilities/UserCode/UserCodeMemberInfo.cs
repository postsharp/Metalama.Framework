// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Fabrics;
using Microsoft.CodeAnalysis;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Metalama.Framework.Engine.Utilities
{
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

        public static UserCodeMemberInfo FromExpression( Expression expression ) => new( expression );

        public Location? GetDiagnosticLocation()
            => this._underlying switch
            {
                Expression => null,
                ISymbol symbol => symbol.GetDiagnosticLocation(),
                MemberInfo => null,
                null => null,
                _ => throw new AssertionFailedException()
            };

        private static string GetTypeName( Type t ) => typeof(Fabric).IsAssignableFrom( t ) ? t.FullName! : t.Name;

        public string ToString( string format, IFormatProvider formatProvider )
            => this._underlying switch
            {
                Expression expression => expression.ToString(),
                ISymbol symbol => UserMessageFormatter.Instance.Format( "", symbol, formatProvider ),
                MemberInfo member =>
                    GetTypeName( member.DeclaringType! ) + "." + member.Name,
                null => "",
                _ => throw new AssertionFailedException()
            };
    }
}