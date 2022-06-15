[Override]
internal class BaseClass
{
    ~BaseClass()
    {
        global::System.Console.WriteLine("This is the override.");
        this.Finalize_Source();
        return;
    }

    private void Finalize_Source()
    {
        Console.WriteLine($"This is the original finalizer.");
    }
}

[Override]
internal class DerivedClass : BaseClass
{


    ~DerivedClass()
    {
        global::System.Console.WriteLine("This is the override.");
        return;
    }
}