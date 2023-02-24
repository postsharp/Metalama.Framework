// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Maintenance;
using Metalama.Framework.Engine.Licensing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Metalama.Tool.Licensing;

internal class ResetCreditsCommands : CreditsBaseCommand
{
    protected override void Execute( CreditsCommandContext context, CreditsCommandSettings settings )
    {
        var deleted = 0;

        foreach ( var file in context.Files )
        {
            try
            {
                File.Delete( file.DataFilePath );

                deleted++;
            }
            catch ( Exception e )
            {
                context.Console.WriteWarning( $"Cannot delete '{file.DataFilePath}': {e.Message}" );
            }
        }

        context.Console.WriteSuccess( $"{deleted} files have been deleted." );
    }
}

internal abstract class CreditsBaseCommand : BaseCommand<CreditsCommandSettings>
{
    protected sealed override void Execute( ExtendedCommandContext context, CreditsCommandSettings settings )
    {
        var horizon = settings.GetHorizon();
        
        context.Console.WriteMessage( $"Considering builds from {horizon:f}. Use -d, -w or -h option to change the time horizon." );

        // Check horizon.
        if ( (DateTime.Now - horizon).TotalDays > 31 )
        {
            throw new CommandException( "The time horizon cannot be larger than 30 days because data files are automatically cleaned up over this horizon." );
        }
        
        // Check license settings.
        var licenseService = context.ServiceProvider.GetRequiredBackstageService<ILicenseConsumptionService>();

        if ( !licenseService.IsTrialLicense )
        {
            context.Console.WriteWarning(
                "The trial mode is currently not activated. Credit consumption data is therefore only being collected " + 
                "when projects are built with the parameter `/p:MetalamaWriteLicenseCreditData=True` or when the build " +
                "fails because of insufficient license credits." );
        }

        var projectFilters = settings.GetProjectRegexes();

        // Read the files.
        var tempFileManager = context.ServiceProvider.GetRequiredBackstageService<ITempFileManager>();

        var files = new List<LicenseConsumptionFile>();

        var hasUnselectedFile = false;

        foreach ( var path in LicenseVerifier.GetConsumptionDataFiles( tempFileManager ) )
        {
            try
            {
                var file = LicenseConsumptionFile.FromFile( path );

                if ( file == null )
                {
                    continue;
                }

                hasUnselectedFile = true;

                var projectName = Path.GetFileNameWithoutExtension( file.ProjectPath );

                // Apply the filters.
                if ( file.BuildTime < horizon )
                {
                    continue;
                }

                if ( projectFilters.Count > 0 && !projectFilters.Any( r => r.IsMatch( projectName ) ) )
                {
                    continue;
                }

                if ( !string.IsNullOrWhiteSpace( settings.Configurations )
                     && !CreditsCommandSettings.SplitCommaSeparatedList( settings.Configurations )
                         .Any( c => string.Equals( c, file.Configuration, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    continue;
                }

                static string GetTargetFrameworkOrEmpty( string s ) => string.IsNullOrWhiteSpace( s ) ? "empty" : s;

                if ( !string.IsNullOrWhiteSpace( settings.TargetFrameworks )
                     && !CreditsCommandSettings.SplitCommaSeparatedList( settings.TargetFrameworks )
                         .Any( c => string.Equals( c, GetTargetFrameworkOrEmpty( file.TargetFramework ), StringComparison.OrdinalIgnoreCase ) ) )
                {
                    continue;
                }

                files.Add( file );
            }
            catch ( Exception e )
            {
                context.Console.WriteWarning( $"Cannot read '{path}': {e.Message}" );
            }
        }

        if ( files.Count == 0 )
        {
            if ( hasUnselectedFile )
            {
                throw new CommandException( "No build matches the specified filters." );
            }
            else
            {
                throw new CommandException( "No license credit data has been collected yet." );
            }
        }

        // Execute the command.
        this.Execute( new CreditsCommandContext( context, files, horizon ), settings );
    }

    protected abstract void Execute( CreditsCommandContext context, CreditsCommandSettings settings );
}