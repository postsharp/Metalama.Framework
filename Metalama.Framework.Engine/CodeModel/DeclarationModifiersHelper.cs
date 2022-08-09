// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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

                case IEvent @event:
                    return GetMemberSyntaxModifierList( @event, categories );

                case IParameter parameter:
                    return GetParameterSyntaxModifierList( parameter );

                case IField field:
                    return GetMemberSyntaxModifierList( field, categories );
            }

            throw new AssertionFailedException();
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

            if ( (categories & ModifierCategories.Accessibility) != 0 )
            {
                AddAccessibilityTokens( member, tokens );
            }

            if ( member.IsStatic && (categories & ModifierCategories.Static) != 0 )
            {
                tokens.Add( Token( SyntaxKind.StaticKeyword ) );
            }

            if ( (categories & ModifierCategories.Inheritance) != 0 )
            {
                if ( member.IsNew )
                {
                    tokens.Add( Token( SyntaxKind.NewKeyword ) );
                }

                if ( member.IsOverride )
                {
                    tokens.Add( Token( SyntaxKind.OverrideKeyword ) );
                }
                else if ( member.IsAbstract )
                {
                    tokens.Add( Token( SyntaxKind.AbstractKeyword ) );
                }
                else if ( member.IsVirtual )
                {
                    tokens.Add( Token( SyntaxKind.VirtualKeyword ) );
                }

                if ( member.IsSealed )
                {
                    tokens.Add( Token( SyntaxKind.SealedKeyword ) );
                }
            }

            if ( (categories & ModifierCategories.ReadOnly) != 0 && member is IMethod { IsReadOnly: true } or
                    IField { Writeability: Writeability.ConstructorOnly } )
            {
                tokens.Add( Token( SyntaxKind.ReadOnlyKeyword ) );
            }

            if ( (categories & ModifierCategories.Unsafe) != 0 && member.GetSymbol() is { } symbol && symbol.HasModifier( SyntaxKind.UnsafeKeyword ) )
            {
                tokens.Add( Token( SyntaxKind.UnsafeKeyword ) );
            }

            if ( (categories & ModifierCategories.Volatile) != 0 && member.GetSymbol() is IFieldSymbol { IsVolatile: true } )
            {
                tokens.Add( Token( SyntaxKind.VolatileKeyword ) );
            }

            if ( (categories & ModifierCategories.Async) != 0 && member.IsAsync )
            {
                tokens.Add( Token( SyntaxKind.AsyncKeyword ) );
            }

            return TokenList( tokens );
        }

        private static void AddAccessibilityTokens( IMemberOrNamedType member, List<SyntaxToken> tokens )
        {
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
                    tokens.Add( Token( SyntaxKind.PrivateKeyword ) );

                    break;

                case Accessibility.PrivateProtected:
                    tokens.Add( Token( SyntaxKind.PrivateKeyword ) );
                    tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );

                    break;

                case Accessibility.Protected:
                    tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );

                    break;

                case Accessibility.Internal:
                    tokens.Add( Token( SyntaxKind.InternalKeyword ) );

                    break;

                case Accessibility.ProtectedInternal:
                    tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );
                    tokens.Add( Token( SyntaxKind.InternalKeyword ) );

                    break;

                case Accessibility.Public:
                    tokens.Add( Token( SyntaxKind.PublicKeyword ) );

                    break;
            }
        }

        private static SyntaxTokenList GetParameterSyntaxModifierList( IParameter parameter )
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