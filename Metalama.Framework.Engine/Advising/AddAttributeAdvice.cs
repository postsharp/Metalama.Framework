// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Advising;

internal sealed class AddAttributeAdvice : Advice
{
    private readonly IAttributeData _attribute;
    private readonly OverrideStrategy _overrideStrategy;

    public AddAttributeAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance template,
        IDeclaration targetDeclaration,
        CompilationModel sourceCompilation,
        IAttributeData attribute,
        OverrideStrategy overrideStrategy,
        string? layerName ) : base( aspect, template, targetDeclaration, sourceCompilation, layerName )
    {
        this._attribute = attribute;
        this._overrideStrategy = overrideStrategy;
    }

    public override AdviceKind AdviceKind => AdviceKind.IntroduceAttribute;

    public override AdviceImplementationResult Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

        if ( this._overrideStrategy != OverrideStrategy.New )
        {
            // Determine if we already have a custom attribute of this type, and handle conflict.

            if ( targetDeclaration.Attributes.OfAttributeType( this._attribute.Type ).Any() )
            {
                // There is a conflict.

                switch ( this._overrideStrategy )
                {
                    case OverrideStrategy.Fail:
                        return AdviceImplementationResult.Failed(
                            AdviceDiagnosticDescriptors.AttributeAlreadyPresent.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this._attribute.Type, targetDeclaration),
                                this ) );

                    case OverrideStrategy.Ignore:
                        return AdviceImplementationResult.Ignored;

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

        AdviceImplementationResult AddTransformations( AdviceOutcome outcome, RemoveAttributesTransformation? removeTransformation = null )
        {
            if ( removeTransformation != null )
            {
                addTransformation( removeTransformation );
            }

            if ( targetDeclaration.ContainingDeclaration is IConstructor { IsImplicitlyDeclared: true } constructor )
            {
                addTransformation( new ConstructorBuilder( constructor.DeclaringType, this ).ToTransformation() );
            }

            var attributeBuilder = new AttributeBuilder( this, targetDeclaration, this._attribute );
            addTransformation( attributeBuilder.ToTransformation() );

            return AdviceImplementationResult.Success( outcome, attributeBuilder.ToTypedRef<IDeclaration>() );
        }
    }
}