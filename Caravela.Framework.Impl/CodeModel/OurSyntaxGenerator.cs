// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;
using VarianceKind = Caravela.Framework.Code.VarianceKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal partial class OurSyntaxGenerator
    {
        public static OurSyntaxGenerator NullOblivious { get; }

        public static OurSyntaxGenerator Default { get; }

        public static OurSyntaxGenerator CompileTime => Default;

        public static OurSyntaxGenerator GetInstance( bool nullableContext ) => nullableContext ? Default : NullOblivious;

        static OurSyntaxGenerator()
        {
            var version =
                typeof(OurSyntaxGenerator).Assembly.GetReferencedAssemblies()
                    .Single( a => string.Equals( a.Name, "Microsoft.CodeAnalysis.Workspaces", StringComparison.OrdinalIgnoreCase ) )
                    .Version;

            var assembly = Assembly.Load( "Microsoft.CodeAnalysis.CSharp.Workspaces, Version=" + version );

            var type = assembly.GetType( $"Microsoft.CodeAnalysis.CSharp.CodeGeneration.CSharpSyntaxGenerator" )!;
            var field = type.GetField( "Instance", BindingFlags.Public | BindingFlags.Static )!;
            var syntaxGenerator = (SyntaxGenerator) field.GetValue( null );
            Default = new OurSyntaxGenerator( syntaxGenerator, true );
            NullOblivious = new OurSyntaxGenerator( syntaxGenerator, false );
        }

        private readonly SyntaxGenerator _syntaxGenerator;

        private OurSyntaxGenerator( SyntaxGenerator syntaxGenerator, bool nullAware )
        {
            this._syntaxGenerator = syntaxGenerator;
            this.IsNullAware = nullAware;
        }

        public TypeOfExpressionSyntax TypeOfExpression( ITypeSymbol type )
        {
            var typeSyntax = this.Type( type.WithNullableAnnotation( NullableAnnotation.NotAnnotated ) );

            if ( type is INamedTypeSymbol { IsGenericType: true } namedType )
            {
                if ( namedType.IsGenericTypeDefinition() )
                {
                    // In generic definitions, we must remove type arguments.
                    typeSyntax = (TypeSyntax) RemoveTypeArgumentsRewriter.Instance.Visit( typeSyntax );
                }
            }

            // In any typeof, we must remove ? annotations of nullable types.
            typeSyntax = (TypeSyntax) new RemoveReferenceNullableAnnotationsRewriter( type ).Visit( typeSyntax );

            // In any typeof, we must change dynamic to object.
            typeSyntax = (TypeSyntax) DynamicToVarRewriter.Instance.Visit( typeSyntax );

            var rewriter = type switch
            {
                INamedTypeSymbol { IsGenericType: true } genericType when genericType.IsGenericTypeDefinition() => RemoveTypeArgumentsRewriter.Instance,
                INamedTypeSymbol { IsGenericType: true } => new RemoveReferenceNullableAnnotationsRewriter( type ),
                _ => DynamicToVarRewriter.Instance
            };

            var rewrittenTypeSyntax = rewriter.Visit( typeSyntax );

            return (TypeOfExpressionSyntax) this._syntaxGenerator.TypeOfExpression( rewrittenTypeSyntax );
        }

        public TypeSyntax Type( ITypeSymbol symbol )
        {
            var typeSyntax = (TypeSyntax) this._syntaxGenerator.TypeExpression( symbol ).WithAdditionalAnnotations( Simplifier.Annotation );

            if ( !this.IsNullAware )
            {
                typeSyntax = (TypeSyntax) new RemoveReferenceNullableAnnotationsRewriter( symbol ).Visit( typeSyntax );
            }

            return typeSyntax;
        }

        public ExpressionSyntax DefaultExpression( ITypeSymbol typeSymbol )
            => SyntaxFactory.DefaultExpression( this.Type( typeSymbol ) )
                .WithAdditionalAnnotations( Simplifier.Annotation );

        public ArrayCreationExpressionSyntax ArrayCreationExpression( TypeSyntax type, IEnumerable<SyntaxNode> elements )
        {
            var array = (ArrayCreationExpressionSyntax) this._syntaxGenerator.ArrayCreationExpression( type, elements );

            return array.WithType( array.Type.WithAdditionalAnnotations( Simplifier.Annotation ) );
        }

        public TypeSyntax Type( SpecialType specialType )
            => (TypeSyntax) this._syntaxGenerator.TypeExpression( specialType )
                .WithAdditionalAnnotations( Simplifier.Annotation );

        public CastExpressionSyntax CastExpression( ITypeSymbol targetTypeSymbol, ExpressionSyntax expression )
        {
            switch ( expression )
            {
                case BinaryExpressionSyntax:
                case ConditionalExpressionSyntax:
                case CastExpressionSyntax:
                case PrefixUnaryExpressionSyntax:
                    expression = ParenthesizedExpression( expression );

                    break;
            }

            return SyntaxFactory.CastExpression( this.Type( targetTypeSymbol ), expression ).WithAdditionalAnnotations( Simplifier.Annotation );
        }

        public ExpressionSyntax NameExpression( INamespaceOrTypeSymbol symbol )
        {
            ExpressionSyntax expression;

            switch ( symbol )
            {
                case ITypeSymbol typeSymbol:
                    return this.Type( typeSymbol );

                case INamespaceSymbol namespaceSymbol:
                    expression = (ExpressionSyntax) this._syntaxGenerator.NameExpression( namespaceSymbol );

                    break;

                default:
                    throw new AssertionFailedException();
            }

            return expression.WithAdditionalAnnotations( Simplifier.Annotation );
        }

        public ThisExpressionSyntax ThisExpression() => (ThisExpressionSyntax) this._syntaxGenerator.ThisExpression();

        public LiteralExpressionSyntax LiteralExpression( object literal ) => (LiteralExpressionSyntax) this._syntaxGenerator.LiteralExpression( literal );

        public IdentifierNameSyntax IdentifierName( string identifier ) => (IdentifierNameSyntax) this._syntaxGenerator.IdentifierName( identifier );

        public TypeSyntax ArrayTypeExpression( TypeSyntax type )
        {
            var arrayType = (ArrayTypeSyntax) this._syntaxGenerator.ArrayTypeExpression( type ).WithAdditionalAnnotations( Simplifier.Annotation );

            // Roslyn does not specify the rank properly so it needs to be fixed up.

            return arrayType.WithRankSpecifiers(
                SingletonList( ArrayRankSpecifier( SingletonSeparatedList<ExpressionSyntax>( OmittedArraySizeExpression() ) ) ) );
        }

        public TypeSyntax ReturnType( IMethod method ) => this.Type( method.ReturnType.GetSymbol() );

        public TypeSyntax PropertyType( IProperty property ) => this.Type( property.Type.GetSymbol() );

        public TypeSyntax EventType( IEvent property ) => this.Type( property.Type.GetSymbol() );

#pragma warning disable CA1822 // Can be made static
        public TypeParameterListSyntax? TypeParameterList( IMethod method )
        {
            if ( method.TypeParameters.Count == 0 )
            {
                return null;
            }
            else
            {
                var list = SyntaxFactory.TypeParameterList( SeparatedList( method.TypeParameters.Select( TypeParameter ).ToArray() ) );

                return list;
            }
        }
#pragma warning restore CA1822 // Can be made static

        private static TypeParameterSyntax TypeParameter( ITypeParameter typeParameter )
        {
            var syntax = SyntaxFactory.TypeParameter( typeParameter.Name );

            switch ( typeParameter.Variance )
            {
                case VarianceKind.In:
                    syntax = syntax.WithVarianceKeyword( Token( SyntaxKind.InKeyword ) );

                    break;

                case VarianceKind.Out:
                    syntax = syntax.WithVarianceKeyword( Token( SyntaxKind.OutKeyword ) );

                    break;
            }

            return syntax;
        }

        public ParameterListSyntax ParameterList( IMethodBase method )
            =>

                // TODO: generics
                SyntaxFactory.ParameterList(
                    SeparatedList(
                        method.Parameters.Select(
                            p => Parameter(
                                List<AttributeListSyntax>(),
                                p.GetSyntaxModifierList(),
                                this.Type( p.Type.GetSymbol() ),
                                Identifier( p.Name ),
                                null ) ) ) );

        public SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses( IMethod method )
        {
            List<TypeParameterConstraintClauseSyntax>? clauses = null;

            foreach ( var genericParameter in method.TypeParameters )
            {
                List<TypeParameterConstraintSyntax>? constraints = null;

                switch ( genericParameter.TypeKindConstraint )
                {
                    case TypeKindConstraint.Class:
                        constraints ??= new List<TypeParameterConstraintSyntax>();
                        var constraint = ClassOrStructConstraint( SyntaxKind.ClassConstraint );

                        if ( genericParameter.HasDefaultConstructorConstraint )
                        {
                            constraint = constraint.WithQuestionToken( Token( SyntaxKind.QuestionToken ) );
                        }

                        constraints.Add( constraint );

                        break;

                    case TypeKindConstraint.Struct:
                        constraints ??= new List<TypeParameterConstraintSyntax>();
                        constraints.Add( ClassOrStructConstraint( SyntaxKind.StructConstraint ) );

                        break;

                    case TypeKindConstraint.Unmanaged:
                        constraints ??= new List<TypeParameterConstraintSyntax>();

                        constraints.Add(
                            TypeConstraint(
                                SyntaxFactory.IdentifierName( Identifier( default, SyntaxKind.UnmanagedKeyword, "unmanaged", "unmanaged", default ) ) ) );

                        break;

                    case TypeKindConstraint.NotNull:
                        constraints ??= new List<TypeParameterConstraintSyntax>();
                        constraints.Add( TypeConstraint( SyntaxFactory.IdentifierName( "notnull" ) ) );

                        break;

                    case TypeKindConstraint.Default:
                        constraints ??= new List<TypeParameterConstraintSyntax>();
                        constraints.Add( DefaultConstraint() );

                        break;
                }

                foreach ( var typeConstraint in genericParameter.TypeConstraints )
                {
                    constraints ??= new List<TypeParameterConstraintSyntax>();

                    constraints.Add( TypeConstraint( this.Type( typeConstraint.GetSymbol() ) ) );
                }

                if ( genericParameter.HasDefaultConstructorConstraint )
                {
                    constraints ??= new List<TypeParameterConstraintSyntax>();
                    constraints.Add( ConstructorConstraint() );
                }

                if ( constraints != null )
                {
                    clauses ??= new List<TypeParameterConstraintClauseSyntax>();

                    clauses.Add(
                        TypeParameterConstraintClause(
                            SyntaxFactory.IdentifierName( genericParameter.Name ),
                            SeparatedList( constraints ) ) );
                }
            }

            if ( clauses == null )
            {
                return default;
            }
            else
            {
                return List( clauses );
            }
        }
    }
}