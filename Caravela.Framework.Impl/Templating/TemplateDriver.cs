// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    internal class TemplateDriver
    {
        private readonly MethodInfo _templateMethod;

        public TemplateDriver( MethodInfo templateMethodInfo )
        {
            this._templateMethod = templateMethodInfo ?? throw new ArgumentNullException( nameof( templateMethodInfo ) );
        }

        public BlockSyntax ExpandDeclaration( TemplateExpansionContext templateExpansionContext )
        {
            Invariant.Assert(
                templateExpansionContext.DiagnosticSink.DefaultLocation != null );

            // TODO: support target declaration other than a method.
            if ( templateExpansionContext.TargetDeclaration is not IMethod )
            {
                throw new NotImplementedException();
            }

            var targetMethod = (IMethod) templateExpansionContext.TargetDeclaration;
            var templateContext = new TemplateContextImpl( targetMethod, targetMethod.DeclaringType!, templateExpansionContext.Compilation, templateExpansionContext.DiagnosticSink );

            TemplateContext.Initialize( templateContext, templateExpansionContext.ProceedImplementation );
            TemplateSyntaxFactory.Initialize( templateExpansionContext );

            SyntaxNode output;
            using ( DiagnosticContext.WithDefaultLocation( templateExpansionContext.DiagnosticSink.DefaultLocation ) )
            {
                try
                {
                    output = (SyntaxNode) this._templateMethod.Invoke( templateExpansionContext.TemplateInstance, Array.Empty<object>() );
                }
                catch ( TargetInvocationException ex ) when ( ex.InnerException != null )
                {
                    ExceptionDispatchInfo.Capture( ex.InnerException ).Throw();
                    throw new AssertionFailedException( "this line is unreachable, but is necessary to make the compiler happy" );
                }
            }

            var result = (BlockSyntax) new FlattenBlocksRewriter().Visit( output );

            TemplateContext.Close();
            TemplateSyntaxFactory.Close();

            return result;
        }
    }
}
