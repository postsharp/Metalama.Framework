using Metalama.Framework.Validation;

#pragma warning disable CS8618

namespace Metalama.Framework.TestApp
{

    [OnlyVisibleTo(typeof(NotFriendClass))]
    [ForTestOnly]
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
