#if TEST_OPTIONS
// @DesignTime
#endif

using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Aspects.Initializers.DesignTime
{
    internal class IdAttribute : TypeAspect
    {
        // The initializer should NOT make it to the partial class.
        [Introduce]
        public Guid Id { get; } = Guid.NewGuid();
    }

    // <target>
    [Id]
    internal partial class TargetCode { }
}