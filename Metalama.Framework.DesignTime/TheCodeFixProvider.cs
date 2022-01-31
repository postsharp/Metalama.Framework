// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
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
    public class TheCodeFixProvider : CodeFixProvider
    {
        private const string _makePartialKey = "Metalama.MakePartial";
        private readonly DesignTimeDiagnosticDefinitions _designTimeDiagnosticDefinitions = DesignTimeDiagnosticDefinitions.GetInstance();

        private readonly ILogger _logger;
        private readonly ICodeActionExecutionService _codeActionExecutionService;

        public TheCodeFixProvider() : this( DesignTimeServiceProviderFactory.GetServiceProvider() ) { }

        public TheCodeFixProvider( IServiceProvider serviceProvider )
        {
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "CodeFix" );
            serviceProvider.GetRequiredService<ICodeActionDiscoveryService>();
            this._codeActionExecutionService = serviceProvider.GetRequiredService<ICodeActionExecutionService>();

            this.FixableDiagnosticIds =
                ImmutableArray.Create( GeneralDiagnosticDescriptors.TypeNotPartial.Id )
                    .Add( GeneralDiagnosticDescriptors.SuggestedCodeFix.Id )
                    .AddRange( this._designTimeDiagnosticDefinitions.UserDiagnosticDescriptors.Keys );
        }

        public override Task RegisterCodeFixesAsync( CodeFixContext context )
        {
            this._logger.Trace?.Log( "DesignTimeCodeFixProvider.RegisterCodeFixesAsync" );

            var projectOptions = new ProjectOptions( context.Document.Project );

            if ( string.IsNullOrEmpty( projectOptions.ProjectId ) )
            {
                this._logger.Trace?.Log( "Not a Metalama project." );

                return Task.CompletedTask;
            }

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

                var codeFixes = ProvideCodeFixes(
                    context.Diagnostics,
                    context.CancellationToken );

                if ( codeFixes.IsDefault )
                {
                    // This means the call was not successful.
                    return Task.CompletedTask;
                }

                var invocationContext = new CodeActionInvocationContext(
                    this._codeActionExecutionService,
                    context.Document,
                    this._logger,
                    projectOptions.ProjectId );

                foreach ( var fix in codeFixes )
                {
                    foreach ( var codeAction in ((CodeActionBaseModel) fix.CodeAction).ToCodeActions( invocationContext ) )
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

        private static ImmutableArray<CodeFixModel> ProvideCodeFixes( ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken )
        {
            var codeFixesBuilder = ImmutableArray.CreateBuilder<CodeFixModel>();

            foreach ( var diagnostic in diagnostics )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( diagnostic.Properties.TryGetValue( CodeFixTitles.DiagnosticPropertyKey, out var codeFixTitles ) &&
                     !string.IsNullOrEmpty( codeFixTitles ) )
                {
                    var splitTitles = codeFixTitles!.Split( CodeFixTitles.Separator );

                    foreach ( var codeFixTitle in splitTitles )
                    {
                        // TODO: We may support hierarchical code fixes by allowing a separator in the title given by the user, i.e. '|'.
                        // The creation of the tree structure would then be done here.

                        var codeAction = new UserCodeActionModel(
                            codeFixTitle,
                            diagnostic );

                        codeFixesBuilder.Add( new CodeFixModel( codeAction, ImmutableArray.Create( diagnostic ) ) );
                    }
                }
            }

            return codeFixesBuilder.ToImmutable();
        }
    }
}