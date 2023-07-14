// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests
{
    internal static class AssertEx
    {
        public static void DynamicEquals( object expression, string expected )
        {
            var meta = (IExpression) expression;
            var actual = meta.ToExpressionSyntax( TemplateExpansionContext.CurrentSyntaxGenerationContext ).NormalizeWhitespace().ToString();

            Assert.Equal( expected, actual );
        }

        internal static void ThrowsWithDiagnostic( IDiagnosticDefinition diagnosticDefinition, Func<object?> testCode )
        {
            try
            {
                var runtimeExpression = (IExpression) testCode()!;
                _ = runtimeExpression.ToExpressionSyntax( TemplateExpansionContext.CurrentSyntaxGenerationContext );

                Assert.Fail( "Exception InvalidUserCodeException was not received." );
            }
            catch ( DiagnosticException e )
            {
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                Assert.Contains( e.Diagnostics, d => d.Id == diagnosticDefinition.Id );
            }
        }
    }
}