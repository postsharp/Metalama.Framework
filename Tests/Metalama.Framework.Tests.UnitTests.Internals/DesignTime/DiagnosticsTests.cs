// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.DesignTime.Diagnostics;
using Microsoft.CodeAnalysis;
using System.IO;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime
{
    public class DiagnosticsTests
    {
        [Fact]
        public void DiagnosticUserProfileSerialization()
        {
            UserDiagnosticRegistrationFile file = new();
            var originalDiagnostic = new UserDiagnosticRegistration( "MY001", DiagnosticSeverity.Error, "Category", "Title" );
            file.Diagnostics.Add( "MY001", originalDiagnostic );
            file.Suppressions.Add( "MY001" );

            StringWriter stringWriter = new();
            file.Write( stringWriter );
            var roundtrip = UserDiagnosticRegistrationFile.ReadContent( stringWriter.ToString() );

            Assert.Contains( "MY001", roundtrip.Suppressions );
            Assert.Contains( "MY001", roundtrip.Diagnostics.Keys );
        }
    }
}