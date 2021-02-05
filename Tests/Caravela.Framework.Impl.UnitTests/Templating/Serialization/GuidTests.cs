using Caravela.Framework.Impl.Templating.Serialization;
using System;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{
    public class GuidTests
    {
        [Fact]
        public void TestGuid()
        {
            var serializer = new GuidSerializer();
            var guid = new Guid( new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16} );
            Assert.Equal(guid, new Guid(67305985, 1541, 2055, 9, 10, 11, 12, 13, 14, 15, 16));
            Assert.Equal("new System.Guid(67305985, 1541, 2055, 9, 10, 11, 12, 13, 14, 15, 16)", serializer.Serialize(guid).ToString());
        }
    }
}