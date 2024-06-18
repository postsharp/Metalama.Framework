#if TEST_OPTIONS
// @DesignTime
#endif

using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.DesignTime.LocationInitializer
{
    internal class IdAttribute : TypeAspect
    {
        // Initializers should NOT make it into the partial class.

        [Introduce]
        public Guid Property { get; } = Guid.NewGuid();

        [Introduce]
        public Guid Field = Guid.NewGuid();
    }

    // <target>
    [Id]
    internal partial class TargetCode { }
}