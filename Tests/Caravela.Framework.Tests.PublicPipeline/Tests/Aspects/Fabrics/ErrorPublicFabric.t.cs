// Error CR0230 on `F`: `The compile-time type 'TargetCode.F' must have private visibility because it is nested in a run-time-type.`
internal class TargetCode
    {
        public class F : ITypeFabric
        {
            public void AmendType(ITypeAmender builder) => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

        }
    }