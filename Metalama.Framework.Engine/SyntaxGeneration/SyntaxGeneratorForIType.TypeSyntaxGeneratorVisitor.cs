// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.SyntaxGeneration;

internal partial class SyntaxGeneratorForIType
{
    // Based on Roslyn TypeSyntaxGeneratorVisitor.
    private class TypeSyntaxGeneratorVisitor : TypeVisitor<TypeSyntax>
    {
        private readonly SyntaxGeneratorForIType _syntaxGeneratorForIType;

        public TypeSyntaxGeneratorVisitor( SyntaxGeneratorForIType syntaxGeneratorForIType )
        {
            this._syntaxGeneratorForIType = syntaxGeneratorForIType;
        }

        public override TypeSyntax DefaultVisit( IType type ) => throw new AssertionFailedException();

        private TTypeSyntax AddInformationTo<TTypeSyntax>( TTypeSyntax syntax, IType type )
            where TTypeSyntax : TypeSyntax
        {
            var generationOptions = this._syntaxGeneratorForIType._generationOptions;

            if ( generationOptions.TriviaMatters )
            {
                syntax = syntax
                    .WithRequiredLeadingTrivia( syntax.GetLeadingTrivia().Insert( 0, SyntaxFactory.ElasticMarker ) )
                    .WithRequiredTrailingTrivia( syntax.GetTrailingTrivia().Add( SyntaxFactory.ElasticMarker ) );
            }

            if ( generationOptions.AddFormattingAnnotations )
            {
                syntax = syntax.WithAdditionalAnnotations( SymbolAnnotation.Create( type ) );
            }

            return syntax;
        }

        private TTypeSyntax AddInformationTo<TTypeSyntax>( TTypeSyntax syntax, INamespace ns )
            where TTypeSyntax : TypeSyntax
        {
            var generationOptions = this._syntaxGeneratorForIType._generationOptions;

            if ( generationOptions.TriviaMatters )
            {
                syntax = syntax
                    .WithRequiredLeadingTrivia( syntax.GetLeadingTrivia().Insert( 0, SyntaxFactory.ElasticMarker ) )
                    .WithRequiredTrailingTrivia( syntax.GetTrailingTrivia().Add( SyntaxFactory.ElasticMarker ) );
            }

            syntax = syntax.WithAdditionalAnnotations( SymbolAnnotation.Create( ns ) );

            return syntax;
        }

        private static IdentifierNameSyntax ToIdentifierName( string identifier ) => (IdentifierNameSyntax) RoslynSyntaxGenerator.IdentifierName( identifier );

        public override TypeSyntax VisitArrayType( IArrayType type )
        {
            IType underlyingType = type;

            while ( underlyingType is IArrayType innerArray )
            {
                underlyingType = innerArray.ElementType;

                if ( underlyingType.IsNullableReferenceType() )
                {
                    // If the inner array we just moved to is also nullable, then
                    // we must terminate the digging now so we produce the syntax for that,
                    // and then append the ranks we passed through at the end. This is because
                    // nullability annotations acts as a "barrier" where we won't reorder array
                    // through. So whereas:
                    //
                    //     string[][,]
                    //
                    // is really an array of rank 1 that has an element of rank 2,
                    //
                    //     string[]?[,]
                    //
                    // is really an array of rank 2 that has nullable elements of rank 1.

                    break;
                }
            }

            var elementTypeSyntax = this._syntaxGeneratorForIType.TypeExpression( underlyingType );
            var ranks = new List<ArrayRankSpecifierSyntax>();

            var arrayType = type;
            while ( arrayType != null && !arrayType.Equals( underlyingType ) )
            {
                ranks.Add( SyntaxFactory.ArrayRankSpecifier(
                    SyntaxFactory.SeparatedList( Enumerable.Repeat<ExpressionSyntax>( SyntaxFactory.OmittedArraySizeExpression(), arrayType.Rank ) ) ) );

                arrayType = arrayType.ElementType as IArrayType;
            }

            TypeSyntax arrayTypeSyntax = SyntaxFactory.ArrayType( elementTypeSyntax, SyntaxFactory.List( ranks ) );

            if ( type.IsNullableReferenceType() )
            {
                arrayTypeSyntax = SyntaxFactory.NullableType( arrayTypeSyntax );
            }

            return this.AddInformationTo( arrayTypeSyntax, type );
        }

        public override TypeSyntax VisitDynamicType( IDynamicType type )
            => this.AddInformationTo( SyntaxFactory.IdentifierName( "dynamic" ), type );

        public override TypeSyntax VisitFunctionPointerType( IFunctionPointerType functionPointerType ) => throw new NotImplementedException();

        public TypeSyntax CreateSimpleTypeSyntax( INamedType type )
        {
            var syntax = this.TryCreateSpecializedNamedTypeSyntax( type );
            if ( syntax != null )
            {
                return syntax;
            }

            if ( type.Name == string.Empty )
            {
                return CreateSystemObject();
            }

            if ( type.TypeParameters.Count == 0 )
            {
                if ( type.TypeKind == TypeKind.Error && type.Name == "var" )
                {
                    return CreateSystemObject();
                }

                return ToIdentifierName( type.Name );
            }

            var typeArguments = type.TypeArguments.SelectAsArray( this._syntaxGeneratorForIType.TypeExpression );

            return SyntaxFactory.GenericName(
                ToIdentifierName( type.Name ).Identifier,
                SyntaxFactory.TypeArgumentList( SyntaxFactory.SeparatedList( typeArguments ) ) );
        }

