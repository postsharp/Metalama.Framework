﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Engine.Utilities
{
    public static class SyntaxExtensions
    {
        public static MemberDeclarationSyntax FindMemberDeclaration( this SyntaxNode? node )
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

            throw new AssertionFailedException();
        }

        public static bool IsAutoPropertyDeclaration( this PropertyDeclarationSyntax propertyDeclaration )
            => propertyDeclaration.ExpressionBody == null
               && propertyDeclaration.AccessorList?.Accessors.All( x => x.Body == null && x.ExpressionBody == null ) == true
               && propertyDeclaration.Modifiers.All( x => x.Kind() != SyntaxKind.AbstractKeyword );
    }
}