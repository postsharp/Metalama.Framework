// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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