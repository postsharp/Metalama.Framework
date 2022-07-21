// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests
{
    public static class AssertEx
    {
        public static void DynamicEquals( object expression, string expected )
        {
            var meta = (IUserExpression) expression;
            var actual = meta.ToExpressionSyntax( TemplateExpansionContext.CurrentSyntaxGenerationContext ).NormalizeWhitespace().ToString();

            Assert.Equal( expected, actual );
        }

        public static void DynamicThrows<T>( Func<object?> func )
            where T : Exception
            => Assert.Throws<T>( () => ((IUserExpression) func()!).ToExpressionSyntax( TemplateExpansionContext.CurrentSyntaxGenerationContext ) );

        public static void DynamicThrows<T>( object expression )
            where T : Exception
        {
            var meta = (IUserExpression) expression;
            Assert.Throws<T>( () => meta.ToExpressionSyntax( TemplateExpansionContext.CurrentSyntaxGenerationContext ) );
        }

        internal static void ThrowsWithDiagnostic( IDiagnosticDefinition diagnosticDefinition, Func<object?> testCode )
        {
            try
            {
                var runtimeExpression = (IUserExpression) testCode()!;
                _ = runtimeExpression.ToExpressionSyntax( TemplateExpansionContext.CurrentSyntaxGenerationContext );

                Assert.False( true, "Exception InvalidUserCodeException was not received." );
            }
            catch ( DiagnosticException e )
            {
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                Assert.Contains( e.Diagnostics, d => d.Id == diagnosticDefinition.Id );
            }
        }
    }
}