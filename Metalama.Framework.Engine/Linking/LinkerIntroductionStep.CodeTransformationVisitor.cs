// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Linking;

internal partial class LinkerIntroductionStep
{
    private class BodyTransformationVisitor : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly Dictionary<ISymbol, (TypeDeclarationSyntax Declaration, bool Static, bool Instance)> _typesWithRequiredImplicitConstructors;
        private readonly List<InsertedStatement> _marks;
        private Context _currentContext;

        public IReadOnlyList<InsertedStatement> Marks => this._marks;

        public IReadOnlyDictionary<ISymbol, (TypeDeclarationSyntax Declaration, bool Static, bool Instance)> TypesWithRequiredImplicitConstructors
            => this._typesWithRequiredImplicitConstructors;

        public BodyTransformationVisitor( SemanticModel semanticModel, IReadOnlyList<IInsertStatementTransformation> transformations )
        {
            this._semanticModel = semanticModel;
            this._marks = new List<InsertedStatement>();
            this._currentContext = new Context( transformations );
            this._typesWithRequiredImplicitConstructors = new Dictionary<ISymbol, (TypeDeclarationSyntax Declaration, bool Static, bool Instance)>();
        }

        public override void VisitBlock( BlockSyntax node ) => this.VisitBody( node, n => base.VisitBlock( n ) );

        public override void VisitArrowExpressionClause( ArrowExpressionClauseSyntax node )
            => this.VisitBody( node, n => base.VisitArrowExpressionClause( n ) );

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
                    if ( !SymbolEqualityComparer.Default.Equals( transformation.TargetDeclaration.GetSymbol(), symbol ) )
                    {
                        continue;
                    }

                    var context = new InsertStatementTransformationContext(
                        transformation,
                        this._semanticModel.GetDeclaredSymbol( node.Parent ).AssertNotNull(),
                        node );

                    transformation.GetInsertedStatement( context );

                    if ( !visitDeeper )
                    {
                        unrejectedTransformations.Remove( transformation );
                    }

                    this._marks.AddRange( transformations );
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

        public override void VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node, n => base.VisitClassDeclaration( n ) );

        public override void VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node, n => base.VisitStructDeclaration( n ) );

        public override void VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitTypeDeclaration( node, n => base.VisitRecordDeclaration( n ) );

        private void VisitTypeDeclaration<T>( T node, Action<T> baseVisit )
            where T : TypeDeclarationSyntax
        {
            var symbol = this._semanticModel.GetDeclaredSymbol( node ).AssertNotNull();
            var hasStaticRequiredImplicitCtor = false;
            var hasInstanceRequiredImplicitCtor = false;

            bool AddSyntaxNodeTransformations( IInsertStatementTransformation transformation )
            {
                var context = new InsertStatementTransformationContext( transformation, this._semanticModel.GetDeclaredSymbol( node ).AssertNotNull(), null );
                transformation.GetInsertedStatement( context );

                if ( !transformations.IsDefaultOrEmpty )
                {
                    this._marks.AddRange( transformations );

                    return true;
                }
                else
                {
                    return false;
                }
            }

            // Temporary detection of implicit instance constructors.
            foreach ( var ctor in symbol.Constructors )
            {
                var syntaxReference = ctor.GetPrimarySyntaxReference();

                if ( syntaxReference == null )
                {
                    foreach ( var transformation in this._currentContext.CodeTransformations )
                    {
                        if ( !SymbolEqualityComparer.Default.Equals( ctor, transformation.TargetDeclaration.GetSymbol() ) )
                        {
                            continue;
                        }

                        if ( AddSyntaxNodeTransformations( transformation ) )
                        {
                            hasInstanceRequiredImplicitCtor = true;
                        }
                    }
                }
            }

            // Temporary detection of missing static constructors.
            if ( symbol.StaticConstructors.Length == 0 )
            {
                foreach ( var transformation in this._currentContext.CodeTransformations )
                {
                    if ( transformation.TargetDeclaration.DeclarationKind == DeclarationKind.Constructor
                         && transformation.TargetDeclaration.IsStatic
                         && transformation.TargetDeclaration.GetSymbol() == null
                         && SymbolEqualityComparer.Default.Equals( symbol, transformation.TargetDeclaration.DeclaringType.GetSymbol() ) )
                    {
                        if ( AddSyntaxNodeTransformations( transformation ) )
                        {
                            hasStaticRequiredImplicitCtor = true;
                        }
                    }
                }
            }

            if ( hasStaticRequiredImplicitCtor || hasInstanceRequiredImplicitCtor )
            {
                this._typesWithRequiredImplicitConstructors.Add( symbol, (node, hasStaticRequiredImplicitCtor, hasInstanceRequiredImplicitCtor) );
            }

            baseVisit( node );
        }

        internal class Context
        {
            public ImmutableList<IInsertStatementTransformation> CodeTransformations { get; }

            public Context( IReadOnlyList<IInsertStatementTransformation> transformations )
            {
                var builder = ImmutableList.CreateBuilder<IInsertStatementTransformation>();
                builder.AddRange( transformations );
                this.CodeTransformations = builder.ToImmutable();
            }
        }
    }
}