using System;

namespace PostSharp.Engineering.BuildTools.Build
{
    public class MsbuildSolution : Solution
    {
        public MsbuildSolution( string path, bool canTest, bool canPack ) : base( path, canTest, canPack )
        {
        }


        public override void Build( BuildContext context ) => throw new NotImplementedException();

        public override void Test( BuildContext context, bool includeCoverage ) => throw new NotImplementedException();

        public override void Restore( BuildContext context ) => throw new NotImplementedException();

        public override void Pack( BuildContext context ) => throw new NotImplementedException();
    }
}