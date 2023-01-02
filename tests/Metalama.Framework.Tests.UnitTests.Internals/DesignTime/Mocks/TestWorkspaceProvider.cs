﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Services;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;

internal sealed class TestWorkspaceProvider : WorkspaceProvider
{
    private readonly AdhocWorkspace _workspace = new();
    private readonly Dictionary<string, ProjectData> _projectIdsByProjectName = new();

    public TestWorkspaceProvider( GlobalServiceProvider serviceProvider ) : base( serviceProvider ) { }

    private sealed record ProjectData( ProjectId ProjectId, ProjectKey ProjectKey )
    {
        public Dictionary<string, DocumentId> Documents { get; } = new();
    }

    public override Task<Workspace> GetWorkspaceAsync( CancellationToken cancellationToken = default ) => Task.FromResult( (Workspace) this._workspace );

    public ProjectKey AddOrUpdateProject( string projectName, string[]? projectReferences = null )
    {
        if ( this._projectIdsByProjectName.TryGetValue( projectName, out var projectData ) )
        {
            return projectData.ProjectKey;
        }
        else
        {
            var projectId = ProjectId.CreateNewId();

            var parseOptions = TestCompilationFactory.GetParseOptions();

            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                projectName,
                projectName,
                LanguageNames.CSharp,
                metadataReferences: TestCompilationFactory.GetMetadataReferences(),
                parseOptions: parseOptions,
                compilationOptions: TestCompilationFactory.GetCompilationOptions() );

            projectInfo = projectInfo.WithMetadataReferences( TestCompilationFactory.GetMetadataReferences() );

            if ( projectReferences != null )
            {
                projectInfo = projectInfo.WithProjectReferences(
                    projectReferences.SelectAsArray( r => new ProjectReference( this._projectIdsByProjectName[r].ProjectId ) ) );
            }

            var projectKey = ProjectKeyFactory.Create( projectName, parseOptions );
            this._projectIdsByProjectName[projectName] = new ProjectData( projectId, projectKey );
            this._workspace.AddProject( projectInfo );

            return projectKey;
        }
    }

    public ProjectKey AddOrUpdateProject( string projectName, Dictionary<string, string> code, string[]? projectReferences = null )
    {
        var projectKey = this.AddOrUpdateProject( projectName, projectReferences );
        this.AddOrUpdateDocuments( projectName, code );

        return projectKey;
    }

    public Microsoft.CodeAnalysis.Project GetProject( string projectName )
        => this._workspace.CurrentSolution.GetProject( this._projectIdsByProjectName[projectName].ProjectId ).AssertNotNull();

    public void AddOrUpdateDocuments( string projectName, Dictionary<string, string> code )
    {
        var projectData = this._projectIdsByProjectName[projectName];

        var solution = this._workspace.CurrentSolution;

        foreach ( var file in code )
        {
            Document newDocument;

            if ( projectData.Documents.TryGetValue( file.Key, out var documentId ) )
            {
                var document = solution.GetProject( projectData.ProjectId ).AssertNotNull().GetDocument( documentId ).AssertNotNull();
                newDocument = document.WithText( SourceText.From( file.Value ) );
            }
            else
            {
                var loader = TextLoader.From( TextAndVersion.Create( SourceText.From( file.Value ), VersionStamp.Create() ) );

                var documentInfo = DocumentInfo.Create(
                    DocumentId.CreateNewId( projectData.ProjectId, file.Key ),
                    file.Key,
                    filePath: file.Key,
                    loader: loader );

                newDocument = this._workspace.AddDocument( documentInfo );
                projectData.Documents.Add( file.Key, newDocument.Id );
            }

            solution = newDocument.Project.Solution;
        }

        if ( !this._workspace.TryApplyChanges( solution ) )
        {
            throw new AssertionFailedException( "Updating the solution was not successful." );
        }
    }

    public Document GetDocument( string projectName, string documentName )
    {
        var projectData = this._projectIdsByProjectName[projectName];
        var documentId = projectData.Documents[documentName];

        return this._workspace.CurrentSolution.GetDocument( documentId ).AssertNotNull();
    }

    public override void Dispose()
    {
        base.Dispose();
        this._workspace.Dispose();
    }
}