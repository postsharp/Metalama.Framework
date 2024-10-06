// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.Built;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using Attribute = Metalama.Framework.Engine.CodeModel.Source.Attribute;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Helpers;

internal static class CodeModelInternalExtensions
{
    public static CompilationModel GetCompilationModel( this ICompilationElement declaration ) => (CompilationModel) declaration.Compilation;

    [Obsolete( "Use the Compilation property." )]
    public static CompilationModel GetCompilationModel( this ICompilationElementImpl declaration ) => declaration.Compilation;

    public static CompilationContext GetCompilationContext( this ICompilationElementImpl declaration ) => declaration.Compilation.CompilationContext;

    public static CompilationContext GetCompilationContext( this ICompilationElement declaration ) => declaration.GetCompilationModel().CompilationContext;

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

    public static InsertPosition ToInsertPosition( this DeclarationBuilderData declaration )
    {
        switch ( declaration )
        {
            // TODO: This is a hack (since splitting transformations and builders).
            // If not treated as a special case, the promoted field will be inserted into a wrong place and possibly into a wrong syntax tree.
            //case PromotedField promotedField:
            //   return promotedField.OriginalSourceFieldOrFieldBuilder.ToInsertPosition();

            case NamedTypeBuilderData { DeclaringType: IDeclarationBuilderDataRef { BuilderData: NamedDeclarationBuilderData named } }:
                return new InsertPosition( InsertPositionRelation.Within, named );

            case NamedTypeBuilderData { DeclaringType: ISymbolRef declaringType }:
                return new InsertPosition(
                    InsertPositionRelation.Within,
                    (MemberDeclarationSyntax) declaringType.Symbol.GetPrimaryDeclaration().AssertNotNull() );

            case NamedTypeBuilderData topLevelType:
                return new InsertPosition( topLevelType.PrimarySyntaxTree );

            case MemberBuilderData { DeclaringType: IDeclarationBuilderDataRef { BuilderData: NamedDeclarationBuilderData named } }:
                return new InsertPosition( InsertPositionRelation.Within, named );

            case MemberBuilderData { DeclaringType: ISymbolRef declaringType }:
                return new InsertPosition(
                    InsertPositionRelation.Within,
                    (MemberDeclarationSyntax) declaringType.Symbol.GetPrimaryDeclaration().AssertNotNull() );

            default:
                throw new AssertionFailedException( $"Unexpected declaration: '{declaration}'." );
        }
    }

    public static InsertPosition ToInsertPosition( this IRef declaration )
        => declaration switch
        {
            ISymbolRef symbolRef => symbolRef.Symbol.ToInsertPosition(),
            IDeclarationBuilderDataRef builderDataRef => builderDataRef.BuilderData.ToInsertPosition(),
            _ => throw new AssertionFailedException()
        };

    public static InsertPosition ToInsertPosition( this ISymbol symbol )
    {
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
    }

    public static InsertPosition ToInsertPosition( this IDeclaration declaration )
    {
        using ( StackOverflowHelper.Detect() )
        {
            switch ( declaration )
            {
                case BuiltDeclaration builtDeclaration:
                    return builtDeclaration.BuilderData.ToInsertPosition();

                // TODO: This is a hack (since splitting transformations and builders).
                // If not treated as a special case, the promoted field will be inserted into a wrong place and possibly into a wrong syntax tree.
                //  case PromotedField promotedField:
                //    return promotedField.OriginalSourceFieldOrFieldBuilder.ToInsertPosition();

                case SymbolBasedDeclaration baseDeclaration:
                    return baseDeclaration.Symbol.ToInsertPosition();

                default:
                    throw new AssertionFailedException( $"Unexpected declaration: '{declaration}'." );
            }
        }
    }

    internal static SyntaxToken GetCleanName( this IMember member )
        => SyntaxFactory.Identifier(
            member.IsExplicitInterfaceImplementation
                ? member.Name.Split( '.' ).Last()
                : member.Name );
}