internal struct TargetStruct
    {
        private int Method1( int a ) => a;

        private string Method2( string s ) => s;
#pragma warning disable CS0067

        private class Fabric : TypeFabric
        {
            public override void AmendType(ITypeAmender amender) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");


            [Template]
[global::Metalama.Framework.Aspects.AccessibilityAttribute(global::Metalama.Framework.Code.Accessibility.Private)
]public dynamic? Template() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

        }

#pragma warning restore CS0067
    }