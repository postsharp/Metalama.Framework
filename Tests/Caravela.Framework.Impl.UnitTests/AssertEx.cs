using System;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using System.Linq;
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

                Assert.False( true, "Exception InvalidUserCodeException was not received." );
            }
            catch ( InvalidUserCodeException e )
            {
                Assert.Contains(  e.Diagnostics, d => d.Id == diagnosticDescriptor.Id );
            }
        }
    }
}