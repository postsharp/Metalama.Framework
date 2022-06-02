public class Target : Interface
{
    [Test]
    void Interface.Foo()
    {
        global::System.Console.WriteLine("Overridden code.");
        Console.WriteLine("Original");
        return;
    }

    [Test]
    void Interface.Bar<T>()
    {
        global::System.Console.WriteLine("Overridden code.");
        Console.WriteLine("Original");
        return;
    }
}