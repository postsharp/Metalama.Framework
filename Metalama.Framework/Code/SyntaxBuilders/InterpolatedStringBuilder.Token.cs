// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code.SyntaxBuilders
{
    public sealed partial class InterpolatedStringBuilder
    {
        internal sealed class Token
        {
            public object? Expression { get; }

            public Token( object? expression )
            {
                this.Expression = expression;
            }
        }
    }
}