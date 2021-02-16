using System;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests
{
    public static class AssertEx
    {
        public static void DynamicEquals( dynamic expression, string expected )
        {
            _ = expression;
            var meta = (IDynamicMember) expression;
            var actual = meta.CreateExpression().Syntax.NormalizeWhitespace().ToString();

            Assert.Equal( expected, actual );
        }

        public static void ThrowsWithDiagnostic( DiagnosticDescriptor diagnosticDescriptor, Action testCode )
        {
            try
            {
                testCode();

                Assert.False( true, "Exception CaravelaException was not received." );
            }
            catch ( CaravelaException e )
            {
                Assert.Equal( diagnosticDescriptor.Id, e.Diagnostic.Id );
            }
        }

    }
}