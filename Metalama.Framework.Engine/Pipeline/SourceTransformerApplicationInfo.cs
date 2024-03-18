// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Diagnostics;
using System.Collections.Immutable;
using System.Reflection;

namespace Metalama.Framework.Engine.Pipeline;

/// <summary>
/// Provide application information stored using <see cref="AssemblyMetadataAttribute"/>.
/// </summary>
internal sealed class SourceTransformerApplicationInfo : ApplicationInfoBase
{
    private readonly bool _ignoreUnattendedProcess;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceTransformerApplicationInfo"/> class.
    /// </summary>
    public SourceTransformerApplicationInfo( bool isLongRunningProcess, bool ignoreUnattendedProcess )
        : base( typeof(SourceTransformerApplicationInfo).Assembly )
    {
        this._ignoreUnattendedProcess = ignoreUnattendedProcess;
        this.IsLongRunningProcess = isLongRunningProcess;
    }

    /// <inheritdoc />
    public override ProcessKind ProcessKind => ProcessKind.Compiler;

    /// <inheritdoc />
    public override bool IsUnattendedProcess( ILoggerFactory loggerFactory ) => !this._ignoreUnattendedProcess && base.IsUnattendedProcess( loggerFactory );

    /// <inheritdoc />
    public override bool IsLongRunningProcess { get; }

    /// <inheritdoc />
    public override string Name => "Metalama.Framework";

    public override ImmutableArray<IComponentInfo> Components => ImmutableArray<IComponentInfo>.Empty;
}