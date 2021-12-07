// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.DesignTime.Diagnostics;
using System.Globalization;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Diagnostics
{
    public class DiagnosticTests
    {
        [Fact]
        public void StandardDiagnosticDescriptors()
        {
            var args = new object[32];

            // This should at least test that there is no duplicate.
            _ = DesignTimeDiagnosticDefinitions.StandardDiagnosticDescriptors;

            // Test that the formatting strings are valid.
            foreach ( var descriptor in DesignTimeDiagnosticDefinitions.StandardDiagnosticDescriptors.Values )
            {
                var formattingString = descriptor.MessageFormat.ToString( CultureInfo.InvariantCulture );
                _ = string.Format( CultureInfo.InvariantCulture, formattingString, args );
            }
        }
    }
}