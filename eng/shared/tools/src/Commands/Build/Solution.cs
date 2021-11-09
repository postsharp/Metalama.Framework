using PostSharp.Engineering.BuildTools.Commands.Build;

namespace PostSharp.Engineering.BuildTools.Build
{
    public abstract class Solution
    {
        public string Path { get; }
        public bool CanTest { get; }
        public bool CanPack { get; }

        public abstract bool Build( BuildContext context, CommonOptions options );
        public abstract bool Test( BuildContext context, TestOptions options );
        public abstract bool Restore( BuildContext context, CommonOptions options );
        public abstract bool Pack( BuildContext context, CommonOptions options );

        protected Solution( string path, bool canTest, bool canPack )
        {
            this.Path = path;
            this.CanTest = canTest;
            this.CanPack = canPack;
        }
    }
}