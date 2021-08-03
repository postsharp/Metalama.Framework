class TargetCode
    {
        [Aspect]
        IEnumerable<int> Enumerable(int a)
{
    global::System.Console.WriteLine("Before");
    var result = global::Caravela.Framework.RunTime.RunTimeAspectHelper.Buffer(this.__Enumerable__OriginalImpl(a));
    global::System.Console.WriteLine("After");
    return (System.Collections.Generic.IEnumerable<int>)result;
}

private IEnumerable<int> __Enumerable__OriginalImpl(int a)
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }
        
        [Aspect]
        IEnumerator<int> Enumerator(int a)
{
    global::System.Console.WriteLine("Before");
    var result = global::Caravela.Framework.RunTime.RunTimeAspectHelper.Buffer(this.__Enumerator__OriginalImpl(a));
    global::System.Console.WriteLine("After");
    return (System.Collections.Generic.IEnumerator<int>)result;
}

private IEnumerator<int> __Enumerator__OriginalImpl(int a)
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }
        
        [Aspect]
        IEnumerable OldEnumerable(int a)
{
    global::System.Console.WriteLine("Before");
    var result = global::Caravela.Framework.RunTime.RunTimeAspectHelper.Buffer(this.__OldEnumerable__OriginalImpl(a));
    global::System.Console.WriteLine("After");
    return (System.Collections.IEnumerable)result;
}

private IEnumerable __OldEnumerable__OriginalImpl(int a)
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }
        
          [Aspect]
        IEnumerator OldEnumerator(int a)
{
    global::System.Console.WriteLine("Before");
    var result = global::Caravela.Framework.RunTime.RunTimeAspectHelper.Buffer(this.__OldEnumerator__OriginalImpl(a));
    global::System.Console.WriteLine("After");
    return (System.Collections.IEnumerator)result;
}

private IEnumerator __OldEnumerator__OriginalImpl(int a)
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }
      
    }
