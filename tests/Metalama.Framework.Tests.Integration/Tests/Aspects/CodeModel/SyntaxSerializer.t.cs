internal class TargetClass
    {
        public TargetClass( int x ) { }

        public int Field;
        public int Property { get; set; }
        
        [Override]
        public void TargetMethod_Void(int x)
{
    var methodInfo = ((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetMethod("TargetMethod_Void").MethodHandle)!);
    var methodBase = ((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetMethod("TargetMethod_Void").MethodHandle)!);
    var memberInfo = ((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetMethod("TargetMethod_Void").MethodHandle)!);
    var parameterInfo = global::System.Reflection.MethodBase.GetMethodFromHandle(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetMethod("TargetMethod_Void").MethodHandle)!.GetParameters()[0];
    var returnValueInfo = ((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetMethod("TargetMethod_Void").MethodHandle)!).ReturnParameter;
    var type = typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass);
    var field = new global::Metalama.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetField("Field", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance));
    var propertyAsFieldOrProperty = new global::Metalama.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetProperty("Property", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance));
    var property = typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetProperty("Property", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance);
    var constructor = ((global::System.Reflection.ConstructorInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetMethod(".ctor").MethodHandle)!);
    var constructorParameter = global::System.Reflection.MethodBase.GetMethodFromHandle(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetMethod(".ctor").MethodHandle)!.GetParameters()[0];
    var array = new global::System.Reflection.MemberInfo[]{((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetMethod("TargetMethod_Void").MethodHandle)!), typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass)};
    return;
}

        public int TargetMethod_Int(int x)
        {
           return 0;
        }
    }