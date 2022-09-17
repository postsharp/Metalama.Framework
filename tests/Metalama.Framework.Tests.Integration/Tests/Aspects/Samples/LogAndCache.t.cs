class TargetCode
    {
 
        [Log]
        [Cache]
        static int Add(int a, int b)
        {
            global::System.Console.WriteLine("TargetCode.Add(int, int) started.");
    try
    {
        global::System.Int32 result;
    string cacheKey = string.Format("TargetCode.Add({0}, {1})", new object[]{a, b});
    if (global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Composition.LogAndCache.SampleCache.Cache.TryGetValue(cacheKey, out object? value))
    {
        global::System.Console.WriteLine("Cache hit.");
        result = (global::System.Int32)value;
    }
    else
    {
        global::System.Console.WriteLine("Cache miss.");
        global::System.Int32 result_1;
            Console.WriteLine("Thinking...");
            result_1 = a + b;
        global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Composition.LogAndCache.SampleCache.Cache.TryAdd(cacheKey, result_1);
        result = (global::System.Int32)result_1;
    }
        global::System.Console.WriteLine("TargetCode.Add(int, int) succeeded.");
        return (global::System.Int32)result;
    }
    catch (global::System.Exception e)
    {
        global::System.Console.WriteLine("TargetCode.Add(int, int) failed: " + e.Message);
        throw;
    }
        }
    }