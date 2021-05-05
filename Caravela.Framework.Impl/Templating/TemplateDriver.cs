// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating
{
    internal class TemplateDriver
    {
        private readonly ISymbol _sourceTemplateSymbol;
        private readonly MethodInfo _templateMethod;

        public TemplateDriver( ISymbol sourceTemplateSymbol, MethodInfo compiledTemplateMethodInfo )
        {
            this._sourceTemplateSymbol = sourceTemplateSymbol;
            this._templateMethod = compiledTemplateMethodInfo ?? throw new ArgumentNullException( nameof(compiledTemplateMethodInfo) );
        }

        public bool TryExpandDeclaration(
            TemplateExpansionContext templateExpansionContext,
            IDiagnosticAdder diagnosticAdder,
            [NotNullWhen( true )] out BlockSyntax? block )
        {
            Invariant.Assert( templateExpansionContext.DiagnosticSink.DefaultScope != null );

            // TODO: support target declaration other than a method.
            if ( templateExpansionContext.TargetDeclaration is not IMethod targetMethod )
            {
                throw new NotImplementedException();
            }

            var templateContext = new TemplateContextImpl(
                targetMethod,
                targetMethod.DeclaringType!,
                templateExpansionContext.Compilation,
                templateExpansionContext.DiagnosticSink );

            using ( TemplateSyntaxFactory.WithContext( templateExpansionContext ) )
            using ( TemplateContext.WithContext( templateContext, templateExpansionContext.ProceedImplementation ) )
            {
                SyntaxNode output;

                using ( DiagnosticContext.WithDefaultLocation( templateExpansionContext.DiagnosticSink.DefaultScope.DiagnosticLocation ) )
                {
                    try
                    {
                        output = (SyntaxNode) this._templateMethod.Invoke( templateExpansionContext.TemplateInstance, Array.Empty<object>() );
                    }
                    catch ( TargetInvocationException ex ) when ( ex.InnerException != null )
                    {
                        // The most probably reason we could have a exception here is that the user template has an error.

                        diagnosticAdder.ReportDiagnostic(
                            TemplatingDiagnosticDescriptors.ExceptionInTemplate.CreateDiagnostic(
                                this._sourceTemplateSymbol.GetDiagnosticLocation() ?? Location.None,
                                (this._sourceTemplateSymbol, templateExpansionContext.TargetDeclaration, ex.InnerException.GetType().Name,
                                 ex.InnerException.ToString()) ) );

                        block = null;

                        return false;
                    }
                }

                block = (BlockSyntax) new FlattenBlocksRewriter().Visit( output );

                return true;
            }
        }
    }
}