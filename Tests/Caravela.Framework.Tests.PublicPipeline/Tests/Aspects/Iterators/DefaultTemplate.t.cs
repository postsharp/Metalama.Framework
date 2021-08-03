class TargetCode
    {
        [Aspect]
        public IEnumerable<int> Enumerable(int a)
{
    global::System.Console.WriteLine($"Before Enumerable");
    var result = global::Caravela.Framework.RunTime.RunTimeAspectHelper.Buffer(this.__Enumerable__OriginalImpl(a));
    global::System.Console.WriteLine($"After Enumerable");
    return (System.Collections.Generic.IEnumerable<int>)result;
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
    global::System.Console.WriteLine($"Before Enumerator");
    var result = global::Caravela.Framework.RunTime.RunTimeAspectHelper.Buffer(this.__Enumerator__OriginalImpl(a));
    global::System.Console.WriteLine($"After Enumerator");
    return (System.Collections.Generic.IEnumerator<int>)result;
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
    global::System.Console.WriteLine($"Before OldEnumerable");
    var result = global::Caravela.Framework.RunTime.RunTimeAspectHelper.Buffer(this.__OldEnumerable__OriginalImpl(a));
    global::System.Console.WriteLine($"After OldEnumerable");
    return (System.Collections.IEnumerable)result;
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
    global::System.Console.WriteLine($"Before OldEnumerator");
    var result = global::Caravela.Framework.RunTime.RunTimeAspectHelper.Buffer(this.__OldEnumerator__OriginalImpl(a));
    global::System.Console.WriteLine($"After OldEnumerator");
    return (System.Collections.IEnumerator)result;
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
