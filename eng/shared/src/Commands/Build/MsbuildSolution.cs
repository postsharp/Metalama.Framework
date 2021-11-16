using System;

namespace PostSharp.Engineering.BuildTools.Commands.Build
{
    public class MsbuildSolution : Solution
    {
        public MsbuildSolution( string solutionPath ) : base( solutionPath )
        {
        }

        public override bool Build( BuildContext context, BuildOptions options ) =>
            throw new NotImplementedException();

        public override bool Pack( BuildContext context, BuildOptions options ) => throw new NotImplementedException();

        public override bool Test( BuildContext context, TestOptions includeCoverage ) =>
            throw new NotImplementedException();

        public override bool Restore( BuildContext context, CommonOptions options ) =>
            throw new NotImplementedException();
    }
}