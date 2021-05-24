// The compiled template failed. 
// Error CR0112 on `Template`: `The advice 'Aspect.Template()' threw 'InvalidUserCodeException' when applied to 'TargetCode.Method(int)':
// Caravela.Framework.Impl.InvalidUserCodeException: Test.cs(22,20): error CR0114: The advice 'Aspect.Template()' cannot use 'meta.This' in an advice applied to method 'TargetCode.Method(int)' because the target method is static.
//    at Caravela.Framework.Impl.Templating.MetaModel.MetaApi.GetThisOrBase(String expressionName, LinkerAnnotation linkerAnnotation)
//    at Caravela.Framework.Impl.Templating.MetaModel.MetaApi.get_This()
//    at Caravela.Framework.Aspects.meta.get_This()
//    at Caravela.Framework.Tests.Integration.Templating.Dynamic.DynamicReceiverThisInStaticContext.Aspect.Template_Template()`