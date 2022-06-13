// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Advices
{
    internal class ContractAdvice : Advice
    {
        public ContractAdvice( IAspectInstanceInternal aspect, TemplateClassInstance templateInstance, IDeclaration targetDeclaration, string? layerName )
            : base( aspect, templateInstance, targetDeclaration, layerName ) { }

        public override AdviceImplementationResult Implement( IServiceProvider serviceProvider, ICompilation compilation )
        {
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            switch ( targetDeclaration )
            {
                case IMethod method:
                    return AdviceImplementationResult.Create( new FilterMethodTransformation( this, method ) );

                case IProperty property:
                    return AdviceImplementationResult.Create( new FilterPropertyTransformation( this, property ) );

                case IField field:
                    var promotedField = new PromotedField( serviceProvider, this, field, ObjectReader.Empty );

                    return AdviceImplementationResult.Create(
                        promotedField,
                        new FilterPropertyTransformation( this, promotedField ) );

                default:
                    throw new AssertionFailedException();
            }
        }

        public List<Contract> Contracts { get; } = new();

        public bool TryExecuteTemplates(
            IDeclaration targetMember,
            in MemberIntroductionContext context,
            ContractDirection direction,
            string? returnValueLocalName,
            [NotNullWhen( true )] out List<StatementSyntax>? statements )
        {
            statements = null;

            foreach ( var filter in this.Contracts )
            {
                if ( !filter.AppliesTo( direction ) )
                {
                    continue;
                }

                var filterTarget = filter.TargetDeclaration.GetTarget( targetMember.Compilation );

                var parameterName = filterTarget switch
                {
                    IParameter { IsReturnParameter: true } => returnValueLocalName.AssertNotNull(),
                    IParameter parameter => parameter.Name,
                    IFieldOrPropertyOrIndexer when direction == ContractDirection.Input => "value",
                    IFieldOrPropertyOrIndexer when direction == ContractDirection.Output => returnValueLocalName.AssertNotNull(),
                    _ => throw new AssertionFailedException()
                };

                statements ??= new List<StatementSyntax>();

                var metaApiProperties = new MetaApiProperties(
                    context.DiagnosticSink,
                    filter.Template.Cast(),
                    filter.Tags,
                    this.AspectLayerId,
                    context.SyntaxGenerationContext,
                    this.Aspect,
                    context.ServiceProvider,
                    MetaApiStaticity.Default );

                var metaApi = MetaApi.ForDeclaration(
                    filterTarget,
                    metaApiProperties,
                    direction );

                var boundTemplate = filter.Template.ForContract( parameterName, filter.TemplateArguments );

                var expansionContext = new TemplateExpansionContext(
                    this.TemplateInstance.Instance,
                    metaApi,
                    (CompilationModel) targetMember.Compilation,
                    context.LexicalScopeProvider.GetLexicalScope( targetMember ),
                    context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                    context.SyntaxGenerationContext,
                    boundTemplate,
                    null,
                    this.AspectLayerId );

                var templateDriver = this.TemplateInstance.TemplateClass.GetTemplateDriver( filter.Template.Declaration! );

                if ( !templateDriver.TryExpandDeclaration( expansionContext, boundTemplate.TemplateArguments, out var filterBody ) )
                {
                    statements = null;

                    return false;
                }
                else
                {
                    statements.AddRange( filterBody.Statements );
                }
            }

            return statements is { Count: > 0 };
        }
    }
}