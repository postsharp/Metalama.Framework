// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code.SyntaxBuilders
{
    public sealed partial class InterpolatedStringBuilder
    {
        internal class Token
        {
            public object? Expression { get; }

            public Token( object? expression )
            {
                this.Expression = expression;
            }
        }
    }
}