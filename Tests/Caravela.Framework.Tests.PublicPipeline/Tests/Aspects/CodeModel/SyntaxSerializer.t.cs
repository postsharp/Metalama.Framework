internal class TargetClass
    {
        public TargetClass( int x ) { }

        public int Field;
        public int Property { get; set; }
        
        [Override]
        public void TargetMethod_Void(int x)
{
    var methodInfo = ((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle("M:Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass.TargetMethod_Void(System.Int32)"))!);
    var methodBase = ((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle("M:Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass.TargetMethod_Void(System.Int32)"))!);
    var memberInfo = ((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle("M:Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass.TargetMethod_Void(System.Int32)"))!);
    var parameterInfo = global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle("M:Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass.TargetMethod_Void(System.Int32)"))!.GetParameters()[0];
    var returnValueInfo = ((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle("M:Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass.TargetMethod_Void(System.Int32)"))!).ReturnParameter;
    var type = typeof(global::Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass);
    var field = new global::Caravela.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetField("Field", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance));
    var propertyAsFieldOrProperty = new global::Caravela.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetProperty("Property", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance));
    var property = typeof(global::Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetProperty("Property", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance);
    var constructor = ((global::System.Reflection.ConstructorInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle("M:Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass.#ctor(System.Int32)"))!);
    var constructorParameter = global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle("M:Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass.#ctor(System.Int32)"))!.GetParameters()[0];
    var array = new global::System.Reflection.MemberInfo[]{((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle("M:Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass.TargetMethod_Void(System.Int32)"))!), typeof(global::Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass)};
    return;
}

        public int TargetMethod_Int(int x)
        {
           return 0;
        }
    }