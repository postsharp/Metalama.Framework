using System.CommandLine;

namespace PostSharp.Engineering.BuildTools.Commands.Csproj
{
    internal class CsprojCommand : Command
    {
        public CsprojCommand() : base( "csproj", "Work with *.csproj files" )
        {
            this.AddCommand( new AddProjectReferenceCommand() );
        }
    }
}