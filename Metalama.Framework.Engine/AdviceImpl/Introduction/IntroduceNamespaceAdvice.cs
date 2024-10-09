// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceNamespaceAdvice : IntroduceDeclarationAdvice<INamespace, NamespaceBuilder>
{
    private readonly string _name;

    public override AdviceKind AdviceKind => AdviceKind.IntroduceNamespace;

    public IntroduceNamespaceAdvice(
        AdviceConstructorParameters<INamespace> parameters,
        string name ) : base( parameters, null )
    {
        this._name = name;
    }

    protected override NamespaceBuilder CreateBuilder( in AdviceImplementationContext context )
    {
        return new NamespaceBuilder( this.AspectLayerInstance, (INamespace) this.TargetDeclaration.AssertNotNull(), this._name );
    }

    protected override IntroductionAdviceResult<INamespace> ImplementCore( NamespaceBuilder builder, in AdviceImplementationContext context )
    {
        builder.Freeze();
        var targetDeclaration = (INamespace) this.TargetDeclaration;
        var existingNamespace = targetDeclaration.Namespaces.OfName( builder.Name );

        if ( existingNamespace == null )
        {
            context.AddTransformation( builder.ToTransformation() );

            return this.CreateSuccessResult( AdviceOutcome.Default, builder );
        }
        else
        {
            return this.CreateSuccessResult( AdviceOutcome.Ignore, existingNamespace );
        }
    }
}