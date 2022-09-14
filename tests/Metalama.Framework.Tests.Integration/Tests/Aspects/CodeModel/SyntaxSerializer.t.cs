internal class TargetClass
    {
        public TargetClass( int x ) { }

        public int Field;
        public int Property { get; set; }
        
        [Override]
        public void TargetMethod_Void(int x)
{
    var methodInfo = ((global::System.Reflection.MethodInfo)global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass), "TargetMethod_Void", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, "TargetClass.TargetMethod_Void(int)")!);
    var methodBase = ((global::System.Reflection.MethodInfo)global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass), "TargetMethod_Void", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, "TargetClass.TargetMethod_Void(int)")!);
    var memberInfo = ((global::System.Reflection.MethodInfo)global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass), "TargetMethod_Void", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, "TargetClass.TargetMethod_Void(int)")!);
    var parameterInfo = global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass), "TargetMethod_Void", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, "TargetClass.TargetMethod_Void(int)")!.GetParameters()[0];
    var returnValueInfo = ((global::System.Reflection.MethodInfo)global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass), "TargetMethod_Void", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, "TargetClass.TargetMethod_Void(int)")!).ReturnParameter;
    var type = typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass);
    var field = new global::Metalama.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetField("Field", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance));
    var propertyAsFieldOrProperty = new global::Metalama.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetProperty("Property", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance));
    var property = typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetProperty("Property", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance);
    var constructor = ((global::System.Reflection.ConstructorInfo)global::Metalama.Framework.RunTime.ReflectionHelper.GetConstructor(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass), global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, "TargetClass.TargetClass(int)")!);
    var constructorParameter = global::Metalama.Framework.RunTime.ReflectionHelper.GetConstructor(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass), global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, "TargetClass.TargetClass(int)")!.GetParameters()[0];
    var array = new global::System.Reflection.MemberInfo[]{((global::System.Reflection.MethodInfo)global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass), "TargetMethod_Void", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, "TargetClass.TargetMethod_Void(int)")!), typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass)};
    return;
}

        public int TargetMethod_Int(int x)
        {
           return 0;
        }
    }