// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerLinkingStep
    {
        /// <summary>
        /// Rewriter which rewrites classes and methods producing the linked and inlined syntax tree.
        /// </summary>
        private class LinkingRewriter : CSharpSyntaxRewriter
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly Compilation _intermediateCompilation;
            private readonly LinkerRewritingDriver _rewritingDriver;

            public LinkingRewriter(
                IServiceProvider serviceProvider,
                Compilation intermediateCompilation,
                LinkerRewritingDriver rewritingDriver )
            {
                this._serviceProvider = serviceProvider;
                this._intermediateCompilation = intermediateCompilation;
                this._rewritingDriver = rewritingDriver;
            }

            public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node )
            {
                return this.VisitTypeDeclaration( node );
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                return this.VisitTypeDeclaration( node );
            }

            public override SyntaxNode? VisitInterfaceDeclaration( InterfaceDeclarationSyntax node )
            {
                return this.VisitTypeDeclaration( node );
            }

            public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node )
            {
                var recordWithTransformedMembers = (RecordDeclarationSyntax) this.VisitTypeDeclaration( node )!;

                if ( node.ParameterList != null )
                {
                    var semanticModel = this._intermediateCompilation.GetSemanticModel( node.SyntaxTree );
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
                                => generationContext ??= SyntaxGenerationContext.Create(
                                    this._serviceProvider,
                                    this._intermediateCompilation,
                                    node.SyntaxTree,
                                    node.SpanStart );

                            var setAccessor =
                                node.ClassOrStructKeyword.IsKind( SyntaxKind.StructKeyword )
                                    ? node.Modifiers.Any( m => m.IsKind( SyntaxKind.ReadOnlyKeyword ) )
                                        ? AccessorDeclaration( SyntaxKind.InitAccessorDeclaration )
                                        : AccessorDeclaration( SyntaxKind.SetAccessorDeclaration )
                                    : AccessorDeclaration( SyntaxKind.InitAccessorDeclaration );

                            // We need to create a "fake" syntax for the property, so we can use normal logic to process
                            // properties with almost no change.
                            var property = PropertyDeclaration(
                                    default,
                                    TokenList( Token( SyntaxKind.PublicKeyword ) ),
                                    parameter.Type.AssertNotNull(),
                                    default,
                                    parameter.Identifier,
                                    AccessorList(
                                        List(
                                            new[]
                                            {
                                                AccessorDeclaration( SyntaxKind.GetAccessorDeclaration )
                                                    .WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) ),
                                                setAccessor.WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) )
                                            } ) ),
                                    null,
                                    EqualsValueClause( IdentifierName( parameter.Identifier ) ) )
                                .NormalizeWhitespace()
                                .AddColoringAnnotation( TextSpanClassification.GeneratedCode );

                            // Property-level custom attributes must be moved from the parameter to the new property.
                            foreach ( var attributeList in parameter.AttributeLists )
                            {
                                if ( attributeList.Target != null )
                                {
                                    var propertyLevelAttributeList = attributeList;

                                    if ( attributeList.Target.Identifier.IsKind( SyntaxKind.PropertyKeyword ) )
                                    {
                                        propertyLevelAttributeList = attributeList.WithTarget( default );
                                    }

                                    property = property.WithAttributeLists( property.AttributeLists.Add( propertyLevelAttributeList ) );
                                }
                            }

                            var transformedParameter = parameter.WithAttributeLists( List( parameter.AttributeLists.Where( l => l.Target == null ) ) );
                            transformedParametersAndCommas.Add( transformedParameter );

                            newMembers.AddRange( this._rewritingDriver.RewriteMember( property, propertySymbol, GetSyntaxGenerationContext() ) );
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

                    if ( newMembers != null )
                    {
                        recordWithTransformedMembers = recordWithTransformedMembers.WithMembers( recordWithTransformedMembers.Members.AddRange( newMembers ) );
                    }

                    recordWithTransformedMembers =
                        recordWithTransformedMembers.WithParameterList( ParameterList( SeparatedList<ParameterSyntax>( transformedParametersAndCommas ) ) );
                }

                return recordWithTransformedMembers;
            }

            private SyntaxNode? VisitTypeDeclaration( TypeDeclarationSyntax node )
            {
                // TODO: Other transformations than method overrides.
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

                    var semanticModel = this._intermediateCompilation.GetSemanticModel( node.SyntaxTree );

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
                                fieldDecl.Declaration.Variables.Select( v => semanticModel.GetDeclaredSymbol( v ) ).ToArray(),
                            EventFieldDeclarationSyntax eventFieldDecl =>
                                eventFieldDecl.Declaration.Variables.Select( v => semanticModel.GetDeclaredSymbol( v ) ).ToArray(),
                            _ => Array.Empty<ISymbol>()
                        };

                    if ( symbols.Length == 0 || (symbols.Length == 1 && symbols[0] == null) )
                    {
                        // TODO: Comment when this happens.
                        newMembers.Add( (MemberDeclarationSyntax) this.Visit( member ) );

                        continue;
                    }

                    SyntaxGenerationContext? generationContext = null;

                    SyntaxGenerationContext GetSyntaxGenerationContext()
                        => generationContext ??= SyntaxGenerationContext.Create(
                            this._serviceProvider,
                            this._intermediateCompilation,
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
                            newMembers.Add( (MemberDeclarationSyntax) this.Visit( member ) );
                        }
                    }
                    else
                    {
                        var remainingSymbols = new HashSet<ISymbol>( SymbolEqualityComparer.Default );

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
                                    throw new AssertionFailedException();
                            }
                        }
                    }
                }

                return node.WithMembers( List( newMembers ) );
            }
        }
    }
}