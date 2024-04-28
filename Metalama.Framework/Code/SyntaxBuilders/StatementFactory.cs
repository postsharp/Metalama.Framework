// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Code.SyntaxBuilders;

/// <summary>
/// Creates instances of the <see cref="IStatement"/> interface.
/// </summary>
[CompileTime]
public static class StatementFactory
{
    /// <summary>
    /// Parses a string containing a C# statement and returns an <see cref="IStatement"/>, which can be inserted into the run-time code
    /// using <see cref="meta.InsertStatement(Metalama.Framework.Code.SyntaxBuilders.IStatement)"/>. The string must contain a single statement,
    /// and must be finished by a semicolon or a closing bracket. An alternative to this method is the <see cref="StatementBuilder"/> class.
    /// </summary>
    /// <seealso href="@templates"/>
    public static IStatement Parse( string code ) => SyntaxBuilder.CurrentImplementation.ParseStatement( code );

    public static IStatement FromExpression( IExpression expression ) => SyntaxBuilder.CurrentImplementation.CreateExpressionStatement( expression );

    public static IStatement FromTemplate( TemplateInvocation templateInvocation, string? args = null )
        => SyntaxBuilder.CurrentImplementation.CreateTemplateStatement( templateInvocation, args );
}