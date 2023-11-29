// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Advising;

internal sealed class SyntaxBasedInitializeAdvice : InitializeAdvice
{
    private readonly IStatement _statement;

    public SyntaxBasedInitializeAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance templateInstance,
        IMemberOrNamedType targetDeclaration,
        ICompilation sourceCompilation,
        IStatement statement,
        InitializerKind kind,
        string? layerName ) : base( aspect, templateInstance, targetDeclaration, sourceCompilation, kind, layerName )
    {
        this._statement = statement;
    }

    protected override void AddTransformation( IMemberOrNamedType targetDeclaration, IConstructor targetCtor, Action<ITransformation> addTransformation )
    {
        var statementSyntax = ((UserStatement) this._statement).Syntax;

        if ( targetCtor.IsPrimary )
        {
            if ( statementSyntax is not ExpressionStatementSyntax
                {
                    Expression: AssignmentExpressionSyntax
                    {
                        RawKind: (int) SyntaxKind.SimpleAssignmentExpression,
                        Left: var leftExpression,
                        Right: var rightExpression
                    }
                } )
            {
                throw new InvalidOperationException(
                    $"The statement '{statementSyntax}' can't be added as an initializer to a primary constructor. Only simple assignment is supported." );
            }

            var identifier = leftExpression switch
            {
                IdentifierNameSyntax identifierName => identifierName,
                MemberAccessExpressionSyntax
                {
                    RawKind: (int) SyntaxKind.SimpleMemberAccessExpression,
                    Expression: ThisExpressionSyntax,
                    Name: IdentifierNameSyntax thisIdentifierName
                } => thisIdentifierName,
                _ => throw new InvalidOperationException(
                    $"The expression '{leftExpression}' can't used as the assignment target for an initializer of a primary constructor. Only the 'memberName' and 'this.memberName' forms are supported." )
            };

            var memberName = identifier.Identifier.ValueText;
            var fieldOrProperty = targetCtor.DeclaringType.FieldsAndProperties.OfName( memberName ).Single();

            if ( fieldOrProperty.InitializerExpression != null )
            {
                throw new InvalidOperationException(
                    $"The {fieldOrProperty.DeclarationKind.ToDisplayString()} '{fieldOrProperty}' can't be used as the the assignment target for an initializer of a primary constructor, because it already has an initializer." );
            }

            if ( fieldOrProperty.RefKind != RefKind.None )
            {
                throw new InvalidOperationException(
                    $"The {fieldOrProperty.DeclarationKind.ToDisplayString()} '{fieldOrProperty}' can't be used as the the assignment target for an initializer of a primary constructor, because it is a ref {fieldOrProperty.DeclarationKind.ToDisplayString()}." );
            }

            switch ( fieldOrProperty )
            {
                case IProperty { IsAutoPropertyOrField: not true } property:
                    throw new InvalidOperationException(
                        $"The property '{property}' can't be used as the the assignment target for an initializer of a primary constructor, because it is not an auto-property." );

                case IProperty property:
                    var newProperty = new PropertyBuilder(
                        this,
                        property.DeclaringType,
                        property.Name,
                        hasGetter: true,
                        hasSetter: true,
                        isAutoProperty: true,
                        hasInitOnlySetter: property.Writeability == Writeability.InitOnly,
                        hasImplicitGetter: false,
                        hasImplicitSetter: property.Writeability == Writeability.ConstructorOnly,
                        ObjectReader.Empty )
                    {
                        Accessibility = property.Accessibility,
                        HasNewKeyword = property.IsNew,
                        IsRequired = property.IsRequired,
                        IsVirtual = property.IsVirtual,
                        IsOverride = property.IsOverride,
                        IsSealed = property.IsSealed,
                        Type = property.Type
                    };

                    newProperty.SetMethod!.Accessibility = property.SetMethod!.Accessibility;

                    newProperty.AddAttributes( property.Attributes.SelectAsReadOnlyCollection( a => a.ToAttributeConstruction() ) );

                    // TODO: preserve field attributes?

                    newProperty.InitializerExpression = new SyntaxUserExpression( rightExpression, property.Type );

                    addTransformation( new ReplaceWithPropertyTransformation( this, property, newProperty ) );

                    break;

                case IField { Writeability: Writeability.None } field:
                    throw new InvalidOperationException(
                        $"The field '{field}' can't be used as the the assignment target for an initializer of a primary constructor, because it is const." );

                case IField field:
                    var newField = new FieldBuilder( this, field.DeclaringType, field.Name, ObjectReader.Empty )
                    {
                        Accessibility = field.Accessibility,
                        HasNewKeyword = field.IsNew,
                        IsRequired = field.IsRequired,
                        Type = field.Type,
                        Writeability = field.Writeability
                    };

                    newField.AddAttributes( field.Attributes.SelectAsReadOnlyCollection( a => a.ToAttributeConstruction() ) );

                    newField.InitializerExpression = new SyntaxUserExpression( rightExpression, field.Type );

                    addTransformation( new ReplaceWithFieldTransformation( this, field, newField ) );

                    break;

                default:
                    throw new AssertionFailedException( $"Unexpected member type '{fieldOrProperty?.GetType()}'." );
            }
        }
        else
        {
            addTransformation( new SyntaxBasedInitializationTransformation( this, targetDeclaration, targetCtor, _ => statementSyntax ) );
        }
    }
}