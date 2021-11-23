using System.IO;

namespace PostSharp.Engineering.BuildTools.Build.Model
{
    public abstract class Solution
    {
        public virtual string Name => Path.GetFileName( this.SolutionPath );

        public string SolutionPath { get; }
        
        public bool IsTestOnly { get; init; }

        public bool PackRequiresExplicitBuild { get; init; }

        public bool SupportsTestCoverage { get; init; }

        public abstract bool Build( BuildContext context, BuildOptions options );

        public abstract bool Pack( BuildContext context, BuildOptions options );

        public abstract bool Test( BuildContext context, TestOptions options );

        public abstract bool Restore( BuildContext context, BaseBuildSettings options );

        protected Solution( string solutionPath )
        {
            this.SolutionPath = solutionPath;
        }
    }
}