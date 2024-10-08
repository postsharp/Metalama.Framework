// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.AdviceImpl.Contracts;

internal sealed class ParameterContractAdvice : ContractAdvice<IParameter>
{
    public ParameterContractAdvice(
        AdviceConstructorParameters<IParameter> parameters,
        TemplateMember<IMethod> template,
        ContractDirection direction,
        IObjectReader tags,
        IObjectReader templateArguments )
        : base( parameters, template, direction, tags, templateArguments ) { }

    protected override AddContractAdviceResult<IParameter> Implement( in AdviceImplementationContext context )
    {
        var targetDeclaration = this.TargetDeclaration;

        switch ( targetDeclaration )
        {
            case IParameter { ContainingDeclaration: IIndexer indexer } parameter:
                context.AddTransformation(
                    new ContractIndexerTransformation(
                        this,
                        indexer.ToFullRef(),
                        parameter.ToRef(),
                        this.Direction,
                        this.Template,
                        this.TemplateArguments,
                        this.Tags,
                        this.TemplateProvider ) );

                return CreateSuccessResult( parameter );

            case IParameter { ContainingDeclaration: IMethod method } parameter:
                context.AddTransformation(
                    new ContractMethodTransformation(
                        this,
                        method.ToFullRef(),
                        parameter.ToRef(),
                        this.Direction,
                        this.Template,
                        this.TemplateArguments,
                        this.Tags,
                        this.TemplateProvider ) );

                return CreateSuccessResult( parameter );

            case IParameter { ContainingDeclaration: IConstructor constructor } parameter:
                context.AddTransformation(
                    new ContractConstructorTransformation(
                        this,
                        constructor.ToFullRef(),
                        parameter.ToRef(),
                        this.Direction,
                        this.Template,
                        this.TemplateArguments,
                        this.Tags,
                        this.TemplateProvider ) );

                return CreateSuccessResult( parameter );

            default:
                throw new AssertionFailedException();
        }
    }
}