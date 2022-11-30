using System.ComponentModel;
using Metalama.Framework.Aspects;
using Metalama.Testing.Framework;


namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DynamicGreedyArguments
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // Dynamic argument of build-time-only method.
            meta.Target.Method.Invoke( new PropertyChangedEventArgs( meta.Target.Parameters[0].Name) );
            
            // Invocation in dynamic context.
            meta.This.PropertyChanged.Invoke(new PropertyChangedEventArgs( meta.Target.Parameters[0].Name));
            
            // Conditional access in dynamic context.
           meta.This.PropertyChanged.Invoke( new PropertyChangedEventArgs(meta.Target.Parameters[0].Name));
            
            return default;
        }
    }

    // <target>
    class TargetCode
    {
        int Method(PropertyChangedEventArgs a)
        {
            return 0;
        }
    }
}