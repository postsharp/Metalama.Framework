// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal static class CodeModelInternalExtensions
    {
        public static CompilationModel GetCompilationModel( this ICompilationElement declaration ) => (CompilationModel) declaration.Compilation;

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

        public static SyntaxNode? GetPrimaryDeclarationSyntax( this IDeclaration declaration )
        {
            return declaration.GetSymbol()?.GetPrimaryDeclaration();
        }

        public static SyntaxTree? GetPrimarySyntaxTree( this IDeclaration declaration )
            => declaration switch
            {
                IDeclarationImpl declarationImpl => declarationImpl.PrimarySyntaxTree,
                _ => throw new AssertionFailedException()
            };

        public static InsertPosition ToInsertPosition( this IDeclaration declaration )
        {
            switch ( declaration )
            {
                case IReplaceMemberTransformation { ReplacedMember: var replacedMember } when !replacedMember.IsDefault:
                    return replacedMember.GetTarget( declaration.Compilation, ReferenceResolutionOptions.DoNotFollowRedirections ).ToInsertPosition();

                case BuiltDeclaration builtDeclaration:
                    return builtDeclaration.Builder.ToInsertPosition();

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
                        var primaryTypeDeclaration = symbol.ContainingType.GetPrimaryDeclaration();

                        return new InsertPosition( InsertPositionRelation.Within, primaryTypeDeclaration.FindMemberDeclaration() );
                    }

                default:
                    throw new AssertionFailedException();
            }
        }
    }
}