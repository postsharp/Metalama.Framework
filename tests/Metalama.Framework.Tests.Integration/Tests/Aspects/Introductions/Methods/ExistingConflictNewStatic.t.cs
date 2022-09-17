[Introduction]
    internal class TargetClass : DerivedClass
    {
        // All methods in this class should contain a comment describing the correct output.

        public static int ExistingMethod()
        {
            // No new keyword, return a constant.
                return 27;
        }


public static new global::System.Int32 BaseClassMethod()
{
    // New keyword, call the base class method of the same name.
    return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNewStatic.BaseClass.BaseClassMethod();
}

public global::System.Int32 BaseClassInaccessibleMethod()
{
    // No new keyword, return a default value.
        return default(global::System.Int32);
}

public static new global::System.Int32 BaseClassMethodHiddenByMethod()
{
    // New keyword, call the derived class method of the same name.
    return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNewStatic.DerivedClass.BaseClassMethodHiddenByMethod();
}

public static new global::System.Int32 BaseClassMethodHiddenByInaccessibleMethod()
{
    // New keyword, call the base class method of the same name.
    return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNewStatic.BaseClass.BaseClassMethodHiddenByInaccessibleMethod();
}

public static new global::System.Int32 DerivedClassMethod()
{
    // New keyword, call the derived class method of the same name.
    return global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictNewStatic.DerivedClass.DerivedClassMethod();
}

public static global::System.Int32 DerivedClassInaccessibleMethod()
{
    // No new keyword, return a default value.
        return default(global::System.Int32);
}

public static global::System.Int32 NonExistentMethod()
{
    // No new keyword, return a default value.
        return default(global::System.Int32);
}    }