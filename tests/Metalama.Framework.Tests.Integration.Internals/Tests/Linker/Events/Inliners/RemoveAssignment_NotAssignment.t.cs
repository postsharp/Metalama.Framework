// Warning CS8625 on `null`: `Cannot convert null literal to non-nullable reference type.`
public class Target
{
    event EventHandler? Foo
    {
        add
        {

        }
        remove
        {
            Console.WriteLine("Before");
            this.Foo_Source?.Invoke(null, null);
            Console.WriteLine("After");
        }
    }

    private event EventHandler? Foo_Source;
}