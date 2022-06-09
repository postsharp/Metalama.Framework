// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine.CodeModel;

internal class SyntaxGeneratorWithContext : OurSyntaxGenerator
{
    public SyntaxGeneratorWithContext( OurSyntaxGenerator prototype, SyntaxGenerationContext context ) : base( prototype )
    {
        _ = context;
    }
    
    // This class would have members that require context, but we don't have any now.

}