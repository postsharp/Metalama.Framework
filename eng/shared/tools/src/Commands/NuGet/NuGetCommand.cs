using System.CommandLine;

namespace PostSharp.Engineering.BuildTools.Commands.NuGet
{
    internal class NuGetCommand : Command
    {
        public NuGetCommand() : base( "nuget", "Work with NuGet packages" )
        {
            this.AddCommand( new RenamePackagesCommand() );
            this.AddCommand( new VerifyPackageCommand() );
        }
    }
}