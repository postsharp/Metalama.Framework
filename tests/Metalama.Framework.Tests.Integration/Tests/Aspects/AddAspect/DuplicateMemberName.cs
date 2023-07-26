using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.DuplicateMemberName
{
    internal class Aspect1 : MethodAspect
    {
        // Error: two templates of the same name in the class.

        [Template]
        public void Template() { }

        [Template]
        public void Template( int x ) { }
    }

    internal class TargetCode
    {
        [Aspect1]
        private void M() { }
    }
}