// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

internal sealed class ContractAdvice : Advice
{
    private readonly ContractDirection _direction;
    private readonly TemplateMember<IMethod> _template;
    private readonly IObjectReader _tags;
    private readonly IObjectReader _templateArguments;

    public ContractAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance templateInstance,
        IDeclaration targetDeclaration,
        ICompilation sourceCompilation,
        TemplateMember<IMethod> template,
        ContractDirection direction,
        string? layerName,
        IObjectReader tags,
        IObjectReader templateArguments )
        : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName )
    {
        Invariant.Assert( direction is ContractDirection.Input or ContractDirection.Output or ContractDirection.Both );

        this._direction = direction;
        this._template = template;
        this._tags = tags;
        this._templateArguments = templateArguments;
    }

    public override AdviceKind AdviceKind => AdviceKind.AddContract;

    public override AdviceImplementationResult Implement(
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
                    new ContractPropertyTransformation( this, promotedField, this._direction, this._template, this._templateArguments, this._tags ) );

                return AdviceImplementationResult.Success( promotedField );

            case IProperty property:
                addTransformation( new ContractPropertyTransformation( this, property, this._direction, this._template, this._templateArguments, this._tags ) );

                return AdviceImplementationResult.Success( property );

            case IIndexer indexer:
                addTransformation(
                    new ContractIndexerTransformation( this, indexer, null, this._direction, this._template, this._templateArguments, this._tags ) );

                return AdviceImplementationResult.Success( indexer );

            case IParameter { ContainingDeclaration: IIndexer indexer } parameter:
                addTransformation(
                    new ContractIndexerTransformation( this, indexer, parameter, this._direction, this._template, this._templateArguments, this._tags ) );

                return AdviceImplementationResult.Success( indexer );

            case IParameter { ContainingDeclaration: IMethod method } parameter:
                addTransformation(
                    new ContractMethodTransformation( this, method, parameter, this._direction, this._template, this._templateArguments, this._tags ) );

                return AdviceImplementationResult.Success( method );

            case IParameter { ContainingDeclaration: IConstructor constructor } parameter:
                addTransformation(
                    new ContractConstructorTransformation(
                        this,
                        constructor,
                        parameter,
                        this._direction,
                        this._template,
                        this._templateArguments,
                        this._tags ) );

                return AdviceImplementationResult.Success( constructor );

            default:
                throw new AssertionFailedException( $"Unexpected kind of declaration: '{targetDeclaration}'." );
        }
    }
}