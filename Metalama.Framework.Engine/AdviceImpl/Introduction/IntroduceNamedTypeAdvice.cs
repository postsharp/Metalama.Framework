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

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceNamedTypeAdvice : IntroduceDeclarationAdvice<INamedType, NamedTypeBuilder>
{
    public override AdviceKind AdviceKind => AdviceKind.IntroduceType;

    private OverrideStrategy OverrideStrategy { get; }

    public IntroduceNamedTypeAdvice(
        AdviceConstructorParameters<INamespaceOrNamedType> parameters,
        string? explicitName,
        OverrideStrategy overrideStrategy,
        Action<NamedTypeBuilder>? buildAction )
        : base( parameters, buildAction )
    {
        this.OverrideStrategy = overrideStrategy;
        this.Builder = new NamedTypeBuilder( this, parameters.TargetDeclaration.AssertNotNull(), explicitName.AssertNotNull() );
    }

    protected override void Initialize( in ProjectServiceProvider serviceProvider, IDiagnosticAdder diagnosticAdder )
    {
        base.Initialize( serviceProvider, diagnosticAdder );

        this.BuildAction?.Invoke( this.Builder );
    }

    protected override IntroductionAdviceResult<INamedType> Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        var targetDeclaration = this.TargetDeclaration.As<INamespaceOrNamedType>().GetTarget( compilation );

        var existingType =
            targetDeclaration switch
            {
                INamespace @namespace =>
                    @namespace.Types
                        .OfName( this.Builder.Name )
                        .FirstOrDefault( t => this.Builder.TypeParameters.Count == t.TypeParameters.Count ),
                INamedType namedType =>
                    namedType.AllTypes
                        .OfName( this.Builder.Name )
                        .FirstOrDefault( t => this.Builder.TypeParameters.Count == t.TypeParameters.Count ),
                _ => throw new AssertionFailedException( $"Unsupported: {targetDeclaration}" ),
            };

        if ( existingType == null )
        {
            addTransformation( this.Builder.ToTransformation() );

            return this.CreateSuccessResult( AdviceOutcome.Default, this.Builder );
        }
        else
        {
            switch ( this.OverrideStrategy )
            {
                case OverrideStrategy.Fail:
                    return this.CreateFailedResult(
                        AdviceDiagnosticDescriptors.CannotIntroduceNewTypeWhenItAlreadyExists.CreateRoslynDiagnostic(
                            targetDeclaration.GetDiagnosticLocation(),
                            (this.AspectInstance.AspectClass.ShortName, this.Builder, targetDeclaration),
                            this ) );

                case OverrideStrategy.Ignore:
                    return this.CreateIgnoredResult( existingType );

                case OverrideStrategy.New:
                    this.Builder.HasNewKeyword = this.Builder.IsNew = true;
                    addTransformation( this.Builder.ToTransformation() );

                    return this.CreateSuccessResult( AdviceOutcome.Default, this.Builder );

                default:
                    throw new AssertionFailedException( $"Unexpected OverrideStrategy: {this.OverrideStrategy}." );
            }
        }
    }
}