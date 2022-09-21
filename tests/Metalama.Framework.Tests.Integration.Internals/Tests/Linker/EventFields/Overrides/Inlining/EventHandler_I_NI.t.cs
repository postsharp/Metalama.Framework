class Target
{
    event EventHandler? Foo
    {
        add
        {
            Console.WriteLine("Before");
            this.Foo_Source += value;
            Console.WriteLine("After");

        }
        remove
        {
            Console.WriteLine("Before");
            this.Foo_Source -= value;
            Console.WriteLine("After");
        }
    }

    private EventHandler? Foo_Source;
}