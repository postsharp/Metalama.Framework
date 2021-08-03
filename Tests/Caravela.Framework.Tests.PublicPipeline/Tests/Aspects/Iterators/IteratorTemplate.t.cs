class TargetCode
    {
        [Aspect]
        public IEnumerable<int> Enumerable(int a)
{
    global::System.Console.WriteLine($"Starting Enumerable");
    foreach (var item in this.__Enumerable__OriginalImpl(a))
    {
        global::System.Console.WriteLine($" Intercepting {item}");
        yield return item;
    }

    global::System.Console.WriteLine($"Ending Enumerable");
}

private IEnumerable<int> __Enumerable__OriginalImpl(int a)
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            yield return 3;
        }
        
        [Aspect]
        public IEnumerator<int> Enumerator(int a)
{
    global::System.Console.WriteLine($"Starting Enumerator");
    var enumerator = this.__Enumerator__OriginalImpl(a);
    while (enumerator.MoveNext())
    {
        global::System.Console.WriteLine($" Intercepting {enumerator.Current}");
        yield return enumerator.Current;
    }

    global::System.Console.WriteLine($"Ending Enumerator");
}

private IEnumerator<int> __Enumerator__OriginalImpl(int a)
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            yield return 3;
        }
        
        [Aspect]
        public IEnumerable OldEnumerable(int a)
{
    global::System.Console.WriteLine($"Starting OldEnumerable");
    foreach (var item in this.__OldEnumerable__OriginalImpl(a))
    {
        global::System.Console.WriteLine($" Intercepting {item}");
        yield return item;
    }

    global::System.Console.WriteLine($"Ending OldEnumerable");
}

private IEnumerable __OldEnumerable__OriginalImpl(int a)
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            yield return 3;
        }
        
          [Aspect]
        public IEnumerator OldEnumerator(int a)
{
    global::System.Console.WriteLine($"Starting OldEnumerator");
    var enumerator = this.__OldEnumerator__OriginalImpl(a);
    while (enumerator.MoveNext())
    {
        global::System.Console.WriteLine($" Intercepting {enumerator.Current}");
        yield return enumerator.Current;
    }

    global::System.Console.WriteLine($"Ending OldEnumerator");
}

private IEnumerator __OldEnumerator__OriginalImpl(int a)
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            yield return 3;
        }
      
    }
