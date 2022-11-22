﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.CodeLens;

namespace Metalama.Framework.DesignTime.CodeLens;

public class CodeLensSummary : ICodeLensSummary
{
    public static CodeLensSummary NotAvailable { get; } = new( "-" );

    public static CodeLensSummary NoAspect { get; } = new( "no aspect" );

    public CodeLensSummary( string description, string? tooltipText = null )
    {
        this.Description = description;
        this.TooltipText = tooltipText;
    }

    public string Description { get; }

    public string? TooltipText { get; }
}