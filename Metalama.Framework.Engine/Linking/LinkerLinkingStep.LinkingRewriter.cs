// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed partial class LinkerLinkingStep
    {
        /// <summary>
        /// Rewriter which rewrites classes and methods producing the linked and inlined syntax tree.
        /// </summary>
        private sealed class LinkingRewriter : SafeSyntaxRewriter
        {
            private readonly CompilationContext _compilationContext;
            private readonly SemanticModelProvider _semanticModelProvider;
            private readonly LinkerRewritingDriver _rewritingDriver;

            public LinkingRewriter(
                CompilationContext intermediateCompilationContext,
                LinkerRewritingDriver rewritingDriver )
            {
                this._compilationContext = intermediateCompilationContext;
                this._semanticModelProvider = intermediateCompilationContext.SemanticModelProvider;
                this._rewritingDriver = rewritingDriver;
            }

            public override SyntaxNode VisitStructDeclaration( StructDeclarationSyntax node )
                => node.WithMembers( List( this.GetMembersForTypeDeclaration( node ) ) );

            public override SyntaxNode VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                var transformedMembers = this.GetMembersForTypeDeclaration( node );

                var semanticModel = this._semanticModelProvider.GetSemanticModel( node.SyntaxTree );

                var symbol = semanticModel.GetDeclaredSymbol( node ).AssertNotNull();

                node = this._rewritingDriver.RewriteClass( node, symbol );

                node = node.WithMembers( List( transformedMembers ) );

                return node;
            }

            public override SyntaxNode VisitInterfaceDeclaration( InterfaceDeclarationSyntax node )
                => node.WithMembers( List( this.GetMembersForTypeDeclaration( node ) ) );

            public override SyntaxNode VisitRecordDeclaration( RecordDeclarationSyntax node )
            {
                var transformedMembers = this.GetMembersForTypeDeclaration( node ).AssertNotNull();

                if ( node.ParameterList != null )
                {
                    var semanticModel = this._semanticModelProvider.GetSemanticModel( node.SyntaxTree );
                    SyntaxGenerationContext? generationContext = null;

                    List<MemberDeclarationSyntax>? newMembers = null;

                    var transformedParametersAndCommas = new List<SyntaxNodeOrToken>( node.ParameterList.Parameters.Count * 2 );

                    for ( var i = 0; i < node.ParameterList.Parameters.Count; i++ )
                    {
                        var parameter = node.ParameterList.Parameters[i];
                        newMembers ??= new List<MemberDeclarationSyntax>();

                        var parameterSymbol = semanticModel.GetDeclaredSymbol( parameter );

                        if ( parameterSymbol == null )
                        {
                            continue;
                        }

                        var propertySymbol = parameterSymbol.ContainingType.GetMembers( parameterSymbol.Name ).OfType<IPropertySymbol>().FirstOrDefault();

                        if ( propertySymbol != null && this._rewritingDriver.IsRewriteTarget( propertySymbol ) )
                        {
                            SyntaxGenerationContext GetSyntaxGenerationContext()
                                => generationContext ??= this._compilationContext.GetSyntaxGenerationContext(
                                    node.SyntaxTree,
                                    node.SpanStart );

                            if ( this._rewritingDriver.IsRewriteTarget( propertySymbol.AssertNotNull() ) )
                            {
                                // Add new members that take place of synthesized positional property.
                                newMembers.AddRange(
                                    this._rewritingDriver.RewritePositionalProperty(
                                        parameter,
                                        propertySymbol.AssertNotNull(),
                                        GetSyntaxGenerationContext() ) );
                            }

                            // Remove all attributes related to properties (property/field/get/set target specifiers).
                            var transformedParameter =
                                parameter.WithAttributeLists(
                                    List(
                                        parameter.AttributeLists
                                            .Where(
                                                l =>
                                                    l.Target?.Identifier.IsKind( SyntaxKind.PropertyKeyword ) != true
                                                    && l.Target?.Identifier.IsKind( SyntaxKind.FieldKeyword ) != true
                                                    && l.Target?.Identifier.IsKind( SyntaxKind.GetKeyword ) != true
                                                    && l.Target?.Identifier.IsKind( SyntaxKind.SetKeyword ) != true ) ) );

                            transformedParametersAndCommas.Add( transformedParameter );
                        }
                        else
                        {
                            transformedParametersAndCommas.Add( parameter );
                        }

                        if ( i < node.ParameterList.Parameters.SeparatorCount )
                        {
                            transformedParametersAndCommas.Add( node.ParameterList.Parameters.GetSeparator( i ) );
                        }
                    }

                    node = node.WithParameterList( node.ParameterList.WithParameters( SeparatedList<ParameterSyntax>( transformedParametersAndCommas ) ) );

                    if ( newMembers is { Count: > 0 } )
                    {
                        transformedMembers =
                            transformedMembers.Concat( newMembers );
                    }
                }

                return node.WithMembers( List( transformedMembers ) );
            }

            private IReadOnlyList<MemberDeclarationSyntax> GetMembersForTypeDeclaration( TypeDeclarationSyntax node )
            {
                var newMembers = new List<MemberDeclarationSyntax>();

                foreach ( var member in node.Members )
                {
                    // Go through all members of the type.
                    // For members that represent overrides:
                    //  * If the member can be inlined, skip it.
                    //  * If the member cannot be inlined (or is the root of inlining), add the transformed member with all possible inlining instances.
                    // For members that represent override targets (i.e. overridden members):
                    //  * If the last (transformation order) override is inlineable, replace the member with it's transformed body.
                    //  * Otherwise create a stub that calls the last override.

                    var semanticModel = this._semanticModelProvider.GetSemanticModel( node.SyntaxTree );

                    var symbols =
                        member switch
                        {
                            ConstructorDeclarationSyntax ctorDecl => new ISymbol?[] { semanticModel.GetDeclaredSymbol( ctorDecl ) },
                            OperatorDeclarationSyntax operatorDecl => new ISymbol?[] { semanticModel.GetDeclaredSymbol( operatorDecl ) },
                            ConversionOperatorDeclarationSyntax destructorDecl => new ISymbol?[] { semanticModel.GetDeclaredSymbol( destructorDecl ) },
                            DestructorDeclarationSyntax destructorDecl => new ISymbol?[] { semanticModel.GetDeclaredSymbol( destructorDecl ) },
                            MethodDeclarationSyntax methodDecl => new ISymbol?[] { semanticModel.GetDeclaredSymbol( methodDecl ) },
                            BasePropertyDeclarationSyntax basePropertyDecl => new[] { semanticModel.GetDeclaredSymbol( basePropertyDecl ) },
                            FieldDeclarationSyntax fieldDecl =>
                                fieldDecl.Declaration.Variables.SelectAsArray( v => semanticModel.GetDeclaredSymbol( v ) ),
                            EventFieldDeclarationSyntax eventFieldDecl =>
                                eventFieldDecl.Declaration.Variables.SelectAsArray( v => semanticModel.GetDeclaredSymbol( v ) ),
                            _ => Array.Empty<ISymbol>()
                        };

                    if ( symbols.Length == 0 || symbols is [null] )
                    {
                        // TODO: Comment when this happens.
                        newMembers.Add( (MemberDeclarationSyntax) this.Visit( member )! );

                        continue;
                    }

                    SyntaxGenerationContext? generationContext = null;

                    SyntaxGenerationContext GetSyntaxGenerationContext()
                        => generationContext ??= this._compilationContext.GetSyntaxGenerationContext(
                            node.SyntaxTree,
                            member.SpanStart );

                    if ( symbols.Length == 1 )
                    {
                        // Simple case where the declaration declares a single symbol.
                        if ( this._rewritingDriver.IsRewriteTarget( symbols[0].AssertNotNull() ) )
                        {
                            // Add rewritten member and it's induced members (or nothing if the member is discarded).
                            newMembers.AddRange( this._rewritingDriver.RewriteMember( member, symbols[0].AssertNotNull(), GetSyntaxGenerationContext() ) );
                        }
                        else
                        {
                            // Normal member without any transformations.
                            newMembers.Add( (MemberDeclarationSyntax) this.Visit( member )! );
                        }
                    }
                    else
                    {
                        var remainingSymbols = new HashSet<ISymbol>( this._compilationContext.SymbolComparer );

                        foreach ( var symbol in symbols )
                        {
                            if ( this._rewritingDriver.IsRewriteTarget( symbol.AssertNotNull() ) )
                            {
                                newMembers.AddRange( this._rewritingDriver.RewriteMember( member, symbol.AssertNotNull(), GetSyntaxGenerationContext() ) );
                            }
                            else
                            {
                                remainingSymbols.Add( symbol.AssertNotNull() );
                            }
                        }

                        if ( remainingSymbols.Count == symbols.Length )
                        {
                            // No change.
                            newMembers.Add( member );
                        }
                        else if ( remainingSymbols.Count > 0 )
                        {
                            // Remove declarators that were rewritten.
                            switch ( member )
                            {
                                case EventFieldDeclarationSyntax eventFieldDecl:
                                    newMembers.Add(
                                        eventFieldDecl.WithDeclaration(
                                            eventFieldDecl.Declaration.WithVariables(
                                                SeparatedList(
                                                    eventFieldDecl.Declaration.Variables
                                                        .Where(
                                                            v =>
                                                                remainingSymbols.Contains( semanticModel.GetDeclaredSymbol( v ).AssertNotNull() ) ) ) ) ) );

                                    break;

                                default:
                                    throw new AssertionFailedException( $"Unexpected member {member.Kind()} at '{member.GetLocation()}'." );
                            }
                        }
                    }
                }

                return newMembers;
            }
        }
    }
}