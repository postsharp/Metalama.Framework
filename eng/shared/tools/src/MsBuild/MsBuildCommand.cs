// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;

namespace PostSharp.Engineering.BuildTools.MsBuild
{
    internal class MsBuildCommand : Command
    {
        public MsBuildCommand() : base( "msbuild", "Work with MSBuild projects" )
        {
            this.AddCommand( new AddProjectReferenceCommand() );
        }
    }
}