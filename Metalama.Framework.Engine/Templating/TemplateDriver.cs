// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.UserCode;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Metalama.Framework.Engine.Templating
{
    internal sealed class TemplateDriver
    {
        private readonly UserCodeInvoker _userCodeInvoker;
        private readonly MethodInfo _templateMethod;

        public TemplateDriver(
            ProjectServiceProvider serviceProvider,
            MethodInfo compiledTemplateMethodInfo )
        {
            this._userCodeInvoker = serviceProvider.GetRequiredService<UserCodeInvoker>();
            this._templateMethod = compiledTemplateMethodInfo ?? throw new ArgumentNullException( nameof(compiledTemplateMethodInfo) );
        }

        public bool TryExpandDeclaration(
            TemplateExpansionContext templateExpansionContext,
            object?[] templateArguments,
            [NotNullWhen( true )] out BlockSyntax? block )
        {
            var errorCountBefore = templateExpansionContext.DiagnosticSink.ErrorCount;

            // Add the first template argument.
            var allArguments = new object?[templateArguments.Length + 1];
            allArguments[0] = templateExpansionContext.SyntaxFactory;
            templateArguments.CopyTo( allArguments, 1 );

            if ( !this._userCodeInvoker.TryInvoke(
                    () => (SyntaxNode) this._templateMethod.Invoke( templateExpansionContext.TemplateProvider.Object, allArguments ).AssertNotNull(),
                    templateExpansionContext,
                    out var output ) )
            {
                block = null;

                return false;
            }

            var errorCountAfter = templateExpansionContext.DiagnosticSink.ErrorCount;

            block = (BlockSyntax) new FlattenBlocksRewriter().Visit( output! )!;

            // If we're generating an async iterator method, but there is no yield statement, we would get an error.
            // Prevent that by adding `yield break;` at the end of the method body.
            block = templateExpansionContext.AddYieldBreakIfNecessary( block );

            block = block.NormalizeWhitespace();

            // We add generated-code annotations to the statements and not to the block itself so that the brackets don't get colored.
            var aspectClass = templateExpansionContext.MetaApi.AspectInstance?.AspectClass;
            block = block.WithGeneratedCodeAnnotation( aspectClass?.GeneratedCodeAnnotation ?? FormattingAnnotations.SystemGeneratedCodeAnnotation );

            return errorCountAfter == errorCountBefore;
        }
    }
}