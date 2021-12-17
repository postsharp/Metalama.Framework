using Metalama.Framework.TestApp.Aspects;
using Metalama.Framework.Validation;

namespace Metalama.Framework.TestApp
{
    partial class ClassWithInheritedAspect : IInterface
    {
        public void ManualMethod()
        {
            this.IntroducedMethod();
        }
    }


    [Friend(typeof(FriendClass))]
    class ValidatedClass
    {

    }


    class FriendClass
    {
        ValidatedClass _validatedClass;

    }

    class NotFriendClass
    {
        ValidatedClass _validatedClass;
    }
}
