// Copyright (c) SharpCrafters s.r.o. All rights reserved.

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
