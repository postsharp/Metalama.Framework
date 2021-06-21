// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static class LanguageServiceFactory
    {
        public static AnnotatingSyntaxGenerator CSharpSyntaxGenerator { get; }

        static LanguageServiceFactory()
        {
            var version =
                typeof(LanguageServiceFactory).Assembly.GetReferencedAssemblies()
                    .Single( a => a.Name == "Microsoft.CodeAnalysis.Workspaces" )
                    .Version;

            var assembly = Assembly.Load( "Microsoft.CodeAnalysis.CSharp.Workspaces, Version=" + version );

            var type = assembly.GetType( $"Microsoft.CodeAnalysis.CSharp.CodeGeneration.CSharpSyntaxGenerator" )!;
            var field = type.GetField( "Instance", BindingFlags.Public | BindingFlags.Static )!;
            var syntaxGenerator = (SyntaxGenerator) field.GetValue( null );
            CSharpSyntaxGenerator = new AnnotatingSyntaxGenerator( syntaxGenerator );
        }

        internal class AnnotatingSyntaxGenerator
        {
            private readonly SyntaxGenerator _syntaxGenerator;

            public AnnotatingSyntaxGenerator( SyntaxGenerator syntaxGenerator )
            {
                this._syntaxGenerator = syntaxGenerator;
            }

            public TypeSyntax TypeExpression( ITypeSymbol symbol )
                => (TypeSyntax) this._syntaxGenerator.TypeExpression( symbol )
                    .WithAdditionalAnnotations( Simplifier.Annotation );

            public ParameterSyntax ParameterDeclaration( string name, TypeSyntax type, ExpressionSyntax? expression, RefKind refKind )
                => (ParameterSyntax) this._syntaxGenerator.ParameterDeclaration( name, type, expression, refKind );

            public SimpleNameSyntax GenericName( string methodName, IEnumerable<ITypeSymbol> @select )
                => (SimpleNameSyntax) this._syntaxGenerator.GenericName( methodName, select )
                    .WithAdditionalAnnotations( Simplifier.Annotation );

            public ExpressionSyntax DefaultExpression( ITypeSymbol typeSymbol )
                => (ExpressionSyntax) this._syntaxGenerator.DefaultExpression( typeSymbol )
                    .WithAdditionalAnnotations( Simplifier.Annotation );

            public ArrayCreationExpressionSyntax ArrayCreationExpression( TypeSyntax type, IEnumerable<SyntaxNode> elements )
            {
                var array = (ArrayCreationExpressionSyntax) this._syntaxGenerator.ArrayCreationExpression( type, elements );

                return array.WithType( array.Type.WithAdditionalAnnotations( Simplifier.Annotation ) );
            }

            public TypeSyntax TypeExpression( SpecialType specialType )
                => (TypeSyntax) this._syntaxGenerator.TypeExpression( specialType )
                    .WithAdditionalAnnotations( Simplifier.Annotation );

            public TupleExpressionSyntax TupleExpression( IEnumerable<SyntaxNode> elements )
                => (TupleExpressionSyntax) this._syntaxGenerator.TupleExpression( elements );

            public CastExpressionSyntax CastExpression( ITypeSymbol targetTypeSymbol, ExpressionSyntax expression )
            {
                var cast = (CastExpressionSyntax) this._syntaxGenerator.CastExpression( targetTypeSymbol, expression );

                return cast.WithType( cast.Type.WithAdditionalAnnotations( Simplifier.Annotation ) );
            }

            public ExpressionSyntax NameExpression( INamespaceOrTypeSymbol symbol )
                => (ExpressionSyntax) this._syntaxGenerator.NameExpression( symbol )
                    .WithAdditionalAnnotations( Simplifier.Annotation );

            public ThisExpressionSyntax ThisExpression() => (ThisExpressionSyntax) this._syntaxGenerator.ThisExpression();

            public LiteralExpressionSyntax LiteralExpression( object literal ) => (LiteralExpressionSyntax) this._syntaxGenerator.LiteralExpression( literal );

            public IdentifierNameSyntax IdentifierName( string identifier ) => (IdentifierNameSyntax) this._syntaxGenerator.IdentifierName( identifier );

            public TypeSyntax ArrayTypeExpression( ExpressionSyntax type )
            {
                return (TypeSyntax) this._syntaxGenerator.ArrayTypeExpression( type ).WithAdditionalAnnotations( Simplifier.Annotation );
            }
        }
    }
}