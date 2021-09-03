// Copyright (c) SharpCrafters s.r.o. All rights reserved.

using System.CommandLine;

namespace PostSharp.Engineering.BuildTools.Coverage
{
    public class CoverageCommand : Command
    {
        public CoverageCommand() : base( "coverage", "Work with test coverage" )
        {
            this.AddCommand( new WarnCommand() );
        }
    }
}