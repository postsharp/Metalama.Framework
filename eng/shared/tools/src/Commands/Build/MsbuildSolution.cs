using PostSharp.Engineering.BuildTools.Commands.Build;
using System;

namespace PostSharp.Engineering.BuildTools.Build
{
    public class MsbuildSolution : Solution
    {
        public MsbuildSolution( string path, bool canTest, bool canPack ) : base( path, canTest, canPack )
        {
        }


        public override bool Build( BuildContext context, CommonOptions options ) =>
            throw new NotImplementedException();

        public override bool Test( BuildContext context, TestOptions includeCoverage ) =>
            throw new NotImplementedException();

        public override bool Restore( BuildContext context, CommonOptions options ) =>
            throw new NotImplementedException();

        public override bool Pack( BuildContext context, CommonOptions options ) => throw new NotImplementedException();
    }
}