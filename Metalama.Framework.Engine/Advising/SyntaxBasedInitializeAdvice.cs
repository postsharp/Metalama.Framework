// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advising;

internal class SyntaxBasedInitializeAdvice : InitializeAdvice
{
    private readonly IStatement _statement;

    public SyntaxBasedInitializeAdvice(
        IAspectInstanceInternal aspect,
        TemplateClassInstance templateInstance,
        IMemberOrNamedType targetDeclaration,
        ICompilation sourceCompilation,
        IStatement statement,
        InitializerKind kind,
        string? layerName ) : base( aspect, templateInstance, targetDeclaration, sourceCompilation, kind, layerName )
    {
        this._statement = statement;
    }

    protected override void AddTransformation( IMemberOrNamedType targetDeclaration, IConstructor targetCtor, Action<ITransformation> addTransformation )
    {
        addTransformation( new SyntaxBasedInitializationTransformation( this, targetDeclaration, targetCtor, _ => ((UserStatement) this._statement).Syntax ) );
    }
}