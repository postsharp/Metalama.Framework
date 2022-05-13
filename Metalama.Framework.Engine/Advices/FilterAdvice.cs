// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Diagnostics;
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
    internal class FilterAdvice : Advice
    {
        public FilterAdvice( IAspectInstanceInternal aspect, TemplateClassInstance templateInstance, IDeclaration targetDeclaration, string? layerName )
            : base( aspect, templateInstance, targetDeclaration, layerName, ObjectReader.Empty ) { }

        public override void Initialize( IDiagnosticAdder diagnosticAdder ) { }

        public override AdviceResult ToResult( ICompilation compilation, IReadOnlyList<IObservableTransformation> observableTransformations )
        {
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            switch ( targetDeclaration )
            {
                case IMethod method:
                    return AdviceResult.Create( new FilterMethodTransformation( this, method ) );

                case IProperty property:
                    return AdviceResult.Create( new FilterPropertyTransformation( this, property ) );
                
                case IField field:
                    var promotedField = new PromotedField( this, field, this.Tags );

                    return AdviceResult.Create(
                        promotedField,
                        new FilterPropertyTransformation( this, promotedField ) );
                
                default:
                    throw new NotImplementedException();
            }
        }

        public List<Filter> Filters { get; } = new();

        public bool TryExecuteTemplates(
            IDeclaration targetMember,
            in MemberIntroductionContext context,
            FilterDirection direction,
            string? returnValueLocalName,
            [NotNullWhen( true )] out List<StatementSyntax>? statements )
        {
            statements = null;

            foreach ( var filter in this.Filters )
            {
                if ( !filter.AppliesTo( direction ) )
                {
                    continue;
                }

                var filterTarget = filter.TargetDeclaration.GetTarget( targetMember.Compilation );

                var parameterName = filterTarget switch
                {
                    IParameter { IsReturnParameter: true } => returnValueLocalName ?? null!,
                    IParameter parameter => parameter.Name,
                    IFieldOrPropertyOrIndexer when direction == FilterDirection.Input => "value",
                    IFieldOrPropertyOrIndexer when direction == FilterDirection.Output => returnValueLocalName,
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
                    metaApiProperties );

                var boundTemplate = filter.Template.ForFilter( parameterName );

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