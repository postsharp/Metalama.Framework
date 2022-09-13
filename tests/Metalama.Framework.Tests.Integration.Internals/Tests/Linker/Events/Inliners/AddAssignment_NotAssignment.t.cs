// Warning CS8625 on `null`: `Cannot convert null literal to non-nullable reference type.`
public class Target
{
    event EventHandler? Foo
    {
        add
        {
            Console.WriteLine("Before");
            this.Foo_Source?.Invoke(null, null);
            Console.WriteLine("After");

        }
        remove
        {
        }
    }

    private event EventHandler? Foo_Source;
}