// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.CodeFixes;
using System.Collections.Immutable;

namespace Metalama.Framework.Diagnostics;

/// <summary>
/// Represents an instance of <see cref="DiagnosticDefinition{T}"/>, encapsulating the arguments used for the parametric diagnostic definition.
/// </summary>
internal sealed class DiagnosticImpl<T> : IDiagnostic
    where T : notnull
{
    private readonly DiagnosticDefinition<T> _definition;
    private readonly T _arguments;

    object IDiagnostic.Arguments => this._arguments;

    IDiagnosticDefinition IDiagnostic.Definition => this._definition;

    private ImmutableArray<CodeFix> CodeFixes { get; set; }

    ImmutableArray<CodeFix> IDiagnostic.CodeFixes => this.CodeFixes;

    public DiagnosticImpl( DiagnosticDefinition<T> definition, T arguments, ImmutableArray<CodeFix> codeFixes )
    {
        this._definition = definition;
        this._arguments = arguments;
        this.CodeFixes = codeFixes;
    }

    IDiagnostic IDiagnostic.WithCodeFixes( params CodeFix[] codeFixes )
    {
        this.CodeFixes = this.CodeFixes.AddRange( codeFixes );

        return this;
    }

    public override string ToString() => $"{this._definition.Id}: {this._definition.Title} ({this._arguments})";
}