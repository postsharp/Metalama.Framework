// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Transformations;

internal class SyntaxBasedInitializationTransformation : BaseTransformation, IInsertStatementTransformation
{
    private readonly IConstructor _targetConstructor;
    private readonly Func<SyntaxGenerationContext, StatementSyntax> _initializationStatement;

    public IMemberOrNamedType ContextDeclaration { get; }

    public IMember TargetMember => this._targetConstructor;

    public SyntaxBasedInitializationTransformation(
        Advice advice,
        IMemberOrNamedType initializedDeclaration,
        IConstructor targetConstructor,
        Func<SyntaxGenerationContext, StatementSyntax> initializationStatement ) : base( advice )
    {
        this.ContextDeclaration = initializedDeclaration;
        this._targetConstructor = targetConstructor;
        this._initializationStatement = initializationStatement;
    }

    public IEnumerable<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context )
    {
        return new[]
        {
            new InsertedStatement(
                this._initializationStatement( context.SyntaxGenerationContext )
                    .WithGeneratedCodeAnnotation( this.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation )
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                this.ContextDeclaration )
        };
    }
}