using System.CommandLine;

namespace PostSharp.Engineering.BuildTools.Commands.Coverage
{
    public class CoverageCommand : Command
    {
        public CoverageCommand() : base( "coverage", "Work with test coverage" )
        {
            this.AddCommand( new WarnCommand() );
        }
    }
}