internal class TargetClass
    {
        public TargetClass( int x ) { }

        public int Field;
        public int Property { get; set; }
        
        [Override]
        public void TargetMethod_Void(int x)
{
    var methodInfo = ((global::System.Reflection.MethodInfo)typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetMethod("TargetMethod_Void", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, new[] { typeof(global::System.Int32) })!);
    var methodBase = ((global::System.Reflection.MethodInfo)typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetMethod("TargetMethod_Void", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, new[] { typeof(global::System.Int32) })!);
    var memberInfo = ((global::System.Reflection.MethodInfo)typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetMethod("TargetMethod_Void", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, new[] { typeof(global::System.Int32) })!);
    var parameterInfo = typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetMethod("TargetMethod_Void", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, new[] { typeof(global::System.Int32) })!.GetParameters()[0];
    var returnValueInfo = ((global::System.Reflection.MethodInfo)typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetMethod("TargetMethod_Void", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, new[] { typeof(global::System.Int32) })!).ReturnParameter;
    var type = typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass);
    var field = new global::Metalama.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetField("Field", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance));
    var propertyAsFieldOrProperty = new global::Metalama.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetProperty("Property", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance));
    var property = typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetProperty("Property", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance);
    var constructor = ((global::System.Reflection.ConstructorInfo)typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetConstructor(global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, new[] { typeof(global::System.Int32) })!);
    var constructorParameter = typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetConstructor(global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, new[] { typeof(global::System.Int32) })!.GetParameters()[0];
    var array = new global::System.Reflection.MemberInfo[]{((global::System.Reflection.MethodInfo)typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass).GetMethod("TargetMethod_Void", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, new[] { typeof(global::System.Int32) })!), typeof(global::Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer.TargetClass)};
    return;
}

        public int TargetMethod_Int(int x)
        {
           return 0;
        }
    }