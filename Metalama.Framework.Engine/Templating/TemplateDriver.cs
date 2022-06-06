// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Metalama.Framework.Engine.Templating
{
    internal class TemplateDriver
    {
        private readonly UserCodeInvoker _userCodeInvoker;
        private readonly MethodInfo _templateMethod;

        public TemplateDriver(
            IServiceProvider serviceProvider,
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

            using ( meta.WithImplementation( templateExpansionContext.MetaApi ) )
            {
                if ( !this._userCodeInvoker.TryInvoke(
                        () => (SyntaxNode) this._templateMethod.Invoke( templateExpansionContext.TemplateInstance, templateArguments ),
                        templateExpansionContext,
                        out var output ) )
                {
                    block = null;

                    return false;
                }

                var errorCountAfter = templateExpansionContext.DiagnosticSink.ErrorCount;

                block = (BlockSyntax) new FlattenBlocksRewriter().Visit( output! );

                block = block.NormalizeWhitespace();

                // We add generated-code annotations to the statements and not to the block itself so that the brackets don't get colored.
                var aspectClass = templateExpansionContext.MetaApi.AspectInstance?.AspectClass;
                block = block.WithGeneratedCodeAnnotation( aspectClass?.GeneratedCodeAnnotation ?? FormattingAnnotations.SystemGeneratedCodeAnnotation );

                return errorCountAfter == errorCountBefore;
            }
        }
    }
}