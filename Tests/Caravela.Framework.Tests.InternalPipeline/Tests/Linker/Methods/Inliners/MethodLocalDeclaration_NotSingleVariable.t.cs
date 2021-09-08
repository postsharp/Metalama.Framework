// Warning CS0219 on `y`: `The variable 'y' is assigned but its value is never used`
class Target
    {
        int Foo()
{
    Console.WriteLine("Before");
    int y = 0, x = this.Foo_Source();
    Console.WriteLine("After");
    return x;
}
    
private int Foo_Source()
        {
            Console.WriteLine( "Original");
            return 42;
        }
    }