// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Diagnostics
{
    public class DiagnosticFormatterTests
    {
        [Theory]
        [InlineData( 1, "1" )]
        [InlineData( DeclarationKind.Attribute, "attribute" )]
        [InlineData( DeclarationKind.TypeParameter, "generic parameter" )]
        [InlineData( DeclarationKind.ManagedResource, "managed resource" )]
        [InlineData( DeclarationKind.AssemblyReference, "assembly reference" )]
        [InlineData( Accessibility.Private, "private" )]
        [InlineData( Accessibility.Internal, "internal" )]
        [InlineData( Accessibility.Protected, "protected" )]
        [InlineData( Accessibility.Public, "public" )]
        [InlineData( Accessibility.PrivateProtected, "private protected" )]
        [InlineData( Accessibility.ProtectedInternal, "protected internal" )]
        [InlineData( new[] { "a", "b" }, "'a', 'b'" )]
        [InlineData( new[] { 1, 2 }, "1, 2" )]
        public void Format( object value, string expected )
        {
            var formatter = new UserMessageFormatter();
            Assert.Equal( expected, formatter.Format( "", value, formatter ) );
        }
    }
}