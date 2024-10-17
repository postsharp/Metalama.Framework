﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.BuilderData;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.LinkerTests.Runner;

/// <summary>
/// Represents a test transformation that takes syntax of a PseudoOverride-marked member and injects it.
/// </summary>
internal class TestPromoteFieldTransformation : TestIntroduceDeclarationTransformation, IReplaceMemberTransformation
{
    public IFullRef<IMember>? ReplacedMember { get; set; }

    public TestPromoteFieldTransformation(
        AspectLayerInstance aspectLayerInstance, 
        InsertPosition insertPosition, 
        IFullRef<IField> promotedField, 
        PropertyBuilderData builderData,
        MemberDeclarationSyntax syntax )
        : base( aspectLayerInstance, insertPosition, builderData, syntax )
    {
        this.ReplacedMember = promotedField;
    }
}
