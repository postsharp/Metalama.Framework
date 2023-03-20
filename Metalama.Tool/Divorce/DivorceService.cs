// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using LibGit2Sharp;
using Metalama.Backstage.Diagnostics;
using Metalama.Compiler;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Locator;
using Microsoft.Build.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.IO;
using BackstageILogger = Metalama.Backstage.Diagnostics.ILogger;
using MSBuildILogger = Microsoft.Build.Framework.ILogger;

namespace Metalama.Tool.Divorce;

public class DivorceService
{
    private readonly BackstageILogger _logger;
    private readonly string _projectPath;
    private readonly string? _configuration;
    private readonly string? _targetFramework;

    public DivorceService( IServiceProvider serviceProvider, string projectPath, string? configuration, string? targetFramework )
    {
        MSBuildLocator.RegisterDefaults();

        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "Divorce" );
        this._projectPath = projectPath;
        this._configuration = configuration;
        this._targetFramework = targetFramework;
    }

    public void CheckGitStatus()
    {
        var repoPath = Repository.Discover( this._projectPath )
                       ?? throw new InvalidOperationException( $"The path '{this._projectPath}' is not part of a git repository. To use this feature anyway, provide the --force option." );

        using var repo = new Repository( repoPath );

        if ( repo.RetrieveStatus().IsDirty )
        {
            throw new InvalidOperationException( $"The git repository at '{repo.Info.WorkingDirectory}' has pending changes (see git status). To use this feature anyway, provide the --force option." );
        }
    }

    private (string MetalamaDirectory, string? ObjDirectory) BuildProject()
    {
        var loggers = new MSBuildILogger[] { new ConsoleLogger( LoggerVerbosity.Quiet ) };

        var project = new Project( this._projectPath );

        var targetFrameworks = project.GetPropertyValue( "TargetFrameworks" );

        if ( this._targetFramework == null && !string.IsNullOrEmpty( targetFrameworks ) )
        {
            throw new InvalidOperationException(
                $"The project '{this._projectPath}' has multiple target frameworks ({targetFrameworks}). Specify a particular target framework using the -f option." );
        }

        var success = project.Build( "Restore", loggers );

        if ( !success )
        {
            throw new InvalidOperationException( $"Could not restore the project '{this._projectPath}'." );
        }

        // Build has to be run on a different Project than Restore.
        ProjectCollection.GlobalProjectCollection.UnloadProject( project );

        if ( this._configuration != null )
        {
            ProjectCollection.GlobalProjectCollection.SetGlobalProperty( "Configuration", this._configuration );
        }

        if ( this._targetFramework != null )
        {
            ProjectCollection.GlobalProjectCollection.SetGlobalProperty( "TargetFramework", this._targetFramework );
        }

        ProjectCollection.GlobalProjectCollection.SetGlobalProperty( "WarningLevel", "0" );
        ProjectCollection.GlobalProjectCollection.SetGlobalProperty( "NoWarn", "NETSDK1188" );

        project = new Project( this._projectPath );

        project.SetProperty( "MetalamaEmitCompilerTransformedFiles", "True" );
        project.SetProperty( "MetalamaFormatOutput", "True" );

        var projectInstance = project.CreateProjectInstance();
        success = projectInstance.Build( loggers );

        if ( !success )
        {
            throw new InvalidOperationException( $"Could not build the project '{this._projectPath}'." );
        }

        string ResolveRelativeDirectory( string directory )
        {
            if ( Path.IsPathRooted( directory ) )
            {
                return directory;
            }

            return Path.Combine( Path.GetDirectoryName( this._projectPath )!, directory );
        }

        var objDirectory = projectInstance.GetPropertyValue( "IntermediateOutputPath" );

        if ( string.IsNullOrEmpty( objDirectory ) )
        {
            objDirectory = null;
        }
        else
        {
            objDirectory = ResolveRelativeDirectory( objDirectory );
        }

        var metalamaDirectory = projectInstance.GetPropertyValue( "MetalamaCompilerTransformedFilesOutputPath" );

        if ( string.IsNullOrEmpty( metalamaDirectory ) )
        {
            throw new InvalidOperationException( $"Could not find the Metalama output directory for project '{this._projectPath}'." );
        }

        metalamaDirectory = ResolveRelativeDirectory( metalamaDirectory );

        if ( !Directory.Exists( metalamaDirectory ) )
        {
            throw new InvalidOperationException( $"The Metalama output directory for project '{this._projectPath}' does not exist." );
        }

        ProjectCollection.GlobalProjectCollection.UnloadProject( project );

        project.Xml.Reload( throwIfUnsavedChanges: false, preserveFormatting: true );

        return (metalamaDirectory, objDirectory);
    }

    private static ImmutableArray<TransformedFileMapping> ReadFileMap( string metalamaDirectory )
    {
        var fileMapPath = Path.Combine( metalamaDirectory, TransformedFileMapping.FileName );

        using var fileReader = File.OpenText( fileMapPath );

        using var jsonReader = new JsonTextReader( fileReader );

        return new JsonSerializer().Deserialize<ImmutableArray<TransformedFileMapping>>( jsonReader );
    }

    public void PerformDivorce()
    {
        var (metalamaDirectory, objDirectory) = this.BuildProject();

        var fileMap = ReadFileMap( metalamaDirectory );

        var projectDirectory = Path.GetDirectoryName( this._projectPath )!;

        foreach ( var mapping in fileMap )
        {
            // Relative paths seem to correspond to fake files like @@Intrinsics.cs, so ignore those.
            if ( !Path.IsPathRooted( mapping.OldPath ) )
            {
                this._logger.Trace?.Log( $"Skipping file '{mapping.OldPath}'." );

                continue;
            }

            if ( objDirectory != null && mapping.OldPath.StartsWith( objDirectory, StringComparison.Ordinal ) )
            {
                this._logger.Warning?.Log( $"Skipping file '{mapping.OldPath}', because it's in the obj directory." );

                continue;
            }

            if ( !mapping.OldPath.StartsWith( projectDirectory, StringComparison.Ordinal ) )
            {
                this._logger.Warning?.Log( $"Skipping file '{mapping.OldPath}', because it's not in the project directory." );

                continue;
            }

            this._logger.Info?.Log( $"Replacing file '{mapping.OldPath}'." );

            File.Copy( mapping.NewPath, mapping.OldPath, overwrite: true );
        }

        this._logger.Info?.Log( $"Setting property MetalamaEnabled to false in project {this._projectPath}." );

        var project = new Project( this._projectPath );
        project.SetProperty( "MetalamaEnabled", "false" );
        project.Save();

        this._logger.Info?.Log( $"Divorce feature successfully performed on project '{this._projectPath}'." );
    }
}