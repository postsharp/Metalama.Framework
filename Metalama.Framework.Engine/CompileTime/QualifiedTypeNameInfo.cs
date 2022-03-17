// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// Encapsulates both a <see cref="QualifiedName"/> and a <see cref="ShortName"/>.
/// </summary>
internal readonly struct QualifiedTypeNameInfo
{
    public NameSyntax QualifiedName { get; }

    public SyntaxToken ShortName { get; }

    public QualifiedTypeNameInfo( NameSyntax name, string shortName )
    {
        this.QualifiedName = name;
        this.ShortName = SyntaxFactory.Identifier( shortName );
    }

    public QualifiedTypeNameInfo( NameSyntax name )
    {
        this.QualifiedName = name;

        this.ShortName = name switch
        {
            AliasQualifiedNameSyntax aliasQualifiedNameSyntax => aliasQualifiedNameSyntax.Name.Identifier,
            QualifiedNameSyntax qualifiedNameSyntax => qualifiedNameSyntax.Right.Identifier,
            SimpleNameSyntax simpleNameSyntax => simpleNameSyntax.Identifier,
            _ => throw new AssertionFailedException()
        };
    }
}