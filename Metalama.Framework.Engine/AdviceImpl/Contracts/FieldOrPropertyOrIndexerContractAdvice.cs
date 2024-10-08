// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.AdviceImpl.Override;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;

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

    protected override AddContractAdviceResult<IFieldOrPropertyOrIndexer> Implement( in AdviceImplementationContext context )
    {
        var serviceProvider = context.ServiceProvider;
        var contextCopy = context;
        var targetDeclaration = this.TargetDeclaration;

        switch ( targetDeclaration )
        {
            case IProperty property:
                return AddContractToProperty( property );

            case IField { OverridingProperty: { } overridingProperty }:
                return AddContractToProperty( overridingProperty );

            case IField field:
                var promotedField = PromotedFieldBuilder.Create( serviceProvider, field, ObjectReader.Empty, this.AdviceInfo );
                promotedField.Freeze();
                context.AddTransformation( promotedField.ToTransformation() );
                OverrideHelper.AddTransformationsForStructField( field.DeclaringType, this.AdviceInfo, context.AddTransformation );

                return AddContractToProperty( promotedField );

            case IIndexer indexer:
                context.AddTransformation(
                    new ContractIndexerTransformation(
                        this.AdviceInfo,
                        indexer.ToFullRef(),
                        null,
                        this.Direction,
                        this.Template,
                        this.TemplateArguments,
                        this.Tags,
                        this.TemplateProvider ) );

                return CreateSuccessResult( indexer );

            default:
                throw new AssertionFailedException();
        }

        AddContractAdviceResult<IFieldOrPropertyOrIndexer> AddContractToProperty( IProperty property )
        {
            contextCopy.AddTransformation(
                new ContractPropertyTransformation(
                    this.AdviceInfo,
                    property.ToFullRef(),
                    this.Direction,
                    this.Template,
                    this.TemplateArguments,
                    this.Tags,
                    this.TemplateProvider ) );

            return CreateSuccessResult( property );
        }
    }
}