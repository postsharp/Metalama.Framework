class TargetCode
    {
        [Aspect]
        public IEnumerable<int> Enumerable(int a)
{
    global::System.Console.WriteLine("Begin Enumerable");
    var x = this.__Enumerable__OriginalImpl(a);
    return (System.Collections.Generic.IEnumerable<int>)x;
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
    global::System.Console.WriteLine("Begin Enumerator");
    return this.__Enumerator__OriginalImpl(a);
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
    global::System.Console.WriteLine("Begin OldEnumerable");
    var x = this.__OldEnumerable__OriginalImpl(a);
    return (System.Collections.IEnumerable)x;
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
    global::System.Console.WriteLine("Begin OldEnumerator");
    return this.__OldEnumerator__OriginalImpl(a);
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
