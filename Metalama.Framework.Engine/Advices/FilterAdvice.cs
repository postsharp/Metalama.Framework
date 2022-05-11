// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
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
            [NotNullWhen( true )] out IReadOnlyList<StatementSyntax>? statements )
        {
            var success = true;

            List<StatementSyntax>? list = null;

            foreach ( var filter in this.Filters )
            {
                if ( (direction == FilterDirection.Input && filter.Direction == FilterDirection.Output) ||
                     (direction == FilterDirection.Output && filter.Direction == FilterDirection.Input) )
                {
                    continue;
                }

                var filterTarget = filter.TargetDeclaration.GetTarget( targetMember.Compilation );

                string? parameterName;
                bool skip;

                switch ( filterTarget )
                {
                    case IParameter { IsReturnParameter: true }:
                        if ( returnValueLocalName != null )
                        {
                            parameterName = returnValueLocalName;
                            skip = direction == FilterDirection.Input;
                        }
                        else
                        {
                            skip = true;
                            parameterName = null!;
                        }

                        break;

                    case IParameter parameter:
                        parameterName = parameter.Name;

                        skip = filter.Direction == FilterDirection.Default && ((direction == FilterDirection.Input && parameter.RefKind == RefKind.Out)
                                                                               || (direction == FilterDirection.Output && parameter.RefKind != RefKind.Out));

                        break;

                    default:
                        parameterName = "value";
                        skip = filter.Direction == FilterDirection.Default && direction == FilterDirection.Output;

                        break;
                }

                if ( skip )
                {
                    statements = Array.Empty<BlockSyntax>();

                    return true;
                }

                list ??= new List<StatementSyntax>();

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
                    success = false;
                }
                else
                {
                    list.AddRange( filterBody.Statements );
                }
            }

            statements = list ?? (IReadOnlyList<StatementSyntax>) Array.Empty<StatementSyntax>();

            return success;
        }
    }
}