using System;

namespace PostSharp.Engineering.BuildTools.Build
{
    public class DotNetSolution : Solution
    {
        public DotNetSolution( string path, bool canTest, bool canPack ) : base( path, canTest, canPack )
        {
        }

        public override void Build( BuildContext buildOptions ) => throw new NotImplementedException();
        public override void Test( BuildContext context, bool includeCoverage ) => throw new NotImplementedException();


        public override void Restore( BuildContext buildOptions ) => throw new NotImplementedException();
        public override void Pack( BuildContext context ) => throw new NotImplementedException();
    }
}