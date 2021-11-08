namespace PostSharp.Engineering.BuildTools.Build
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

        public static VersionSpec Create( int number, bool isPublic ) => isPublic
            ?
            new VersionSpec( VersionKind.Public )
            :
            number > 0
                ? new VersionSpec( VersionKind.Numbered, number )
                : new VersionSpec( VersionKind.Local );
    }
}