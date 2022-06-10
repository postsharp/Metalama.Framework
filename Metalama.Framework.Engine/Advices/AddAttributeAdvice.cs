// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;

namespace Metalama.Framework.Engine.Advices;

internal class AddAttributeAdvice : Advice
{
    private readonly IAttributeData _attribute;
    private readonly OverrideStrategy _overrideStrategy;

    public AddAttributeAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance template,
        IDeclaration targetDeclaration,
        IAttributeData attribute,
        OverrideStrategy overrideStrategy,
        string? layerName ) : base( aspect, template, targetDeclaration, layerName )
    {
        this._attribute = attribute;
        this._overrideStrategy = overrideStrategy;
    }

    public override void Initialize( IServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder ) { }

    public override AdviceResult ToResult( IServiceProvider serviceProvider, ICompilation compilation )
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
                        return AdviceResult.Create(
                            AdviceDiagnosticDescriptors.AttributeAlreadyPresent.CreateRoslynDiagnostic(
                                targetDeclaration.GetDiagnosticLocation(),
                                (this.Aspect.AspectClass.ShortName, this._attribute.Type, targetDeclaration) ) );

                    case OverrideStrategy.Ignore:
                        return AdviceResult.Empty;

                    case OverrideStrategy.New:
                        return CreateResult();

                    case OverrideStrategy.Override:
                        var removeTransformation = new RemoveAttributesTransformation(
                            this,
                            targetDeclaration,
                            this._attribute.Type );

                        return CreateResult( removeTransformation );

                    default:
                        throw new AssertionFailedException();
                }
            }
        }

        return CreateResult();

        AdviceResult CreateResult( RemoveAttributesTransformation? removeTransformation = null )
        {
            List<ITransformation> transformations = new();

            if ( removeTransformation != null )
            {
                transformations.Add( removeTransformation );
            }

            if ( targetDeclaration.ContainingDeclaration is IConstructor { IsImplicit: true } constructor )
            {
                transformations.Add( new ConstructorBuilder( this, constructor.DeclaringType, ObjectReader.Empty ) );
            }

            transformations.Add( new AttributeBuilder( this, targetDeclaration, this._attribute ) );

            return AdviceResult.Create( transformations );
        }
    }
}