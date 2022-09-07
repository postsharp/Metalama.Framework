﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.CodeFixes.Implementations;

internal class ChangeVisibilityCodeAction : CodeActionBase
{
    public IMemberOrNamedType TargetMember { get; }

    public Accessibility Accessibility { get; }

    public ChangeVisibilityCodeAction( IMemberOrNamedType targetMember, Accessibility accessibility )
    {
        this.TargetMember = targetMember;
        this.Accessibility = accessibility;
    }

    protected override Task ExecuteImplAsync( CodeActionContext context )
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var compilation = context.Compilation.Compilation;

        var targetSymbol = this.TargetMember.ToRef().GetSymbol( compilation );

        if ( targetSymbol == null )
        {
            throw new ArgumentOutOfRangeException( nameof(this.TargetMember), "The declaration is not declared in source." );
        }

        foreach ( var referenceGroup in targetSymbol.DeclaringSyntaxReferences.GroupBy( r => r.SyntaxTree ) )
        {
            var syntaxTree = referenceGroup.Key;

            var rewriter = new Rewriter( referenceGroup.Select( x => x.GetSyntax( context.CancellationToken ) ).ToList(), this );
            var newRoot = rewriter.Visit( syntaxTree.GetRoot( context.CancellationToken ) )!;
            context.UpdateTree( newRoot, syntaxTree );
        }

