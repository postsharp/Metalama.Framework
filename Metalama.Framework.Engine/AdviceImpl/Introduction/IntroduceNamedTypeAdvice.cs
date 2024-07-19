// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
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

    public IntroduceNamedTypeAdvice( AdviceConstructorParameters<INamespaceOrNamedType> parameters, string? explicitName, Action<NamedTypeBuilder>? buildAction )
        : base( parameters, buildAction )
    {
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
        var existingType = targetDeclaration.Types.OfName( this.Builder.Name ).FirstOrDefault( t => this.Builder.TypeParameters.Count == t.TypeParameters.Count );

        if ( existingType == null )
        {
            addTransformation( this.Builder.ToTransformation() );

            return this.CreateSuccessResult( AdviceOutcome.Default, this.Builder );
        }
        else
        {
            return this.CreateFailedResult(
                AdviceDiagnosticDescriptors.CannotIntroduceNewTypeWhenItAlreadyExists.CreateRoslynDiagnostic(
                    targetDeclaration.GetDiagnosticLocation(),
                    (this.AspectInstance.AspectClass.ShortName, this.Builder, targetDeclaration),
                    this ) );
        }
    }
}