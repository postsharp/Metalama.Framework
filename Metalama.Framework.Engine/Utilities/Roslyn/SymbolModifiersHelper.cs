// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    internal static class SymbolModifiersHelper
    {
        public static SyntaxTokenList GetSyntaxModifierList( this ISymbol declaration, ModifierCategories categories = ModifierCategories.All )
        {
            switch ( declaration )
            {
                case IMethodSymbol accessor when accessor.IsAccessor():
                    return GetAccessorSyntaxModifierList( accessor, categories );

                case IMethodSymbol method:
                    return GetMemberSyntaxModifierList( method, categories );

                case IPropertySymbol property:
                    return GetMemberSyntaxModifierList( property, categories );

                case IEventSymbol @event:
                    return GetMemberSyntaxModifierList( @event, categories );

                case IParameterSymbol parameter:
                    return GetParameterSyntaxModifierList( parameter );

                case IFieldSymbol field:
                    return GetMemberSyntaxModifierList( field, categories );

                default:
                    throw new AssertionFailedException( $"Unexpected declaration: '{declaration}'." );
            }
        }

        private static SyntaxTokenList GetAccessorSyntaxModifierList( IMethodSymbol accessor, ModifierCategories categories )
        {
            var methodGroup = accessor.AssociatedSymbol!;

            // TODO: Unify with ToRoslynAccessibility and some roslyn helper?
            var tokens = new List<SyntaxToken>();

            if ( (categories & ModifierCategories.Accessibility) != 0 )
            {
                if ( accessor.DeclaredAccessibility != methodGroup.DeclaredAccessibility )
                {
                    AddAccessibilityTokens( accessor, tokens );
                }
            }

            return TokenList( tokens );
        }

        private static SyntaxTokenList GetMemberSyntaxModifierList( ISymbol member, ModifierCategories categories )
        {
            // TODO: Unify with ToRoslynAccessibility and some roslyn helper?
            var tokens = new List<SyntaxToken>();

            if ( (categories & ModifierCategories.Accessibility) != 0 )
            {
                AddAccessibilityTokens( member, tokens );
            }

            if ( member.IsStatic && (categories & ModifierCategories.Static) != 0 )
            {
                tokens.Add( Token( SyntaxKind.StaticKeyword ).WithTrailingTrivia( Space ) );
            }

            if ( (categories & ModifierCategories.Inheritance) != 0 )
            {
                if ( member.HasModifier( SyntaxKind.NewKeyword ) )
                {
                    tokens.Add( Token( SyntaxKind.NewKeyword ).WithTrailingTrivia( Space ) );
                }

                // The following modifiers are exclusive in C# but not in the symbol model.
                if ( member.IsOverride )
                {
                    tokens.Add( Token( SyntaxKind.OverrideKeyword ).WithTrailingTrivia( Space ) );
                }
                else if ( member.IsAbstract )
                {
                    tokens.Add( Token( SyntaxKind.AbstractKeyword ).WithTrailingTrivia( Space ) );
                }
                else if ( member.IsVirtual )
                {
                    tokens.Add( Token( SyntaxKind.VirtualKeyword ).WithTrailingTrivia( Space ) );
                }

                if ( member.IsSealed )
                {
                    tokens.Add( Token( SyntaxKind.SealedKeyword ).WithTrailingTrivia( Space ) );
                }
            }

            if ( (categories & ModifierCategories.ReadOnly) != 0 && member is IMethodSymbol { IsReadOnly: true } or IFieldSymbol { IsReadOnly: true } )
            {
                tokens.Add( Token( SyntaxKind.ReadOnlyKeyword ).WithTrailingTrivia( Space ) );
            }

            if ( (categories & ModifierCategories.Const) != 0 && member is IFieldSymbol { IsConst: true } )
            {
                tokens.Add( Token( SyntaxKind.ConstKeyword ).WithTrailingTrivia( Space ) );
            }

            if ( (categories & ModifierCategories.Unsafe) != 0 && member.HasModifier( SyntaxKind.UnsafeKeyword ) )
            {
                tokens.Add( Token( SyntaxKind.UnsafeKeyword ).WithTrailingTrivia( Space ) );
            }

            if ( (categories & ModifierCategories.Volatile) != 0 && member is IFieldSymbol { IsVolatile: true } )
            {
                tokens.Add( Token( SyntaxKind.VolatileKeyword ).WithTrailingTrivia( Space ) );
            }

            if ( (categories & ModifierCategories.Async) != 0 && member is IMethodSymbol { IsAsync: true } )
            {
                tokens.Add( Token( SyntaxKind.AsyncKeyword ).WithTrailingTrivia( Space ) );
            }

            return TokenList( tokens );
        }

        private static void AddAccessibilityTokens( ISymbol member, List<SyntaxToken> tokens )
        {
            // If the target is explicit interface implementation, skip accessibility modifiers.
            switch ( member )
            {
                case IMethodSymbol method:
                    if ( method.ExplicitInterfaceImplementations.Length > 0 )
                    {
                        return;
                    }

                    break;

                case IPropertySymbol property:
                    if ( property.ExplicitInterfaceImplementations.Length > 0 )
                    {
                        return;
                    }

                    break;

                case IEventSymbol @event:
                    if ( @event.ExplicitInterfaceImplementations.Length > 0 )
                    {
                        return;
                    }

                    break;
            }

            switch ( member.DeclaredAccessibility )
            {
                case Accessibility.Private:
                    tokens.Add( Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ) );

                    break;

                case Accessibility.ProtectedAndInternal:
                    tokens.Add( Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ) );
                    tokens.Add( Token( SyntaxKind.ProtectedKeyword ).WithTrailingTrivia( Space ) );

                    break;

                case Accessibility.Protected:
                    tokens.Add( Token( SyntaxKind.ProtectedKeyword ).WithTrailingTrivia( Space ) );

                    break;

                case Accessibility.Internal:
                    tokens.Add( Token( SyntaxKind.InternalKeyword ).WithTrailingTrivia( Space ) );

                    break;

                case Accessibility.ProtectedOrInternal:
                    tokens.Add( Token( SyntaxKind.ProtectedKeyword ).WithTrailingTrivia( Space ) );
                    tokens.Add( Token( SyntaxKind.InternalKeyword ).WithTrailingTrivia( Space ) );

                    break;

                case Accessibility.Public:
                    tokens.Add( Token( SyntaxKind.PublicKeyword ).WithTrailingTrivia( Space ) );

                    break;
            }
        }

        private static SyntaxTokenList GetParameterSyntaxModifierList( IParameterSymbol parameter )
        {
            var tokens = new List<SyntaxToken>();

            if ( parameter.RefKind == RefKind.In )
            {
                tokens.Add( Token( SyntaxKind.InKeyword ).WithTrailingTrivia( Space ) );
            }

            if ( parameter.RefKind == RefKind.Ref )
            {
                tokens.Add( Token( SyntaxKind.RefKeyword ).WithTrailingTrivia( Space ) );
            }

            if ( parameter.RefKind == RefKind.Out )
            {
                tokens.Add( Token( SyntaxKind.OutKeyword ).WithTrailingTrivia( Space ) );
            }

            return TokenList( tokens );
        }
    }
}