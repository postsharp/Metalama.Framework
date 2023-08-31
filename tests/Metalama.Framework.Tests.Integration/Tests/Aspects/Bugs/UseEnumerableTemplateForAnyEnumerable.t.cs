class EmptyOverrideFieldOrPropertyExample
{
    [Aspect]
    IEnumerable<int> M()
    {
        global::System.Console.WriteLine("enumerable");
        return new[]
        {
          42
        };
    }
}
