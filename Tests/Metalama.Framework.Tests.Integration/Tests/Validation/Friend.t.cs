// Warning MY001 on `ValidatedClass`: `'ValidatedClass' cannot be used from 'NotFriendClass' because of the [Friend] constraint.`
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
