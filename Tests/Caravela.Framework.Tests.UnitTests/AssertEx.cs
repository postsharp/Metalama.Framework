// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using System;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests
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

        internal static void ThrowsWithDiagnostic( IStrongDiagnosticDescriptor diagnosticDescriptor, Action testCode )
        {
            try
            {
                testCode();

                Assert.False( true, "Exception InvalidUserCodeException was not received." );
            }
            catch ( InvalidUserCodeException e )
            {
                Assert.Contains( e.Diagnostics, d => d.Id == diagnosticDescriptor.Id );
            }
        }
    }
}