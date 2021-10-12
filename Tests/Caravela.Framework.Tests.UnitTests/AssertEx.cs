// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using System;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests
{
    public static class AssertEx
    {
        public static void DynamicEquals( object expression, string expected )
        {
            var meta = (IUserExpression) expression;
            var actual = meta.ToRunTimeExpression().Syntax.NormalizeWhitespace().ToString();

            Assert.Equal( expected, actual );
        }

        public static void DynamicThrows<T>( Func<object?> func )
            where T : Exception
            => Assert.Throws<T>( () => ((IUserExpression) func()!).ToRunTimeExpression().Syntax );

        public static void DynamicThrows<T>( object expression )
            where T : Exception
        {
            var meta = (IUserExpression) expression;
            Assert.Throws<T>( () => meta.ToRunTimeExpression().Syntax );
        }

        internal static void ThrowsWithDiagnostic( IDiagnosticDefinition diagnosticDefinition, Func<object?> testCode )
        {
            try
            {
                var runtimeExpression = (IUserExpression) testCode()!;
                _ = runtimeExpression.ToRunTimeExpression().Syntax;

                Assert.False( true, "Exception InvalidUserCodeException was not received." );
            }
            catch ( InvalidUserCodeException e )
            {
                // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
                Assert.Contains( e.Diagnostics, d => d.Id == diagnosticDefinition.Id );
            }
        }
    }
}