// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;
using Metalama.Backstage.Commands.Commands;
using Metalama.Framework.Engine.Configuration;
using System;
using System.CommandLine;

namespace Metalama.DotNetTools;

internal class TheRootCommand : RootCommand
{
    public TheRootCommand( ICommandServiceProviderProvider commandServiceProvider )
        : base( "Manages user options of Metalama" )
    {
        // Verbose option needs to be added first.
        // Otherwise the commands won't initialize correctly.
        var verboseOption = new Option<bool>( "--verbose", "Set detailed verbosity level" );
        verboseOption.AddAlias( "-v" );
        this.AddGlobalOption( verboseOption );
        
        // Design-time configuration needs to be added from this repo.
        BackstageCommandFactory.ConfigurationFilesByCategory.Add( "design-time", new DesignTimeConfiguration() );

        foreach ( var command in BackstageCommandFactory.CreateCommands( commandServiceProvider ) )
        {
            this.Add( command );
        }
    }
}