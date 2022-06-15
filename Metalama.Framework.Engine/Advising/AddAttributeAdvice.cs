// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Advising;

internal class AddAttributeAdvice : Advice
{
    private readonly IAttributeData _attribute;
    private readonly OverrideStrategy _overrideStrategy;

    public AddAttributeAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance template,
        IDeclaration targetDeclaration,
        ICompilation sourceCompilation,
        IAttributeData attribute,
        OverrideStrategy overrideStrategy,
        string? layerName ) : base( aspect, template, targetDeclaration, sourceCompilation, layerName )
    {
        this._attribute = attribute;
        this._overrideStrategy = overrideStrategy;
    }

    public override AdviceImplementationResult Implement(
        IServiceProvider serviceProvider,
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
                                (this.Aspect.AspectClass.ShortName, this._attribute.Type, targetDeclaration) ) );

                    case OverrideStrategy.Ignore:
                        return AdviceImplementationResult.Ignored;

                    case OverrideStrategy.New:
                        AddTransformations();

                        return AdviceImplementationResult.Success( AdviceOutcome.New );

                    case OverrideStrategy.Override:
                        var removeTransformation = new RemoveAttributesTransformation(
                            this,
                            targetDeclaration,
                            this._attribute.Type );

                        AddTransformations( removeTransformation );

                        return AdviceImplementationResult.Success( AdviceOutcome.Override );

                    default:
                        throw new AssertionFailedException();
                }
            }
        }

        AddTransformations();

        return AdviceImplementationResult.Success();

        void AddTransformations( RemoveAttributesTransformation? removeTransformation = null )
        {
            if ( removeTransformation != null )
            {
                addTransformation( removeTransformation );
            }

            if ( targetDeclaration.ContainingDeclaration is IConstructor { IsImplicit: true } constructor )
            {
                addTransformation( new ConstructorBuilder( this, constructor.DeclaringType, ObjectReader.Empty ) );
            }

            addTransformation( new AttributeBuilder( this, targetDeclaration, this._attribute ) );
        }
    }
}