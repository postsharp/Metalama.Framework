// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.CodeModel
{
    internal abstract class MemberOrNamedType : Declaration, IMemberOrNamedType
    {
        public bool IsSealed => this.Symbol.IsSealed;

        public bool IsNew
        {
            get
            {
                this.OnUsingDeclaration();

                var syntaxReference = this.Symbol.GetPrimarySyntaxReference();

                if ( syntaxReference == null )
                {
                    return false;
                }

                var syntaxNode = syntaxReference.GetSyntax();

                switch ( syntaxNode )
                {
                    case MemberDeclarationSyntax memberDeclaration:
                        return memberDeclaration.Modifiers.Any( m => m.IsKind( SyntaxKind.NewKeyword ) );

                    case VariableDeclaratorSyntax { Parent: { Parent: EventFieldDeclarationSyntax eventFieldDeclaration } }:
                        return eventFieldDeclaration.Modifiers.Any( m => m.IsKind( SyntaxKind.NewKeyword ) );

                    case VariableDeclaratorSyntax { Parent: { Parent: FieldDeclarationSyntax fieldDeclaration } }:
                        return fieldDeclaration.Modifiers.Any( m => m.IsKind( SyntaxKind.NewKeyword ) );

                    case LocalFunctionStatementSyntax:
                        return false;

                    case ParameterSyntax: // Record positional properties.
                        return false;

                    default:
                        throw new AssertionFailedException( $"Unexpected declaration node kind {syntaxNode.Kind()} at '{syntaxNode.GetLocation()}'." );
                }
            }
        }

        public bool IsAbstract
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Symbol.IsAbstract;
            }
        }

        public bool IsStatic
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Symbol.IsStatic;
            }
        }

        public INamedType? DeclaringType
        {
            get
            {
                this.OnUsingDeclaration();

                return this.DeclaringTypeImpl;
            }
        }

        [Memo]
        private INamedType? DeclaringTypeImpl
            => this.Symbol.ContainingType != null ? this.Compilation.Factory.GetNamedType( this.Symbol.ContainingType ) : null;

        public abstract MemberInfo ToMemberInfo();

        protected MemberOrNamedType( CompilationModel compilation, ISymbol symbol ) : base( compilation, symbol ) { }

        public Accessibility Accessibility
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Symbol.DeclaredAccessibility switch
                {
                    Microsoft.CodeAnalysis.Accessibility.NotApplicable => Accessibility.Private,
                    Microsoft.CodeAnalysis.Accessibility.Private => Accessibility.Private,
                    Microsoft.CodeAnalysis.Accessibility.ProtectedAndInternal => Accessibility.PrivateProtected,
                    Microsoft.CodeAnalysis.Accessibility.Protected => Accessibility.Protected,
                    Microsoft.CodeAnalysis.Accessibility.Internal => Accessibility.Internal,
                    Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal => Accessibility.ProtectedInternal,
                    Microsoft.CodeAnalysis.Accessibility.Public => Accessibility.Public,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public string Name
        {
            get
            {
                this.OnUsingDeclaration();

                return this.Symbol.Name;
            }
        }

        INamedType? IMemberOrNamedType.DeclaringType
        {
            get
            {
                this.OnUsingDeclaration();

                return this.DeclaringType;
            }
        }

        public override SyntaxTree? PrimarySyntaxTree
        {
            get
            {
                var primarySyntaxReference = this.Symbol.GetPrimarySyntaxReference();

                if ( primarySyntaxReference != null )
                {
                    return primarySyntaxReference.SyntaxTree;
                }
                else if ( this.ContainingDeclaration != null )
                {
                    // In case we have an implicit type member, look at the primary syntax tree of the type itself.

                    return this.ContainingDeclaration.GetPrimarySyntaxTree();
                }
                else
                {
                    return null;
                }
            }
        }
    }
}