// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Accessibility = Microsoft.CodeAnalysis.Accessibility;
using RefKind = Microsoft.CodeAnalysis.RefKind;

namespace Caravela.Framework.Impl.Utilities
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
            }

            throw new AssertionFailedException();
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

            if ( member.IsStatic && (categories & ModifierCategories.Inheritance) != 0 )
            {
                tokens.Add( Token( SyntaxKind.StaticKeyword ) );
            }

            if ( (categories & ModifierCategories.Inheritance) != 0 )
            {
                if ( member.HasModifier( SyntaxKind.NewKeyword ) )
                {
                    tokens.Add( Token( SyntaxKind.NewKeyword ) );
                }

                if ( member.IsAbstract )
                {
                    tokens.Add( Token( SyntaxKind.AbstractKeyword ) );
                }

                if ( member.IsVirtual )
                {
                    tokens.Add( Token( SyntaxKind.VirtualKeyword ) );
                }

                if ( member.IsOverride )
                {
                    tokens.Add( Token( SyntaxKind.OverrideKeyword ) );
                }

                if ( member.IsSealed )
                {
                    tokens.Add( Token( SyntaxKind.SealedKeyword ) );
                }
            }

            if ( (categories & ModifierCategories.ReadOnly) != 0 && member is IMethodSymbol { IsReadOnly: true } or IFieldSymbol { IsReadOnly: true } )
            {
                tokens.Add( Token( SyntaxKind.ReadOnlyKeyword ) );
            }

            if ( (categories & ModifierCategories.Unsafe) != 0 && member.HasModifier( SyntaxKind.UnsafeKeyword ) )
            {
                tokens.Add( Token( SyntaxKind.UnsafeKeyword ) );
            }

            if ( (categories & ModifierCategories.Volatile) != 0 && member is IFieldSymbol { IsVolatile: true } )
            {
                tokens.Add( Token( SyntaxKind.VolatileKeyword ) );
            }

            if ( (categories & ModifierCategories.Async) != 0 && member is IMethodSymbol { IsAsync: true } )
            {
                tokens.Add( Token( SyntaxKind.AsyncKeyword ) );
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
                    tokens.Add( Token( SyntaxKind.PrivateKeyword ) );

                    break;

                case Accessibility.ProtectedAndInternal:
                    tokens.Add( Token( SyntaxKind.PrivateKeyword ) );
                    tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );

                    break;

                case Accessibility.Protected:
                    tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );

                    break;

                case Accessibility.Internal:
                    tokens.Add( Token( SyntaxKind.InternalKeyword ) );

                    break;

                case Accessibility.ProtectedOrInternal:
                    tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );
                    tokens.Add( Token( SyntaxKind.InternalKeyword ) );

                    break;

                case Accessibility.Public:
                    tokens.Add( Token( SyntaxKind.PublicKeyword ) );

                    break;
            }
        }

        private static SyntaxTokenList GetParameterSyntaxModifierList( IParameterSymbol parameter )
        {
            var tokens = new List<SyntaxToken>();

            if ( parameter.RefKind == RefKind.In )
            {
                tokens.Add( Token( SyntaxKind.InKeyword ) );
            }

            if ( parameter.RefKind == RefKind.Ref )
            {
                tokens.Add( Token( SyntaxKind.RefKeyword ) );
            }

            if ( parameter.RefKind == RefKind.Out )
            {
                tokens.Add( Token( SyntaxKind.OutKeyword ) );
            }

            return TokenList( tokens );
        }

    }
}