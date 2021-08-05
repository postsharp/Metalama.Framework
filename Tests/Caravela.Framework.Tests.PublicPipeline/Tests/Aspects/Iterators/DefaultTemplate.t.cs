class TargetCode
    {
        [Aspect]
        public IEnumerable<int> Enumerable(int a)
{
    global::System.Console.WriteLine($"Before Enumerable");
    var result = global::Caravela.Framework.RunTime.RunTimeAspectHelper.Buffer(this.Enumerable_Source(a));
    global::System.Console.WriteLine($"After Enumerable");
    return (System.Collections.Generic.IEnumerable<int>)result;
}

private IEnumerable<int> Enumerable_Source(int a)
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
    var result = global::Caravela.Framework.RunTime.RunTimeAspectHelper.Buffer(this.Enumerator_Source(a));
    global::System.Console.WriteLine($"After Enumerator");
    return (System.Collections.Generic.IEnumerator<int>)result;
}

private IEnumerator<int> Enumerator_Source(int a)
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
    var result = global::Caravela.Framework.RunTime.RunTimeAspectHelper.Buffer(this.OldEnumerable_Source(a));
    global::System.Console.WriteLine($"After OldEnumerable");
    return (System.Collections.IEnumerable)result;
}

private IEnumerable OldEnumerable_Source(int a)
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
    var result = global::Caravela.Framework.RunTime.RunTimeAspectHelper.Buffer(this.OldEnumerator_Source(a));
    global::System.Console.WriteLine($"After OldEnumerator");
    return (System.Collections.IEnumerator)result;
}

private IEnumerator OldEnumerator_Source(int a)
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            yield return 3;
        }
      
    }