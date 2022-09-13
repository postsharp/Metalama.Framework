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
            EventHandler? x = null;
            x -= this.Foo_Source;
            Console.WriteLine("After");
        }
    }

    private event EventHandler? Foo_Source;
}