// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;
using TypedConstant = Metalama.Framework.Code.TypedConstant;
using VarianceKind = Metalama.Framework.Code.VarianceKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal partial class OurSyntaxGenerator
    {
        public static OurSyntaxGenerator NullOblivious { get; }

        public static OurSyntaxGenerator Default { get; }

        public static OurSyntaxGenerator CompileTime => Default;

        public static OurSyntaxGenerator GetInstance( bool nullableContext ) => nullableContext ? Default : NullOblivious;

        static OurSyntaxGenerator()
        {
            var referencedWorkspaceAssemblyName =
                typeof(OurSyntaxGenerator).Assembly.GetReferencedAssemblies()
                    .Single( a => string.Equals( a.Name, "Microsoft.CodeAnalysis.Workspaces", StringComparison.OrdinalIgnoreCase ) );

            var requiredWorkspaceImplementationAssemblyName = new AssemblyName(
                referencedWorkspaceAssemblyName.ToString().Replace( "Microsoft.CodeAnalysis.Workspaces", "Microsoft.CodeAnalysis.CSharp.Workspaces" ) );

            // See if the assembly is already loaded in the AppDomain.
            var assembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where( a => AssemblyName.ReferenceMatchesDefinition( requiredWorkspaceImplementationAssemblyName, a.GetName() ) )
                .OrderByDescending( a => a.GetName().Version )
                .FirstOrDefault();

            // If is not present, load it.
            if ( assembly == null )
            {
                assembly = Assembly.Load( requiredWorkspaceImplementationAssemblyName );
            }

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

        public TypeOfExpressionSyntax TypeOfExpression( ITypeSymbol type, IReadOnlyDictionary<string, TypeSyntax>? substitutions = null )
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

            if ( substitutions != null && substitutions.Count > 0 )
            {
                var substitutionRewriter = new SubstitutionRewriter( substitutions );
                rewrittenTypeSyntax = substitutionRewriter.Visit( rewrittenTypeSyntax );
            }

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

        public ExpressionSyntax TypeOrNamespace( INamespaceOrTypeSymbol symbol )
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

        public AttributeSyntax Attribute( IAttributeData attribute, ReflectionMapper reflectionMapper )
        {
            var constructorArguments = attribute.ConstructorArguments.Select(
                a => AttributeArgument( this.AttributeValueExpression( a.Value, reflectionMapper ) ) );

            var namedArguments = attribute.NamedArguments.Select(
                a => AttributeArgument(
                    NameEquals( a.Key ),
                    null,
                    this.AttributeValueExpression( a.Value, reflectionMapper ) ) );

            var attributeSyntax = SyntaxFactory.Attribute( (NameSyntax) this.Type( attribute.Type.GetSymbol() ) );

            var argumentList = AttributeArgumentList( SeparatedList( constructorArguments.Concat( namedArguments ) ) );

            if ( argumentList.Arguments.Count > 0 )
            {
                // Add the argument list only when it is non-empty, otherwise this generates redundant parenthesis.
                attributeSyntax = attributeSyntax.WithArgumentList( argumentList );
            }

            return attributeSyntax;
        }

        public SyntaxNode AddAttribute( SyntaxNode oldNode, IAttributeData attribute, ReflectionMapper reflectionMapper )
        {
            var attributeList = AttributeList( SingletonSeparatedList( this.Attribute( attribute, reflectionMapper ) ) )
                .WithLeadingTrivia( oldNode.GetLeadingTrivia() )
                .WithTrailingTrivia( ElasticLineFeed );

            SyntaxNode newNode = oldNode.Kind() switch
            {
                SyntaxKind.MethodDeclaration => ((MethodDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.DestructorDeclaration => ((DestructorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.ConstructorDeclaration => ((ConstructorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.InterfaceDeclaration => ((InterfaceDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.DelegateDeclaration => ((DelegateDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.EnumDeclaration => ((EnumDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.ClassDeclaration => ((ClassDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.StructDeclaration => ((StructDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.Parameter => ((ParameterSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.PropertyDeclaration => ((PropertyDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.EventDeclaration => ((EventDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.AddAccessorDeclaration => ((AccessorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.RemoveAccessorDeclaration => ((AccessorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.GetAccessorDeclaration => ((AccessorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.SetAccessorDeclaration => ((AccessorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.OperatorDeclaration => ((OperatorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.ConversionOperatorDeclaration => ((ConversionOperatorDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.IndexerDeclaration => ((IndexerDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.FieldDeclaration => ((FieldDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                SyntaxKind.EventFieldDeclaration => ((EventFieldDeclarationSyntax) oldNode).AddAttributeLists( attributeList ),
                _ => throw new AssertionFailedException()
            };

            return newNode;
        }

        private ExpressionSyntax AttributeValueExpression( object? value, ReflectionMapper reflectionMapper )
        {
            if ( value == null )
            {
                return SyntaxFactoryEx.Null;
            }

            if ( value is TypedConstant typedConstant )
            {
                return this.AttributeValueExpression( typedConstant.Value, reflectionMapper );
            }

            var literalExpression = SyntaxFactoryEx.LiteralExpressionOrNull( value );

            if ( literalExpression != null )
            {
                return literalExpression;
            }

            if ( value is Type type )
            {
                return this.TypeOfExpression( reflectionMapper.GetTypeSymbol( type ) );
            }

            var valueType = value.GetType();

            if ( valueType.IsEnum )
            {
                var name = Enum.GetName( valueType, value );
                var enumType = reflectionMapper.GetTypeSymbol( valueType );

                if ( name != null )
                {
                    return MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        this.Type( enumType ),
                        SyntaxFactory.IdentifierName( name ) );
                }
                else
                {
                    var underlyingValue = Convert.ChangeType( value, Enum.GetUnderlyingType( valueType ), CultureInfo.InvariantCulture )!;

                    return this.CastExpression( enumType, this.LiteralExpression( underlyingValue ) );
                }
            }

            throw new ArgumentOutOfRangeException( nameof(value), $"The value '{value}' cannot be converted to a custom attribute argument value." );
        }

        private class SubstitutionRewriter : CSharpSyntaxRewriter
        {
            private readonly IReadOnlyDictionary<string, TypeSyntax> _substitutions;

            public SubstitutionRewriter( IReadOnlyDictionary<string, TypeSyntax> substitutions )
            {
                this._substitutions = substitutions;
            }

            public override SyntaxNode? VisitIdentifierName( IdentifierNameSyntax node )
            {
                if ( this._substitutions.TryGetValue( node.Identifier.Text, out var substitution ) )
                {
                    return substitution;
                }
                else
                {
                    return base.VisitIdentifierName( node );
                }
            }
        }
    }
}