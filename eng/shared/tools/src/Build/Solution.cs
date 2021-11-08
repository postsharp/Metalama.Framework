namespace PostSharp.Engineering.BuildTools.Build
{
    public abstract class Solution
    {
        public string Path { get; }
        public bool CanTest { get; }
        public bool CanPack { get; }

        public abstract void Build( BuildContext context );
        public abstract void Test( BuildContext context, bool includeCoverage );
        public abstract void Restore( BuildContext context );
        public abstract void Pack( BuildContext context );

        protected Solution( string path, bool canTest, bool canPack )
        {
            this.Path = path;
            this.CanTest = canTest;
            this.CanPack = canPack;
        }
    }
}