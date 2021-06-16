// <target>
[Introduction]
internal class TargetClass : BaseClass
{
    public int ExistingMethod()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return 27;
    }
    public static int ExistingMethod_Static()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return 27;
    }


    public override global::System.Int32 BaseMethod()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return base.BaseMethod();
    }

    public static override global::System.Int32 BaseMethod_Static()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return global::Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictOverride.BaseClass.BaseMethod_Static();
    }

    public global::System.Int32 NonExistingMethod()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return default(global::System.Int32);
    }

    public static global::System.Int32 NonExistingMethod_Static()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return default(global::System.Int32);
    }
}