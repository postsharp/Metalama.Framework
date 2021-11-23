namespace PostSharp.Engineering.BuildTools.Build.Model
{
    public readonly struct VersionSpec
    {
        public VersionKind Kind { get; }
        public int Number { get; }

        public VersionSpec( VersionKind kind, int number = 0 )
        {
            this.Kind = kind;
            this.Number = number;
        }
    }
}