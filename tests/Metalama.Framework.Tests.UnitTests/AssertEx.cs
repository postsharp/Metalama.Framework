// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Tests.UnitTests.Utilities;
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
            var actual = meta.ToExpressionSyntax( TemplateExpansionContext.CurrentSyntaxSerializationContext ).NormalizeWhitespace().ToString();

            Assert.Equal( expected, actual );
        }

        internal static void ThrowsWithDiagnostic( IDiagnosticDefinition diagnosticDefinition, Func<object?> testCode )
        {
            try
            {
                var runtimeExpression = (IExpression) testCode()!;
                _ = runtimeExpression.ToExpressionSyntax( TemplateExpansionContext.CurrentSyntaxSerializationContext );

                Assert.Fail( "Exception InvalidUserCodeException was not received." );
            }
            catch ( DiagnosticException e )
            {
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                Assert.Contains( e.Diagnostics, d => d.Id == diagnosticDefinition.Id );
            }
        }

        public static void EolInvariantEqual( string expected, string actual ) => Assert.Equal( expected.NormalizeEndOfLines().Trim(), actual.NormalizeEndOfLines().Trim() );
    }
}