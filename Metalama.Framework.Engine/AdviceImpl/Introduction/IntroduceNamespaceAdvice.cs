// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceNamespaceAdvice : IntroduceDeclarationAdvice<INamespace, NamespaceBuilder>
{
    public override AdviceKind AdviceKind => AdviceKind.IntroduceNamespace;

    public IntroduceNamespaceAdvice(
        AdviceConstructorParameters<INamespace> parameters,
        string name ) : base( parameters, null )
    {
        this.Builder = new NamespaceBuilder( this, parameters.TargetDeclaration.AssertNotNull(), name );
    }

    protected override IntroductionAdviceResult<INamespace> Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        var targetDeclaration = this.TargetDeclaration.As<INamespace>().GetTarget( compilation );
        var existingNamespace = targetDeclaration.Namespaces.OfName( this.Builder.Name );

        if ( existingNamespace == null )
        {
            addTransformation( this.Builder.ToTransformation() );

            return this.CreateSuccessResult( AdviceOutcome.Default, this.Builder );
        }
        else
        {
            return this.CreateSuccessResult( AdviceOutcome.Ignore, existingNamespace );
        }
    }
}