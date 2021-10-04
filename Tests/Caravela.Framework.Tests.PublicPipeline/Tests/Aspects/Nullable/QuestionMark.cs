using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Nullable.QuestionMark
{
    internal class Aspect : Attribute, IAspect<INamedType>
    {
        [Introduce]
        private string? Introduced( string? a ) => a?.ToString();
    }

    // <target>
    internal class TargetCode
    {
        [Aspect]
        private class NullableTarget { }

#nullable disable

        [Aspect]
        private class NonNullableTarget { }
    }
}