        return Task.CompletedTask;
    }

    private class Rewriter : SafeSyntaxRewriter
    {
        private readonly IReadOnlyList<SyntaxNode> _nodes;
        private readonly ChangeVisibilityCodeAction _parent;

        public Rewriter( IReadOnlyList<SyntaxNode> nodes, ChangeVisibilityCodeAction parent )
        {
            this._nodes = nodes;
            this._parent = parent;
        }

        // Block the visiting of implementations.

        public override SyntaxNode? VisitBlock( BlockSyntax node ) => node;

        public override SyntaxNode? VisitEqualsValueClause( EqualsValueClauseSyntax node ) => node;

        public override SyntaxNode? VisitArrowExpressionClause( ArrowExpressionClauseSyntax node ) => node;

        // Visit all members.
        public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            => ((ClassDeclarationSyntax) base.VisitClassDeclaration( node )!).WithModifiers( this.ChangeModifiers( node, node.Modifiers ) );

        public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node )
            => ((RecordDeclarationSyntax) base.VisitRecordDeclaration( node )!).WithModifiers( this.ChangeModifiers( node, node.Modifiers ) );

        public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node )
            => ((StructDeclarationSyntax) base.VisitStructDeclaration( node )!).WithModifiers( this.ChangeModifiers( node, node.Modifiers ) );

        public override SyntaxNode? VisitFieldDeclaration( FieldDeclarationSyntax node )
            => ((FieldDeclarationSyntax) base.VisitFieldDeclaration( node )!).WithModifiers( this.ChangeModifiers( node, node.Modifiers ) );

        public override SyntaxNode? VisitEventDeclaration( EventDeclarationSyntax node )
            => ((EventDeclarationSyntax) base.VisitEventDeclaration( node )!).WithModifiers( this.ChangeModifiers( node, node.Modifiers ) );

        public override SyntaxNode? VisitEventFieldDeclaration( EventFieldDeclarationSyntax node )
            => ((EventFieldDeclarationSyntax) base.VisitEventFieldDeclaration( node )!).WithModifiers( this.ChangeModifiers( node, node.Modifiers ) );

        public override SyntaxNode? VisitPropertyDeclaration( PropertyDeclarationSyntax node )
            => ((PropertyDeclarationSyntax) base.VisitPropertyDeclaration( node )!).WithModifiers( this.ChangeModifiers( node, node.Modifiers ) );

        public override SyntaxNode? VisitEnumDeclaration( EnumDeclarationSyntax node )
            => ((EnumDeclarationSyntax) base.VisitEnumDeclaration( node )!).WithModifiers( this.ChangeModifiers( node, node.Modifiers ) );

        public override SyntaxNode? VisitDelegateDeclaration( DelegateDeclarationSyntax node )
            => ((DelegateDeclarationSyntax) base.VisitDelegateDeclaration( node )!).WithModifiers( this.ChangeModifiers( node, node.Modifiers ) );

        public override SyntaxNode? VisitConstructorDeclaration( ConstructorDeclarationSyntax node )
            => ((ConstructorDeclarationSyntax) base.VisitConstructorDeclaration( node )!).WithModifiers( this.ChangeModifiers( node, node.Modifiers ) );

        public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
            => ((MethodDeclarationSyntax) base.VisitMethodDeclaration( node )!).WithModifiers( this.ChangeModifiers( node, node.Modifiers ) );

        public override SyntaxNode? VisitDestructorDeclaration( DestructorDeclarationSyntax node )
            => ((DestructorDeclarationSyntax) base.VisitDestructorDeclaration( node )!).WithModifiers( this.ChangeModifiers( node, node.Modifiers ) );

        public override SyntaxNode? VisitAccessorDeclaration( AccessorDeclarationSyntax node )
            => ((AccessorDeclarationSyntax) base.VisitAccessorDeclaration( node )!).WithModifiers( this.ChangeModifiers( node, node.Modifiers ) );

        public override SyntaxNode? VisitOperatorDeclaration( OperatorDeclarationSyntax node )
            => ((OperatorDeclarationSyntax) base.VisitOperatorDeclaration( node )!).WithModifiers( this.ChangeModifiers( node, node.Modifiers ) );

        public override SyntaxNode? VisitConversionOperatorDeclaration( ConversionOperatorDeclarationSyntax node )
            => ((ConversionOperatorDeclarationSyntax) base.VisitConversionOperatorDeclaration( node )!).WithModifiers(
                this.ChangeModifiers( node, node.Modifiers ) );

        // Main logic.
        private SyntaxTokenList ChangeModifiers( SyntaxNode node, SyntaxTokenList modifiers )
        {
            if ( this._nodes.Contains( node ) )
            {
                var newModifiers = new List<SyntaxToken>();

                void AddModifier( SyntaxKind kind )
                {
                    var token = SyntaxFactory.Token( kind ).WithTrailingTrivia( SyntaxFactory.ElasticSpace );

                    if ( newModifiers.Count == 0 && modifiers.Count > 0 )
                    {
                        token = token.WithLeadingTrivia( modifiers[0].LeadingTrivia );
                    }

                    newModifiers.Add( token );
                }

                switch ( this._parent.Accessibility )
                {
                    case Accessibility.Internal:
                        AddModifier( SyntaxKind.InternalKeyword );

                        break;

                    case Accessibility.Private:
                        AddModifier( SyntaxKind.PrivateKeyword );

                        break;

                    case Accessibility.Protected:
                        AddModifier( SyntaxKind.ProtectedKeyword );

                        break;

                    case Accessibility.Public:
                        AddModifier( SyntaxKind.PublicKeyword );

                        break;

                    case Accessibility.PrivateProtected:
                        AddModifier( SyntaxKind.PrivateKeyword );
                        AddModifier( SyntaxKind.ProtectedKeyword );

                        break;

                    case Accessibility.ProtectedInternal:
                        AddModifier( SyntaxKind.ProtectedKeyword );
                        AddModifier( SyntaxKind.InternalKeyword );

                        break;

                    default:
                        throw new AssertionFailedException();
                }

                newModifiers.AddRange(
                    modifiers.Where( m => !IsAccessibilityModifier( m ) ).Select( m => m.WithoutTrivia().WithTrailingTrivia( SyntaxFactory.ElasticSpace ) ) );

                return SyntaxFactory.TokenList( newModifiers );
            }
            else
            {
                return modifiers;
            }
        }

        private static bool IsAccessibilityModifier( SyntaxToken token )
            => token.Kind() switch
            {
                SyntaxKind.PrivateKeyword => true,
                SyntaxKind.PublicKeyword => true,
                SyntaxKind.InternalKeyword => true,
                SyntaxKind.ProtectedKeyword => true,
                _ => false
            };
    }
}