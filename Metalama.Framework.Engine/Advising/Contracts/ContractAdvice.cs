﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advising;

internal abstract class ContractAdvice<T> : Advice<AddContractAdviceResult<T>>
    where T : class, IDeclaration
{
    protected ContractDirection Direction { get; }

    protected TemplateMember<IMethod> Template { get; }

    protected IObjectReader Tags { get; }

    protected IObjectReader TemplateArguments { get; }

    public ContractAdvice(
        IAspectInstanceInternal aspectInstance,
        TemplateClassInstance templateInstance,
        T targetDeclaration,
        ICompilation sourceCompilation,
        TemplateMember<IMethod> template,
        ContractDirection direction,
        string? layerName,
        IObjectReader tags,
        IObjectReader templateArguments )
        : base( aspectInstance, templateInstance, targetDeclaration, sourceCompilation, layerName )
    {
        Invariant.Assert( direction is ContractDirection.Input or ContractDirection.Output or ContractDirection.Both );

        this.Direction = direction;
        this.Template = template;
        this.Tags = tags;
        this.TemplateArguments = templateArguments;
    }

    public override AdviceKind AdviceKind => AdviceKind.AddContract;

    // TODO: the conversion on the next line will not work with fields.
    protected static AddContractAdviceResult<T> CreateSuccessResult( T member ) => new( member.ToTypedRef() );
}

internal class ParameterContractAdvice : ContractAdvice<IParameter>
{
    public ParameterContractAdvice(
        IAspectInstanceInternal aspectInstance,
        TemplateClassInstance templateInstance,
        IParameter targetDeclaration,
        ICompilation sourceCompilation,
        TemplateMember<IMethod> template,
        ContractDirection direction,
        string? layerName,
        IObjectReader tags,
        IObjectReader templateArguments ) : base(
        aspectInstance,
        templateInstance,
        targetDeclaration,
        sourceCompilation,
        template,
        direction,
        layerName,
        tags,
        templateArguments ) { }

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

internal class FieldOrPropertyOrIndexerContractAdvice : ContractAdvice<IFieldOrPropertyOrIndexer>
{
    public FieldOrPropertyOrIndexerContractAdvice(
        IAspectInstanceInternal aspectInstance,
        TemplateClassInstance templateInstance,
        IFieldOrPropertyOrIndexer targetDeclaration,
        ICompilation sourceCompilation,
        TemplateMember<IMethod> template,
        ContractDirection direction,
        string? layerName,
        IObjectReader tags,
        IObjectReader templateArguments ) : base(
        aspectInstance,
        templateInstance,
        targetDeclaration,
        sourceCompilation,
        template,
        direction,
        layerName,
        tags,
        templateArguments ) { }

    protected override AddContractAdviceResult<IFieldOrPropertyOrIndexer> Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

        switch ( targetDeclaration )
        {
            case IField field:
                var promotedField = new PromotedField( serviceProvider, field, ObjectReader.Empty, this );
                addTransformation( promotedField.ToTransformation() );
                OverrideHelper.AddTransformationsForStructField( field.DeclaringType.ForCompilation( compilation ), this, addTransformation );

                addTransformation(
                    new ContractPropertyTransformation( this, promotedField, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return CreateSuccessResult( promotedField );

            case IProperty property:
                addTransformation( new ContractPropertyTransformation( this, property, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return CreateSuccessResult( property );

            case IIndexer indexer:
                addTransformation( new ContractIndexerTransformation( this, indexer, null, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return CreateSuccessResult( indexer );

            default:
                throw new AssertionFailedException();
        }
    }
}