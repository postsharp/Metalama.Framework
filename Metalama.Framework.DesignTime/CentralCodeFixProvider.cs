// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime
{
    // ReSharper disable UnusedType.Global

    [ExcludeFromCodeCoverage]
    public class CentralCodeFixProvider : CodeFixProvider
    {
        private const string _makePartialKey = "Metalama.MakePartial";
        private readonly DesignTimeDiagnosticDefinitions _designTimeDiagnosticDefinitions = DesignTimeDiagnosticDefinitions.GetInstance();

        private readonly ILogger _logger;
        private readonly DesignTimeAspectPipelineFactory _pipelineFactory;

        public CentralCodeFixProvider() : this( DesignTimeServiceProviderFactory.GetServiceProvider() ) { }

        public CentralCodeFixProvider( IServiceProvider serviceProvider )
        {
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "CodeFix" );
            this._pipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();

            this.FixableDiagnosticIds =
                ImmutableArray.Create( GeneralDiagnosticDescriptors.TypeNotPartial.Id )
                    .Add( GeneralDiagnosticDescriptors.SuggestedCodeFix.Id )
                    .AddRange( this._designTimeDiagnosticDefinitions.UserDiagnosticDescriptors.Keys );
        }

        public override Task RegisterCodeFixesAsync( CodeFixContext context )
        {
            this._logger.Trace?.Log( "DesignTimeCodeFixProvider.RegisterCodeFixesAsync" );

            if ( context.Diagnostics.Any( d => d.Id == GeneralDiagnosticDescriptors.TypeNotPartial.Id ) )
            {
                // This is a hard-coded code fix. It may need to be refactored with our framework.

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Make partial",
                        cancellationToken => GetFixedDocumentAsync( context.Document, context.Span, cancellationToken.IgnoreIfDebugging() ),
                        _makePartialKey ),
                    context.Diagnostics );
            }
            else if ( context.Diagnostics.Any( d => d.Properties.ContainsKey( CodeFixTitles.DiagnosticPropertyKey ) ) )
            {
                // We have a user diagnostics where a code fix provider was specified. We need to execute the CodeFix pipeline to gather
                // the actual code fixes.
                var projectOptions = new ProjectOptions( context.Document.Project );
                var userCodeFixProvider = new UserCodeFixProvider( this._pipelineFactory, projectOptions );

                var codeFixes = userCodeFixProvider.ProvideCodeFixes(
                    context.Document,
                    context.Diagnostics,
                    context.CancellationToken );

                if ( codeFixes.IsDefault )
                {
                    // This means the call was not successful.
                    return Task.CompletedTask;
                }

                var supportsHierarchicalItems = HostProcess.Current.Product != HostProduct.Rider;

                foreach ( var fix in codeFixes )
                {
                    foreach ( var codeAction in fix.CodeAction.ToCodeActions( supportsHierarchicalItems ) )
                    {
                        context.RegisterCodeFix( codeAction, fix.Diagnostic );
                    }
                }
            }

            return Task.CompletedTask;
        }

        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        private static async Task<Document> GetFixedDocumentAsync( Document document, TextSpan span, CancellationToken cancellationToken )
        {
            try
            {
                var syntaxRoot = await document.GetSyntaxRootAsync( cancellationToken );

                if ( syntaxRoot == null )
                {
                    return document;
                }

                var node = syntaxRoot.FindNode( span );
                var typeDeclaration = GetTypeDeclaration( node );

                if ( typeDeclaration == null )
                {
                    return document;
                }

                var newTypeDeclaration = typeDeclaration.AddModifiers( SyntaxFactory.Token( SyntaxKind.PartialKeyword ) );
                var newSyntaxRoot = syntaxRoot.ReplaceNode( typeDeclaration, newTypeDeclaration );
                var newDocument = document.WithSyntaxRoot( newSyntaxRoot );

                return newDocument;
            }
            catch ( Exception e )
            {
                DesignTimeExceptionHandler.ReportException( e );

                return document;
            }
        }

        private static BaseTypeDeclarationSyntax? GetTypeDeclaration( SyntaxNode node )
            => node switch
            {
                BaseTypeDeclarationSyntax typeDeclaration => typeDeclaration,
                { Parent: { } parent } => GetTypeDeclaration( parent ),
                _ => null
            };

        public override ImmutableArray<string> FixableDiagnosticIds { get; }
    }
}