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
    public ContractDirection Direction { get; }

    public TemplateMember<IMethod> Template { get; }

    public IObjectReader Tags { get; }

    public IObjectReader TemplateArguments { get; }

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

        this.Direction = direction;
        this.Template = template;
        this.Tags = tags;
        this.TemplateArguments = templateArguments;
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
                    new ContractPropertyTransformation( this, promotedField, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return AdviceImplementationResult.Success( promotedField );

            case IProperty property:
                addTransformation( new ContractPropertyTransformation( this, property, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return AdviceImplementationResult.Success( property );

            case IIndexer indexer:
                addTransformation( new ContractIndexerTransformation( this, indexer, null, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return AdviceImplementationResult.Success( indexer );

            case IParameter { ContainingDeclaration: IIndexer indexer } parameter:
                addTransformation(
                    new ContractIndexerTransformation( this, indexer, parameter, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return AdviceImplementationResult.Success( indexer );

            case IParameter { ContainingDeclaration: IMethod method } parameter:
                addTransformation(
                    new ContractMethodTransformation( this, method, parameter, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return AdviceImplementationResult.Success( method );

            case IParameter { ContainingDeclaration: IConstructor constructor } parameter:
                addTransformation(
                    new ContractConstructorTransformation( this, constructor, parameter, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return AdviceImplementationResult.Success( constructor );

            default:
                throw new AssertionFailedException( $"Unexpected kind of declaration: '{targetDeclaration}'." );
        }
    }
}