#pragma warning disable CS8618, CS0169

using Metalama.Framework.Validation;

namespace Metalama.Framework.Tests.Integration.Validation.Friend
{
    [OnlyVisibleTo( typeof(FriendClass) )]
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