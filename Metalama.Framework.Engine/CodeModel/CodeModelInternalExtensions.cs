﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal static class CodeModelInternalExtensions
    {
        public static CompilationModel GetCompilationModel( this ICompilationElement declaration ) => (CompilationModel) declaration.Compilation;

        // Resharper disable UnusedMember.Global
        [Obsolete( "Redundant call" )]
        public static CompilationModel GetCompilationModel( this CompilationModel compilation ) => compilation;

        public static AttributeData GetAttributeData( this IAttribute attribute )
        {
            if ( attribute is Attribute attributeModel )
            {
                return attributeModel.AttributeData;
            }

            throw new ArgumentOutOfRangeException( nameof(attribute), "This is not a source attribute." );
        }

        public static bool IsAccessor( this IMethod method )
            => method.MethodKind switch
            {
                MethodKind.PropertyGet => true,
                MethodKind.PropertySet => true,
                MethodKind.EventAdd => true,
                MethodKind.EventRemove => true,
                MethodKind.EventRaise => true,
                _ => false
            };

        public static InsertPosition ToInsertPosition( this IDeclaration declaration )
        {
            switch ( declaration )
            {
                case BuiltDeclaration builtDeclaration:
                    return builtDeclaration.Builder.ToInsertPosition();

                // TODO: This is a hack (since splitting transformations and builders).
                // If not treated as a special case, the promoted field will be inserted into a wrong place and possibly into a wrong syntax tree.
                case PromotedField promotedField:
                    return promotedField.Field.ToInsertPosition();

                case IMemberOrNamedTypeBuilder { DeclaringType: { } declaringType }:
                    return new InsertPosition(
                        InsertPositionRelation.Within,
                        (MemberDeclarationSyntax) declaringType.GetPrimaryDeclarationSyntax().AssertNotNull() );

                case SymbolBasedDeclaration baseDeclaration:
                    var symbol = baseDeclaration.Symbol;
                    var primaryDeclaration = symbol.GetPrimaryDeclaration();

                    if ( primaryDeclaration != null )
                    {
                        var memberDeclaration = primaryDeclaration.FindMemberDeclaration();

                        if ( memberDeclaration is BaseTypeDeclarationSyntax )
                        {
                            return new InsertPosition( InsertPositionRelation.Within, memberDeclaration );
                        }
                        else
                        {
                            return new InsertPosition( InsertPositionRelation.After, memberDeclaration );
                        }
                    }
                    else
                    {
                        var primaryTypeDeclaration = symbol.ContainingType.GetPrimaryDeclaration().AssertNotNull();

                        return new InsertPosition( InsertPositionRelation.Within, primaryTypeDeclaration.FindMemberDeclaration() );
                    }

                default:
                    throw new AssertionFailedException( $"Unexpected declaration: '{declaration}'." );
            }
        }

        internal static SyntaxToken GetCleanName( this IMember member )
        {
            return
                SyntaxFactory.Identifier(
                    member.IsExplicitInterfaceImplementation
                        ? member.Name.Split( '.' ).Last()
                        : member.Name );
        }
    }
}