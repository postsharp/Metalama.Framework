[Aspect]
unsafe class TargetCode
{
    void SystemTypesOnly( dynamic dyn, dynamic[] dynArray, List<dynamic> dynGeneric ) {}


    public void Run()
    {
        var type = typeof(global::System.Object);
        var type_1 = typeof(global::System.Object[]);
        var type_2 = typeof(global::System.Collections.Generic.List<global::System.Object>);
    }    }