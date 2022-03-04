// @DesignTime

using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Initializers.DesignTime
{
   class IdAttribute : TypeAspect
    {
        // The initializer should NOT make it to the partial class.
        [Introduce]
        public Guid Id { get; } = Guid.NewGuid();
    }


    // <target>
    [Id]
    partial class TargetCode
    {
    }
}