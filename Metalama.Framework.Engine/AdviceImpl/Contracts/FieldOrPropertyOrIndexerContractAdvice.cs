// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.AdviceImpl.Override;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
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
            case IProperty property:
                return AddContractToProperty( property );

            case IField { OverridingProperty: { } overridingProperty }:
                return AddContractToProperty( overridingProperty );

            case IField field:
                var promotedField = PromotedFieldBuilder.Create( serviceProvider, field, ObjectReader.Empty, this );
                addTransformation( promotedField.ToTransformation() );
                OverrideHelper.AddTransformationsForStructField( field.DeclaringType.ForCompilation( compilation ), this, addTransformation );

                return AddContractToProperty( promotedField );

            case IIndexer indexer:
                addTransformation( new ContractIndexerTransformation( this, indexer, null, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

                return CreateSuccessResult( indexer );

            default:
                throw new AssertionFailedException();
        }

        AddContractAdviceResult<IFieldOrPropertyOrIndexer> AddContractToProperty( IProperty property )
        {
            addTransformation( new ContractPropertyTransformation( this, property, this.Direction, this.Template, this.TemplateArguments, this.Tags ) );

            return CreateSuccessResult( property );
        }
    }
}