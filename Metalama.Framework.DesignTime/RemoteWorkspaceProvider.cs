// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Metalama.Framework.DesignTime;

internal class RemoteWorkspaceProvider : WorkspaceProvider
{
    public RemoteWorkspaceProvider( GlobalServiceProvider serviceProvider ) : base( serviceProvider )
    {
        var remoteWorkspaceManagerType = Type.GetType(
            "Microsoft.CodeAnalysis.Remote.RemoteWorkspaceManager, Microsoft.CodeAnalysis.Remote.ServiceHub",
            false );

        if ( remoteWorkspaceManagerType == null )
        {
            throw new AssertionFailedException( "Cannot find the RemoteWorkspaceManager type." );
        }

        var remoteWorkspaceManagerDefaultField = remoteWorkspaceManagerType.GetField(
            "Default",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );

        if ( remoteWorkspaceManagerDefaultField == null )
        {
            throw new AssertionFailedException( "Cannot find the RemoteWorkspaceManager.Default property." );
        }

        var remoteWorkspaceManagerGetWorkspaceMethod = remoteWorkspaceManagerType.GetMethod(
            "GetWorkspace",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null );

        if ( remoteWorkspaceManagerGetWorkspaceMethod == null )
        {
            throw new AssertionFailedException( "Cannot find the RemoteWorkspaceManager.GetWorkspace method." );
        }

        var defaultWorkspaceManager = remoteWorkspaceManagerDefaultField.GetValue( null );

        if ( defaultWorkspaceManager == null )
        {
            throw new AssertionFailedException( " RemoteWorkspaceManager.Default returned null." );
        }

        var workspace = remoteWorkspaceManagerGetWorkspaceMethod.Invoke( defaultWorkspaceManager, Array.Empty<object?>() );

        this.Workspace = (Workspace) (workspace ?? throw new AssertionFailedException( "RemoteWorkspaceManager.GetWorkspace returned null." ));
    }

    public override Workspace Workspace { get; }
}