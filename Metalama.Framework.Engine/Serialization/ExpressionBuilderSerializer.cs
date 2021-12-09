// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Serialization
{
    internal class ExpressionBuilderSerializer : ObjectSerializer<IExpressionBuilder>
    {
        public ExpressionBuilderSerializer( SyntaxSerializationService service ) : base( service ) { }

        public override ExpressionSyntax Serialize( IExpressionBuilder obj, SyntaxSerializationContext serializationContext )
            => ((IUserExpression) obj.ToExpression()).ToRunTimeExpression();

        public override Type? OutputType => null;
    }
}