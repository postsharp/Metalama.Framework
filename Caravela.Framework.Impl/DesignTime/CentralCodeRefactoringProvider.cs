// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.DesignTime.CodeFixes;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.DesignTime.Refactoring;
using Caravela.Framework.Impl.DesignTime.Utilities;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.DesignTime
{
    // ReSharper disable UnusedType.Global

    [ExcludeFromCodeCoverage]
    public class CentralCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync( CodeRefactoringContext context )
        {
            try
            {
                var buildOptions = new ProjectOptions( context.Document.Project );

                DebuggingHelper.AttachDebugger( buildOptions );

                if ( !context.Document.SupportsSemanticModel )
                {
                    return;
                }

                var cancellationToken = context.CancellationToken.IgnoreIfDebugging();

                var syntaxTree = await context.Document.GetSyntaxTreeAsync( cancellationToken );

                if ( syntaxTree == null )
                {
                    return;
                }

                var node = (await syntaxTree.GetRootAsync( cancellationToken )).FindNode( context.Span );

                var semanticModel = await context.Document.GetSemanticModelAsync( cancellationToken );

                if ( semanticModel == null )
                {
                    return;
                }

                var symbol = semanticModel.GetDeclaredSymbol( node, cancellationToken );

                if ( symbol == null )
                {
                    return;
                }

                // Execute the pipeline.

                var compilation = await context.Document.Project.GetCompilationAsync( cancellationToken );
                var eligibleAspects = DesignTimeAspectPipelineFactory.Instance.GetEligibleAspects( compilation!, symbol, buildOptions, cancellationToken );

                var aspectActions = ImmutableArray.CreateBuilder<CodeAction>();
                var liveTemplatesActions = ImmutableArray.CreateBuilder<CodeAction>();

                foreach ( var aspect in eligibleAspects )
                {
                    aspectActions.Add( CodeAction.Create( aspect.DisplayName, ct => AddAspectAttributeAsync( aspect, symbol, context.Document, ct ) ) );

                    if ( aspect.IsLiveTemplate )
                    {
                        liveTemplatesActions.Add(
                            CodeAction.Create(
                                aspect.DisplayName,
                                ct => ApplyLiveTemplateAsync( buildOptions, aspect, symbol, context.Document, ct.IgnoreIfDebugging() ) ) );
                    }
                }

                if ( aspectActions.Count > 0 )
                {
                    context.RegisterRefactoring( CodeAction.Create( "Add aspect", aspectActions.ToImmutable(), true ) );
                }

                if ( liveTemplatesActions.Count > 0 )
                {
                    context.RegisterRefactoring( CodeAction.Create( "Apply live template", liveTemplatesActions.ToImmutable(), false ) );
                }
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }

        private static Task<Solution> AddAspectAttributeAsync(
            AspectClass aspect,
            ISymbol targetSymbol,
            Document targetDocument,
            CancellationToken cancellationToken )
        {
            var attributeDescription = new AttributeDescription(
                AttributeRef.GetShortName( aspect.AspectType.Name ),
                imports: ImmutableList.Create( aspect.AspectType.Namespace ) );

            return CSharpAttributeHelper.AddAttributeAsync( targetDocument, targetSymbol, attributeDescription, cancellationToken ).AsTask();
        }

        private static async Task<Solution> ApplyLiveTemplateAsync(
            ProjectOptions projectOptions,
            AspectClass aspectClass,
            ISymbol targetSymbol,
            Document targetDocument,
            CancellationToken cancellationToken )
        {
            var compilation = await targetDocument.Project.GetCompilationAsync( cancellationToken );

            if ( compilation == null )
            {
                return targetDocument.Project.Solution;
            }

            if ( DesignTimeAspectPipelineFactory.Instance.TryApplyAspectToCode(
                projectOptions,
                aspectClass,
                aspectClass.CreateDefaultInstance(),
                compilation,
                targetSymbol,
                cancellationToken,
                out var outputCompilation,
                out var diagnostics ) )
            {
                var project = targetDocument.Project;

                var solution = await CodeFixHelper.ApplyChangesAsync( outputCompilation, project, cancellationToken );

                return solution;
            }
            else
            {
                // How to report errors here? We will add a comment to the target symbol.
                return await CodeFixHelper.ReportDiagnosticsAsCommentsAsync( targetSymbol, targetDocument, diagnostics, cancellationToken );
            }
        }

   
    }
}