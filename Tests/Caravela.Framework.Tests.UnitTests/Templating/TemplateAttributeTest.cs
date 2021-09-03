// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Templating
{
    public class TemplateAttributeTest
    {
        [Fact]
        public void CannotReadUnsetProperties()
        {
            TemplateAttribute attribute = new();

            Assert.Throws<InvalidOperationException>( () => attribute.Accessibility );
            attribute.Accessibility = Accessibility.Internal;
            Assert.Equal( Accessibility.Internal, attribute.Accessibility );

            Assert.Throws<InvalidOperationException>( () => attribute.IsVirtual );
            attribute.IsVirtual = true;
            Assert.True( attribute.IsVirtual );

            Assert.Throws<InvalidOperationException>( () => attribute.IsSealed );
            attribute.IsSealed = true;
            Assert.True( attribute.IsSealed );
        }
    }
}