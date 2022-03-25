    using System;
    using System.Linq;
    using Metalama.Framework.Aspects;
    using Metalama.Framework.Code;
    
    namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing_MultipleDeclarations
    {
    #pragma warning disable CS0067
        public class Aspect : TypeAspect
        {
            public override void BuildAspect(IAspectBuilder<INamedType> builder)
            {
                builder.Advices.Initialize(builder.Target, nameof(Template), InitializationReason.Constructing);
            }
    
            [Template]
    public void Template() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
    
        }
    #pragma warning restore CS0067
    
        [Aspect]
        public partial class TargetCode
        {
            public TargetCode()
            {
        this.Constructing_Aspect();
            }
    
            private int Method( int a )
            {
                return a;
            }
    
    
    private void Constructing_Aspect()
    {
        global::System.Console.WriteLine($"TargetCode: Aspect");
    }    }
    
        public partial class TargetCode
        {
            public TargetCode(int x)
            {
        this.Constructing_Aspect();
            }
        }
    
        public partial class TargetCode
        {
        }
    }