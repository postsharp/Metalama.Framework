// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking;
// TODO: the use of LinkerInjectedMember is a smell/hack.

/// <summary>
/// Extended <see cref="InjectedMember"/> used by <see cref="AspectLinker"/>.
/// </summary>
internal sealed class LinkerInjectedMember : InjectedMemberOrNamedType
{
    /// <summary>
    /// Gets id, which can be used to identify syntax node with the original transformation.
    /// </summary>
    public string LinkerNodeId { get; }

    public LinkerInjectedMember( string linkerNodeId, MemberDeclarationSyntax linkerAnnotatedSyntax, InjectedMemberOrNamedType original )
        : base( original, linkerAnnotatedSyntax )
    {
        this.LinkerNodeId = linkerNodeId;
    }
}