// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code.SyntaxBuilders;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Serialization
{
    internal class ExpressionBuilderSerializer : ObjectSerializer<IExpressionBuilder>
    {
        public ExpressionBuilderSerializer( SyntaxSerializationService service ) : base( service ) { }

        public override ExpressionSyntax Serialize( IExpressionBuilder obj, SyntaxSerializationContext serializationContext )
            => ((IUserExpression) obj.ToExpression()).ToRunTimeExpression();

        public override Type? OutputType => null;
    }
}