// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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