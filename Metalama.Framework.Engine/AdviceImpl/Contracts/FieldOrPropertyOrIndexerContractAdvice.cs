// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.AdviceImpl.Override;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.AdviceImpl.Contracts;

internal sealed class FieldOrPropertyOrIndexerContractAdvice : ContractAdvice<IFieldOrPropertyOrIndexer>
{
    public FieldOrPropertyOrIndexerContractAdvice(
        AdviceConstructorParameters<IFieldOrPropertyOrIndexer> parameters,
        TemplateMember<IMethod> template,
        ContractDirection direction,
        IObjectReader tags,
        IObjectReader templateArguments )
        : base( parameters, template, direction, tags, templateArguments ) { }

    protected override AddContractAdviceResult<IFieldOrPropertyOrIndexer> Implement(
        ProjectServiceProvider serviceProvider,
        CompilationModel compilation,
        Action<ITransformation> addTransformation )
    {
        var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

        switch ( targetDeclaration )
        {
            // Properties must be evaluated prior to IField because PromotedField implements both interfaces but must be considered a property.
            case IProperty property:
                addTransformation( new ContractPropertyTransformation( this, property, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return CreateSuccessResult( property );
            
            case IField field:
                var promotedField = PromotedField.Create( serviceProvider, field, ObjectReader.Empty, this );
                addTransformation( promotedField.ToTransformation() );
                OverrideHelper.AddTransformationsForStructField( field.DeclaringType.ForCompilation( compilation ), this, addTransformation );

                addTransformation(
                    new ContractPropertyTransformation( this, promotedField, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return CreateSuccessResult( promotedField );

            case IIndexer indexer:
                addTransformation( new ContractIndexerTransformation( this, indexer, null, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return CreateSuccessResult( indexer );

            default:
                throw new AssertionFailedException();
        }
    }
}