using System;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Caravela.Framework.Impl;

namespace Caravela.Framework.UnitTests
{
    public static class AssertEx
    {
        public static void DynamicEquals( dynamic expression, string expected )
        {
            _ = expression;
            var meta = (IDynamicMember) expression;
            var actual = meta.CreateExpression().Syntax.NormalizeWhitespace().ToString();

            Xunit.Assert.Equal( expected, actual );
        }

        public static void ThrowsWithDiagnostic( DiagnosticDescriptor diagnosticDescriptor, Action testCode )
        {
            try
            {
                testCode();

                Xunit.Assert.False( true, "Exception InvalidUserCodeException was not received." );
            }
            catch ( InvalidUserCodeException e )
            {
                Xunit.Assert.Contains( e.Diagnostics, d => d.Id == diagnosticDescriptor.Id );
            }
        }
    }
}