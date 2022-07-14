    internal record struct TargetRecordStruct
    {
        private int Method1( int a ) 
{ 
    return a;
}

        private string Method2( string s ) 
{ 
    return s;
}
#pragma warning disable CS0067

        private class Fabric : TypeFabric
        {
            public override void AmendType(ITypeAmender amender) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");


            [Template]
private dynamic? Template() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

        }
#pragma warning restore CS0067
    }