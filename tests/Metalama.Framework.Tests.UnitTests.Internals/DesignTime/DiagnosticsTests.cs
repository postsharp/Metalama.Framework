// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Diagnostics;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
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

            var json = JsonConvert.SerializeObject( file );
            var roundtrip = JsonConvert.DeserializeObject<UserDiagnosticRegistrationFile>( json )!;

            Assert.Contains( "MY001", roundtrip.Suppressions );
            Assert.Contains( "MY001", roundtrip.Diagnostics.Keys );
        }
    }
}