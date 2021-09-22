// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using VarianceKind = Caravela.Framework.Code.VarianceKind;

namespace Caravela.Framework.Impl.Utilities
{
    internal static class SyntaxHelpers
    {
        public static TypeSyntax CreateSyntaxForReturnType( IMethod method )
            => LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( method.ReturnType.GetSymbol() );

        public static TypeSyntax CreateSyntaxForPropertyType( IProperty property )
            => LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( property.Type.GetSymbol() );

        public static TypeSyntax CreateSyntaxForEventType( IEvent property )
            => LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( property.EventType.GetSymbol() );

        public static TypeParameterListSyntax? CreateSyntaxForTypeParameterList( IMethod method )
        {
            if ( method.GenericParameters.Count == 0 )
            {
                return null;
            }
            else
            {
                var list = SyntaxFactory.TypeParameterList(
                    SyntaxFactory.SeparatedList( method.GenericParameters.Select( CreateSyntaxForTypeParameter ).ToArray() ) );

                return list;
            }
        }

        private static TypeParameterSyntax CreateSyntaxForTypeParameter( IGenericParameter genericParameter )
        {
            var typeParameter = SyntaxFactory.TypeParameter( genericParameter.Name );

            switch ( genericParameter.Variance )
            {
                case VarianceKind.In:
                    typeParameter = typeParameter.WithVarianceKeyword( SyntaxFactory.Token( SyntaxKind.InKeyword ) );

                    break;

                case VarianceKind.Out:
                    typeParameter = typeParameter.WithVarianceKeyword( SyntaxFactory.Token( SyntaxKind.OutKeyword ) );

                    break;
            }

            return typeParameter;
        }

        public static ParameterListSyntax CreateSyntaxForParameterList( IMethodBase method )
            =>

                // TODO: generics
                SyntaxFactory.ParameterList(
                    SyntaxFactory.SeparatedList(
                        method.Parameters.Select(
                            p => SyntaxFactory.Parameter(
                                SyntaxFactory.List<AttributeListSyntax>(),
                                p.GetSyntaxModifierList(),
                                LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( p.ParameterType.GetSymbol() ),
                                SyntaxFactory.Identifier( p.Name ),
                                null ) ) ) );

        public static SyntaxList<TypeParameterConstraintClauseSyntax> CreateSyntaxForConstraintClauses( IMethod method )
        {
            List<TypeParameterConstraintClauseSyntax>? clauses = null;

            foreach ( var genericParameter in method.GenericParameters )
            {
                List<TypeParameterConstraintSyntax>? constraints = null;

                switch ( genericParameter.TypeKindConstraint )
                {
                    case TypeKindConstraint.Class:
                        constraints ??= new List<TypeParameterConstraintSyntax>();
                        constraints.Add( SyntaxFactory.ClassOrStructConstraint( SyntaxKind.ClassConstraint ) );

                        break;

                    case TypeKindConstraint.NullableClass:
                        constraints ??= new List<TypeParameterConstraintSyntax>();

                        constraints.Add(
                            SyntaxFactory.ClassOrStructConstraint( SyntaxKind.ClassConstraint )
                                .WithQuestionToken( SyntaxFactory.Token( SyntaxKind.QuestionToken ) ) );

                        break;

                    case TypeKindConstraint.Struct:
                        constraints ??= new List<TypeParameterConstraintSyntax>();
                        constraints.Add( SyntaxFactory.ClassOrStructConstraint( SyntaxKind.StructConstraint ) );

                        break;

                    case TypeKindConstraint.Unmanaged:
                        constraints ??= new List<TypeParameterConstraintSyntax>();

                        constraints.Add(
                            SyntaxFactory.TypeConstraint(
                                SyntaxFactory.IdentifierName(
                                    SyntaxFactory.Identifier( default, SyntaxKind.UnmanagedKeyword, "unmanaged", "unmanaged", default ) ) ) );

                        break;

                    case TypeKindConstraint.NotNull:
                        constraints ??= new List<TypeParameterConstraintSyntax>();
                        constraints.Add( SyntaxFactory.TypeConstraint( SyntaxFactory.IdentifierName( "notnull" ) ) );

                        break;

                    case TypeKindConstraint.Default:
                        constraints ??= new List<TypeParameterConstraintSyntax>();
                        constraints.Add( SyntaxFactory.DefaultConstraint() );

                        break;
                }

                foreach ( var typeConstraint in genericParameter.TypeConstraints )
                {
                    constraints ??= new List<TypeParameterConstraintSyntax>();

                    constraints.Add(
                        SyntaxFactory.TypeConstraint( LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( typeConstraint.GetSymbol() ) ) );
                }

                if ( genericParameter.HasDefaultConstructorConstraint )
                {
                    constraints ??= new List<TypeParameterConstraintSyntax>();
                    constraints.Add( SyntaxFactory.ConstructorConstraint() );
                }

                if ( constraints != null )
                {
                    clauses ??= new List<TypeParameterConstraintClauseSyntax>();

                    clauses.Add(
                        SyntaxFactory.TypeParameterConstraintClause(
                            SyntaxFactory.IdentifierName( genericParameter.Name ),
                            SyntaxFactory.SeparatedList( constraints ) ) );
                }
            }

            if ( clauses == null )
            {
                return default;
            }
            else
            {
                return SyntaxFactory.List( clauses );
            }
        }
    }
}