        public static QualifiedNameSyntax CreateSystemObject()
        {
            return SyntaxFactory.QualifiedName(
                SyntaxFactory.AliasQualifiedName(
                    CreateGlobalIdentifier(),
                    SyntaxFactory.IdentifierName( "System" ) ),
                SyntaxFactory.IdentifierName( "Object" ) );
        }

        private static IdentifierNameSyntax CreateGlobalIdentifier()
            => SyntaxFactory.IdentifierName( SyntaxFactory.Token( SyntaxKind.GlobalKeyword ) );

        private TypeSyntax? TryCreateSpecializedNamedTypeSyntax( INamedType type )
        {
            if ( type.SpecialType == SpecialType.Void )
            {
                return SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.VoidKeyword ) );
            }

            if ( type.IsNullable == true && type.IsReferenceType == false )
            {
                // Can't have a nullable of a pointer type.  i.e. "int*?" is illegal.
                var innerType = type.TypeArguments.First();
                if ( innerType.TypeKind != TypeKind.Pointer )
                {
                    return this.AddInformationTo( SyntaxFactory.NullableType( this._syntaxGeneratorForIType.TypeExpression( innerType ) ), type );
                }
            }

            return null;
        }

        public override TypeSyntax VisitNamedType( INamedType type )
        {
            var typeSyntax = this.CreateSimpleTypeSyntax( type );
            if ( typeSyntax is not SimpleNameSyntax )
            {
                return typeSyntax;
            }

            var simpleNameSyntax = (SimpleNameSyntax) typeSyntax;
            if ( type.DeclaringType != null )
            {
                var containingTypeSyntax = this.VisitNamedType( type.DeclaringType );
                if ( containingTypeSyntax is NameSyntax name )
                {
                    typeSyntax = this.AddInformationTo( SyntaxFactory.QualifiedName( name, simpleNameSyntax ), type );
                }
                else
                {
                    typeSyntax = this.AddInformationTo( simpleNameSyntax, type );
                }
            }
            else if ( type.Namespace != null )
            {
                if ( type.Namespace.IsGlobalNamespace )
                {
                    if ( type.TypeKind != TypeKind.Error )
                    {
                        typeSyntax = this.AddGlobalAlias( type, simpleNameSyntax );
                    }
                }
                else
                {
                    var container = this.VisitNamespace( type.Namespace );
                    typeSyntax = this.AddInformationTo( SyntaxFactory.QualifiedName( (NameSyntax) container, simpleNameSyntax ), type );
                }
            }

            if ( type.IsNullableReferenceType() )
            {
                // value type with nullable annotation may be composed from unconstrained nullable generic
                // doesn't mean nullable value type in this case
                typeSyntax = this.AddInformationTo( SyntaxFactory.NullableType( typeSyntax ), type );
            }

            return typeSyntax;
        }

        public TypeSyntax VisitNamespace( INamespace ns )
        {
            var syntax = this.AddInformationTo( ToIdentifierName( ns.Name ), ns );
            if ( ns.ParentNamespace == null )
            {
                return syntax;
            }

            if ( ns.ParentNamespace.IsGlobalNamespace )
            {
                return this.AddGlobalAlias( ns, syntax );
            }
            else
            {
                var container = this.VisitNamespace( ns.ParentNamespace );
                return this.AddInformationTo( SyntaxFactory.QualifiedName( (NameSyntax) container, syntax ), ns );
            }
        }

        /// <summary>
        /// We always unilaterally add "global::" to all named types/namespaces.  This
        /// will then be trimmed off if possible by the simplifier.
        /// </summary>
        private TypeSyntax AddGlobalAlias( IType type, SimpleNameSyntax syntax )
        {
            return this.AddInformationTo( SyntaxFactory.AliasQualifiedName( CreateGlobalIdentifier(), syntax ), type );
        }

        /// <summary>
        /// We always unilaterally add "global::" to all named types/namespaces.  This
        /// will then be trimmed off if possible by the simplifier.
        /// </summary>
        private TypeSyntax AddGlobalAlias( INamespace ns, SimpleNameSyntax syntax )
        {
            return this.AddInformationTo( SyntaxFactory.AliasQualifiedName( CreateGlobalIdentifier(), syntax ), ns );
        }

        public override TypeSyntax VisitPointerType( IPointerType type )
        {
            return this.AddInformationTo( SyntaxFactory.PointerType( this._syntaxGeneratorForIType.TypeExpression( type.PointedAtType ) ), type );
        }

        public override TypeSyntax VisitTypeParameter( ITypeParameter type )
        {
            TypeSyntax typeSyntax = this.AddInformationTo( ToIdentifierName( type.Name ), type );
            if ( type.IsNullableReferenceType() )
            {
                // value type with nullable annotation may be composed from unconstrained nullable generic
                // doesn't mean nullable value type in this case
                typeSyntax = this.AddInformationTo( SyntaxFactory.NullableType( typeSyntax ), type );
            }

            return typeSyntax;
        }

        // Copied from Roslyn.
        internal class SymbolAnnotation
        {
            public const string Kind = "SymbolId";

            public static SyntaxAnnotation Create( IType type ) => new( Kind, DeclarationDocumentationCommentId.CreateReferenceId( type ) );

            public static SyntaxAnnotation Create( INamespace ns ) => new( Kind, DeclarationDocumentationCommentId.CreateReferenceId( ns ) );
        }
    }
}