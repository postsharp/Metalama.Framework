[Aspect]
    unsafe class TargetCode
    {
        void SystemTypesOnly( dynamic dyn, dynamic[] dynArray, List<dynamic> dynGeneric ) {}


public void Run()
{
    var type = typeof(object);
    var type_1 = typeof(object[]);
    var type_2 = typeof(global::System.Collections.Generic.List<object>);
}    }
