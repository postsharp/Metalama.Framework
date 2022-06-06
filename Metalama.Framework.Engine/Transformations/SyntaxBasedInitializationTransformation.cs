// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Transformations;

internal class SyntaxBasedInitializationTransformation : IInsertStatementTransformation
{
    private readonly IConstructor _targetConstructor;
    private readonly Func<SyntaxGenerationContext, StatementSyntax> _initializationStatement;

    public Advice Advice { get; }

    public IMemberOrNamedType ContextDeclaration { get; }

    public SyntaxTree TargetSyntaxTree
        => this._targetConstructor.GetPrimaryDeclaration()?.SyntaxTree
           ?? this._targetConstructor.DeclaringType.GetPrimaryDeclaration().AssertNotNull().SyntaxTree;

    public IMethodBase TargetDeclaration => this._targetConstructor;

    public SyntaxBasedInitializationTransformation(
        Advice advice,
        IMemberOrNamedType initializedDeclaration,
        IConstructor targetConstructor,
        Func<SyntaxGenerationContext, StatementSyntax> initializationStatement,
        IObjectReader tags )
    {
        this.ContextDeclaration = initializedDeclaration;
        this._targetConstructor = targetConstructor;
        this._initializationStatement = initializationStatement;
        this.Tags = tags;
        this.Advice = advice;
    }

    public InsertedStatement? GetInsertedStatement( InsertStatementTransformationContext context )
    {
        return new InsertedStatement(
            this._initializationStatement( context.SyntaxGenerationContext )
                .WithGeneratedCodeAnnotation( this.Advice.Aspect.AspectClass.GeneratedCodeAnnotation )
                .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
            this.ContextDeclaration );
    }

    public IObjectReader Tags { get; }
}