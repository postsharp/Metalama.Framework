using System.ComponentModel;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;


namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.DynamicGreedyArguments
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // Dynamic argument of build-time-only method.
            meta.Method.Invoke( new PropertyChangedEventArgs( meta.Parameters[0].Name) );
            
            // Invocation in dynamic context.
            meta.This.PropertyChanged.Invoke(new PropertyChangedEventArgs( meta.Parameters[0].Name));
            
            // Conditional access in dynamic context.
           meta.This.PropertyChanged.Invoke( new PropertyChangedEventArgs(meta.Parameters[0].Name));
            
            return default;
        }
    }

    [TestOutput]
    class TargetCode
    {
        int Method(PropertyChangedEventArgs a)
        {
            return 0;
        }
    }
}