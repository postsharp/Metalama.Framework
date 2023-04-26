// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Accessibility = Metalama.Framework.Code.Accessibility;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal static class DeclarationModifiersHelper
    {
        public static SyntaxTokenList GetSyntaxModifierList( this IDeclaration declaration, ModifierCategories categories = ModifierCategories.All )
        {
            switch ( declaration )
            {
                case IMethod accessor when accessor.IsAccessor():
                    return GetAccessorSyntaxModifierList( accessor, categories );

                case IMethod method:
                    return GetMemberSyntaxModifierList( method, categories );

                case IProperty property:
                    return GetMemberSyntaxModifierList( property, categories );

                case IIndexer indexer:
                    return GetMemberSyntaxModifierList( indexer, categories );

                case IEvent @event:
                    return GetMemberSyntaxModifierList( @event, categories );

                case IParameter parameter:
                    return GetParameterSyntaxModifierList( parameter );

                case IField field:
                    return GetMemberSyntaxModifierList( field, categories );

                default:
                    throw new AssertionFailedException( $"Unexpected declaration kind: {declaration.DeclarationKind}." );
            }
        }

        private static SyntaxTokenList GetAccessorSyntaxModifierList( IMethod accessor, ModifierCategories categories )
        {
            var methodGroup = (IMemberOrNamedType) accessor.ContainingDeclaration!;

            // TODO: Unify with ToRoslynAccessibility and some roslyn helper?
            var tokens = new List<SyntaxToken>();

            if ( (categories & ModifierCategories.Accessibility) != 0 )
            {
                if ( accessor.Accessibility != methodGroup.Accessibility )
                {
                    AddAccessibilityTokens( accessor, tokens );
                }
            }

            return TokenList( tokens );
        }

        private static SyntaxTokenList GetMemberSyntaxModifierList( IMember member, ModifierCategories categories )
        {
            // TODO: Unify with ToRoslynAccessibility and some roslyn helper?
            var tokens = new List<SyntaxToken>();

            void AddToken( SyntaxKind syntaxKind )
            {
                tokens.Add( Token( syntaxKind ).WithTrailingTrivia( Space ) );
            }

            if ( (categories & ModifierCategories.Accessibility) != 0 )
            {
                AddAccessibilityTokens( member, tokens );
            }

#if ROSLYN_4_4_0_OR_GREATER
            if ( (categories & ModifierCategories.Required) != 0 && member is IFieldOrProperty { IsRequired: true } )
            {
                AddToken( SyntaxKind.RequiredKeyword );
            }
#endif

            if ( member.IsStatic && (categories & ModifierCategories.Static) != 0 )
            {
                AddToken( SyntaxKind.StaticKeyword );
            }

            if ( (categories & ModifierCategories.Inheritance) != 0 )
            {
                if ( member.IsNew )
                {
                    AddToken( SyntaxKind.NewKeyword );
                }

                if ( member.IsOverride )
                {
                    AddToken( SyntaxKind.OverrideKeyword );
                }
                else if ( member.IsAbstract )
                {
                    AddToken( SyntaxKind.AbstractKeyword );
                }
                else if ( member.IsVirtual )
                {
                    AddToken( SyntaxKind.VirtualKeyword );
                }

                if ( member.IsSealed )
                {
                    AddToken( SyntaxKind.SealedKeyword );
                }
            }

            if ( (categories & ModifierCategories.ReadOnly) != 0 && member is IMethod { IsReadOnly: true } or
                    IField { Writeability: Writeability.ConstructorOnly } )
            {
                AddToken( SyntaxKind.ReadOnlyKeyword );
            }

            if ( (categories & ModifierCategories.Const) != 0 && member is IField { Writeability: Writeability.None } )
            {
                AddToken( SyntaxKind.ConstKeyword );
            }

            if ( (categories & ModifierCategories.Unsafe) != 0 && member.GetSymbol() is { } symbol && symbol.HasModifier( SyntaxKind.UnsafeKeyword ) )
            {
                AddToken( SyntaxKind.UnsafeKeyword );
            }

            if ( (categories & ModifierCategories.Volatile) != 0 && member.GetSymbol() is IFieldSymbol { IsVolatile: true } )
            {
                AddToken( SyntaxKind.VolatileKeyword );
            }

            if ( (categories & ModifierCategories.Async) != 0 && member.IsAsync )
            {
                AddToken( SyntaxKind.AsyncKeyword );
            }

            return TokenList( tokens );
        }

        private static void AddAccessibilityTokens( IMemberOrNamedType member, List<SyntaxToken> tokens )
        {
            void AddToken( SyntaxKind syntaxKind )
            {
                tokens.Add( Token( syntaxKind ).WithTrailingTrivia( Space ) );
            }

            // If the target is explicit interface implementation, skip accessibility modifiers.
            switch ( member )
            {
                case IMethod method:
                    if ( method.ExplicitInterfaceImplementations.Count > 0 )
                    {
                        return;
                    }

                    break;

                case IProperty property:
                    if ( property.ExplicitInterfaceImplementations.Count > 0 )
                    {
                        return;
                    }

                    break;

                case IEvent @event:
                    if ( @event.ExplicitInterfaceImplementations.Count > 0 )
                    {
                        return;
                    }

                    break;
            }

            switch ( member.Accessibility )
            {
                case Accessibility.Private:
                    AddToken( SyntaxKind.PrivateKeyword );

                    break;

                case Accessibility.PrivateProtected:
                    AddToken( SyntaxKind.PrivateKeyword );
                    AddToken( SyntaxKind.ProtectedKeyword );

                    break;

                case Accessibility.Protected:
                    AddToken( SyntaxKind.ProtectedKeyword );

                    break;

                case Accessibility.Internal:
                    AddToken( SyntaxKind.InternalKeyword );

                    break;

                case Accessibility.ProtectedInternal:
                    AddToken( SyntaxKind.ProtectedKeyword );
                    AddToken( SyntaxKind.InternalKeyword );

                    break;

                case Accessibility.Public:
                    AddToken( SyntaxKind.PublicKeyword );

                    break;
            }
        }

        private static SyntaxTokenList GetParameterSyntaxModifierList( IParameter parameter )
        {
            var tokens = new List<SyntaxToken>();

            void AddToken( SyntaxKind syntaxKind )
            {
                tokens.Add( Token( syntaxKind ).WithTrailingTrivia( Space ) );
            }

            if ( parameter.RefKind == RefKind.In )
            {
                AddToken( SyntaxKind.InKeyword );
            }

            if ( parameter.RefKind == RefKind.Ref )
            {
                AddToken( SyntaxKind.RefKeyword );
            }

            if ( parameter.RefKind == RefKind.Out )
            {
                AddToken( SyntaxKind.OutKeyword );
            }

            return TokenList( tokens );
        }
    }
}