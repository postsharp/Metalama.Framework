// Error CR0230 on `F`: `The compile-time type 'TargetCode.F' must have private visibility because it is nested in a run-time-type.`
internal class TargetCode
    {
#pragma warning disable CS0067
        public class F : TypeFabric
        {
            public override void AmendType(ITypeAmender amender) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

        }
#pragma warning restore CS0067
    }