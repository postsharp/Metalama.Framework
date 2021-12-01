using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Sdk;
using Caravela.Framework.Project;
using PostSharp.Backstage.Extensibility;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Initialize.ServicePlugIn
{
    internal class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var myService = meta.Target.Project.ServiceProvider.GetService<IMyService>();
            Console.WriteLine( myService.Message );

            return meta.Proceed();
        }
    }
    
    [CompileTimeOnly]
    internal interface IMyService : IService
    {
        string Message { get; }
    }

    [CompilerPlugin]
    internal class MyService : IMyService
    {
        public string Message => "Hello, world.";
    }

    // <target>
    internal class TargetCode
    {
        [Aspect]
        private int Method( int a )
        {
            return a;
        }
    }
}