// Copyright (c) SharpCrafters s.r.o. All rights reserved.

using System.CommandLine;

namespace PostSharp.Engineering.BuildTools.Nuget
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
