﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl
{
    public static class SyntaxExtensions
    {
        public static MemberDeclarationSyntax? GetMemberDeclarationSyntax( this SyntaxNode? node )
        {
            var current = node;

            while ( current != null )
            {
                if ( current is MemberDeclarationSyntax memberDeclaration )
                {
                    return memberDeclaration;
                }

                current = current.Parent;
            }

            return null;
        }
    }
}