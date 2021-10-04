class TargetCode
    {
 
        [Log]
        [Cache]
        static int Add(int a, int b)
{
    global::System.Console.WriteLine("TargetCode.Add(int, int) started.");
    try
    {
global::System.Int32 result_1;
    string cacheKey = string.Format("Caravela.Framework.IntegrationTests.Aspects.Overrides.Composition.LogAndCache.TargetCode.Add({0}, {1})", new object[]{a, b});
    if (global::Caravela.Framework.IntegrationTests.Aspects.Overrides.Composition.LogAndCache.SampleCache.Cache.TryGetValue(cacheKey, out object? value))
    {
        global::System.Console.WriteLine("Cache hit.");
result_1=(global::System.Int32)value;
goto __aspect_return_1;    }
    else
    {
        global::System.Console.WriteLine("Cache miss.");
global::System.Int32 result;
            Console.WriteLine("Thinking...");
result=a + b;
goto __aspect_return_2;
__aspect_return_2:        global::Caravela.Framework.IntegrationTests.Aspects.Overrides.Composition.LogAndCache.SampleCache.Cache.TryAdd(cacheKey, result);
result_1=(global::System.Int32)result;
goto __aspect_return_1;    }

__aspect_return_1:        global::System.Console.WriteLine("TargetCode.Add(int, int) succeeded.");
        return (global::System.Int32)result_1;
    }
    catch (global::System.Exception e)
    {
        global::System.Console.WriteLine("TargetCode.Add(int, int) failed: " + e.Message);
        throw;
    }
}
    }