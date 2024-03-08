// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking;

internal sealed partial class LinkerInjectionStep
{
    private sealed class AuxiliaryMemberTransformations
    {
        // Indicates that source declaration has to be injected. This declaration will receive inserted statements and initializers.
        // Used for primary constructors.
        private volatile bool _shouldInjectAuxiliarySourceMember;

        // Lists all insert statement transformations that are the origin for output contract auxiliaries.
        // Typically this is the first insert statement after an override (or source version) when there is an output contract statement before the next override (or final version).
        private List<(IInsertStatementTransformation, string)>? _auxiliaryOutputContractMembers;

        public bool ShouldInjectAuxiliarySourceMember => this._shouldInjectAuxiliarySourceMember;

        public IReadOnlyList<(IInsertStatementTransformation OriginTransformation, string ReturnVariableName)> AuxiliaryOutputContractMembers =>
            (IReadOnlyList<(IInsertStatementTransformation, string)>?) this._auxiliaryOutputContractMembers
            ?? Array.Empty<(IInsertStatementTransformation, string)>();

        public void InjectAuxiliarySourceMember() => this._shouldInjectAuxiliarySourceMember = true;

        public void InjectAuxiliaryOutputContractMember(IInsertStatementTransformation originTransformation, string returnVariableName)
        {
            this._auxiliaryOutputContractMembers ??= new List<(IInsertStatementTransformation, string)>();
            this._auxiliaryOutputContractMembers.Add( (originTransformation, returnVariableName) );
        }
    }
}