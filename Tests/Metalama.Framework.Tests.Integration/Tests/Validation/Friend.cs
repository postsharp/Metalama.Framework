using Metalama.Framework.Validation;

namespace Metalama.Framework.Tests.Integration.Validation.Friend
{
    [Friend( typeof(FriendClass) )]
    public class ValidatedClass { }

    internal class FriendClass
    {
        private ValidatedClass _f;
    }

    internal class NotFriendClass
    {
        private ValidatedClass _f;
    }
}