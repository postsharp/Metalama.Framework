// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerIntroductionStep
    {
        private class CodeTransformationVisitor : CSharpSyntaxWalker
        {
            private readonly SemanticModel _semanticModel;
            private readonly Dictionary<ISymbol, (TypeDeclarationSyntax Declaration, bool Static, bool Instance)> _typesWithRequiredImplicitCtors;
            private readonly List<CodeTransformationMark> _marks;
            private Context _currentContext;

            public IReadOnlyList<CodeTransformationMark> Marks => this._marks;

            public IReadOnlyDictionary<ISymbol, (TypeDeclarationSyntax Declaration, bool Static, bool Instance)> TypesWithRequiredImplicitCtors => this._typesWithRequiredImplicitCtors;

            public CodeTransformationVisitor( SemanticModel semanticModel, IReadOnlyList<ICodeTransformation> transformations)
            {
                this._semanticModel = semanticModel;
                this._marks = new List<CodeTransformationMark>();
                this._currentContext = new Context( transformations );
                this._typesWithRequiredImplicitCtors = new Dictionary<ISymbol, (TypeDeclarationSyntax Declaration, bool Static, bool Instance)>();
            }

            public override void Visit( SyntaxNode? node )
            {
                base.Visit( node );
            }

            public override void VisitBlock( BlockSyntax node )
            {
                this.VisitBody( node, n => base.VisitBlock(n) );
            }

            public override void VisitArrowExpressionClause( ArrowExpressionClauseSyntax node )
            {
                this.VisitBody( node, n => base.VisitArrowExpressionClause( n ) );
            }

            private void VisitBody<T>( T? node, Action<T> visitBase )
                where T : SyntaxNode
            {
                if ( node == null || node.Parent == null || !(this._semanticModel.GetDeclaredSymbol( node.Parent ) is not null and var symbol) )
                {
                    return;
                }

                var previousContext = this._currentContext;

                try
                {
                    var unrejectedTransformations = previousContext.CodeTransformations.ToBuilder();
                    foreach ( var transformation in previousContext.CodeTransformations )
                    {
                        if (!SymbolEqualityComparer.Default.Equals(transformation.TargetDeclaration.GetSymbol(), symbol))
                        {
                            continue;
                        }

                        var context = new CodeTransformationContext( transformation, this._semanticModel.GetDeclaredSymbol(node.Parent).AssertNotNull(), node );
                        transformation.EvaluateSyntaxNode( context );

                        if ( context.IsDeclined )
                        {
                            unrejectedTransformations.Remove( transformation );
                        }

                        this._marks.AddRange( context.Marks );
                    }

                    this._currentContext = new Context( unrejectedTransformations.ToImmutable() );

                    if ( unrejectedTransformations.Count > 0 )
                    {
                        visitBase( node );
                    }
                }
                finally
                {
                    this._currentContext = previousContext;
                }
            }

            public override void VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                this.VisitTypeDeclaration( node, n => base.VisitClassDeclaration( n) );
            }

            public override void VisitStructDeclaration( StructDeclarationSyntax node )
            {
                this.VisitTypeDeclaration( node, n => base.VisitStructDeclaration( n ) );
            }

            public override void VisitRecordDeclaration( RecordDeclarationSyntax node )
            {
                this.VisitTypeDeclaration( node, n => base.VisitRecordDeclaration( n ) );
            }

            private void VisitTypeDeclaration<T>(T node, Action<T> baseVisit)
                where T : TypeDeclarationSyntax
            {
                var symbol = this._semanticModel.GetDeclaredSymbol( node ).AssertNotNull();
                var hasStaticRequiredImplicitCtor = false;
                var hasInstanceRequiredImplicitCtor = false;

                // Temporary detection of implicit instance ctors.
                foreach (var ctor in symbol.Constructors)
                {
                    var syntaxReference = ctor.GetPrimarySyntaxReference();

                    if ( syntaxReference == null )
                    {
                        foreach ( var transformation in this._currentContext.CodeTransformations )
                        {
                            if (!SymbolEqualityComparer.Default.Equals(ctor, transformation.TargetDeclaration.GetSymbol()))
                            {
                                continue;
                            }
                            
                            var context = new CodeTransformationContext( transformation, this._semanticModel.GetDeclaredSymbol( node ).AssertNotNull(), null );
                            transformation.EvaluateSyntaxNode( context );

                            Invariant.Assert( !ctor.IsStatic );

                            if ( context.Marks.Count > 0 )
                            {
                                hasInstanceRequiredImplicitCtor = true;

                                this._marks.AddRange( context.Marks );
                            }
                        }
                    }
                }

                // Temporary detection of missing static ctors.
                if ( symbol.StaticConstructors.Length == 0)
                {
                    foreach ( var transformation in this._currentContext.CodeTransformations )
                    {
                        if ( transformation.TargetDeclaration.DeclarationKind == Code.DeclarationKind.Constructor
                            && transformation.TargetDeclaration.IsStatic
                            && transformation.TargetDeclaration.GetSymbol() == null )
                        {
                            var context = new CodeTransformationContext( transformation, this._semanticModel.GetDeclaredSymbol( node ).AssertNotNull(), null );
                            transformation.EvaluateSyntaxNode( context );

                            if ( context.Marks.Count > 0 )
                            {
                                hasStaticRequiredImplicitCtor = true;

                                this._marks.AddRange( context.Marks );
                            }
                        }
                    }
                }

                if ( hasStaticRequiredImplicitCtor || hasInstanceRequiredImplicitCtor )
                {
                    this._typesWithRequiredImplicitCtors.Add( symbol, (node, hasStaticRequiredImplicitCtor, hasInstanceRequiredImplicitCtor) );
                }

                baseVisit( node );
            }

            internal class Context
            {
                public ImmutableList<ICodeTransformation> CodeTransformations { get; }

                public Context( IReadOnlyList<ICodeTransformation> transformations)
                {
                    var builder = ImmutableList.CreateBuilder<ICodeTransformation>();
                    builder.AddRange( transformations );
                    this.CodeTransformations = builder.ToImmutable();
                }
            }
        }
    }
}