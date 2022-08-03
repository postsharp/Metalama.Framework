// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.Engine.Formatting;

namespace Metalama.Framework.DesignTime.VisualStudio.Classification;

internal static class DesignTimeTextSpanClassificationHelper
{
    public static DesignTimeTextSpanClassification ToDesignTime( this TextSpanClassification classification )
        => classification switch
        {
            TextSpanClassification.Conflict => DesignTimeTextSpanClassification.Conflict,
            TextSpanClassification.Default => DesignTimeTextSpanClassification.Default,
            TextSpanClassification.Dynamic => DesignTimeTextSpanClassification.Dynamic,
            TextSpanClassification.Excluded => DesignTimeTextSpanClassification.Excluded,
            TextSpanClassification.CompileTime => DesignTimeTextSpanClassification.CompileTime,
            TextSpanClassification.GeneratedCode => DesignTimeTextSpanClassification.GeneratedCode,
            TextSpanClassification.RunTime => DesignTimeTextSpanClassification.RunTime,
            TextSpanClassification.SourceCode => DesignTimeTextSpanClassification.SourceCode,
            TextSpanClassification.TemplateKeyword => DesignTimeTextSpanClassification.TemplateKeyword,
            TextSpanClassification.CompileTimeVariable => DesignTimeTextSpanClassification.CompileTimeVariable,
            TextSpanClassification.NeutralTrivia => DesignTimeTextSpanClassification.Default,
            _ => throw new ArgumentOutOfRangeException( nameof(classification), classification, null )
        };
}