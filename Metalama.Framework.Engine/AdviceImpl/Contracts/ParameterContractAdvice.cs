// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;

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

    protected override AddContractAdviceResult<IParameter> Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

        switch ( targetDeclaration )
        {
            case IParameter { ContainingDeclaration: IIndexer indexer } parameter:
                addTransformation(
                    new ContractIndexerTransformation( this, indexer, parameter, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return CreateSuccessResult( parameter );

            case IParameter { ContainingDeclaration: IMethod method } parameter:
                addTransformation(
                    new ContractMethodTransformation( this, method, parameter, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return CreateSuccessResult( parameter );

            case IParameter { ContainingDeclaration: IConstructor constructor } parameter:
                addTransformation(
                    new ContractConstructorTransformation(
                        this,
                        constructor,
                        parameter,
                        this.Direction,
                        this.Template,
                        this.TemplateArguments,
                        this.Tags ) );

                return CreateSuccessResult( parameter );

            default:
                throw new AssertionFailedException();
        }
    }
}