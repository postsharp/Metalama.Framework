// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.SyntaxSerialization
{
    internal class ExpressionSerializer : ObjectSerializer<IExpression>
    {
        public ExpressionSerializer( SyntaxSerializationService service ) : base( service ) { }

        public override ExpressionSyntax Serialize( IExpression obj, SyntaxSerializationContext serializationContext )
        {
            return ((IUserExpression) obj.Value!).ToExpressionSyntax( serializationContext.SyntaxGenerationContext );
        }

        public override Type? OutputType => null;
    }
}