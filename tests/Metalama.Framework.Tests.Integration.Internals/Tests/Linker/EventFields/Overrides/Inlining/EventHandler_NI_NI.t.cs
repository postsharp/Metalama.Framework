class Target
{
    event EventHandler? Foo
    {
        add
        {
            this.Foo_Override += value;
        }
        remove
        {
            this.Foo_Override -= value;
        }
    }

    private event EventHandler? Foo_Source;


    event EventHandler? Foo_Override
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
}