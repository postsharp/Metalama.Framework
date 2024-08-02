// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.AdviceImpl.Attributes;

internal sealed class AddAttributeAdvice : Advice<AddAttributeAdviceResult>
{
    private readonly IAttributeData _attribute;
    private readonly OverrideStrategy _overrideStrategy;

    public AddAttributeAdvice( AdviceConstructorParameters parameters, IAttributeData attribute, OverrideStrategy overrideStrategy )
        : base( parameters )
    {
        this._attribute = attribute;
        this._overrideStrategy = overrideStrategy;
    }

    public override AdviceKind AdviceKind => AdviceKind.IntroduceAttribute;

    protected override AddAttributeAdviceResult Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

        if ( this._overrideStrategy != OverrideStrategy.New )
        {
            // Determine if we already have a custom attribute of this type, and handle conflict.

            var existingAttribute = targetDeclaration.Attributes.OfAttributeType( this._attribute.Type ).FirstOrDefault();

            if ( existingAttribute != null )
            {
                // There is a conflict.

                switch ( this._overrideStrategy )
                {
                    case OverrideStrategy.Fail:
                        return this.CreateFailedResult(
                            AdviceDiagnosticDescriptors.AttributeAlreadyPresent.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.AspectInstance.AspectClass.ShortName, this._attribute.Type, targetDeclaration),
                                this ) );

                    case OverrideStrategy.Ignore:
                        return new AddAttributeAdviceResult( AdviceOutcome.Ignore, existingAttribute.GetReference() );

                    case OverrideStrategy.Override:
                        var removeTransformation = new RemoveAttributesTransformation(
                            this,
                            targetDeclaration,
                            this._attribute.Type );

                        return AddTransformations( AdviceOutcome.Override, removeTransformation );

                    default:
                        throw new AssertionFailedException( $"Invalid value of OverrideStrategy: {this._overrideStrategy}." );
                }
            }
        }

        return AddTransformations( AdviceOutcome.Default );

        AddAttributeAdviceResult AddTransformations( AdviceOutcome outcome, RemoveAttributesTransformation? removeTransformation = null )
        {
            if ( removeTransformation != null )
            {
                addTransformation( removeTransformation );
            }

            if ( targetDeclaration.ContainingDeclaration is IConstructor { IsImplicitlyDeclared: true } constructor )
            {
                addTransformation(
                    new ConstructorBuilder( this, constructor.DeclaringType )
                    {
                        ReplacedImplicit = constructor.ToTypedRef(), Accessibility = Accessibility.Public
                    }.ToTransformation() );
            }

            var attributeBuilder = new AttributeBuilder( this, targetDeclaration, this._attribute );
            addTransformation( attributeBuilder.ToTransformation() );

            return new AddAttributeAdviceResult( outcome, attributeBuilder.ToAttributeRef() );
        }
    }
}