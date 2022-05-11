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
            : base( aspect, templateInstance, targetDeclaration, layerName, ObjectReader.Empty )
        {
        }


        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {

        }

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


        public bool TryExecuteTemplates( IDeclaration targetDeclaration, in MemberIntroductionContext context, FilterDirection kind, [NotNullWhen( true )] out IReadOnlyList<BlockSyntax>? filterBodies )
        {
            var success = true;

            List<BlockSyntax>? list = null;

            foreach ( var filter in this.Filters )
            {
                if ( (kind == FilterDirection.Input && filter.Kind == FilterDirection.Output) ||
                     (kind == FilterDirection.Output && filter.Kind == FilterDirection.Input) )
                {
                    continue;
                }

                list ??= new List<BlockSyntax>( this.Filters.Count );

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
                    targetDeclaration,
                    metaApiProperties );


                var boundTemplate = filter.Template.ForFilter();

                var expansionContext = new TemplateExpansionContext(
                    this.TemplateInstance.Instance,
                    metaApi,
                    (CompilationModel) targetDeclaration.Compilation,
                    context.LexicalScopeProvider.GetLexicalScope( targetDeclaration ),
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
                    list.Add( filterBody );
                }
            }

            filterBodies = list ?? (IReadOnlyList<BlockSyntax>) Array.Empty<BlockSyntax>();
            return success;
        }


    }
}