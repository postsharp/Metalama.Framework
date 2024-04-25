// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Transformations;
using System;

namespace Metalama.Framework.Engine.Advising;

internal sealed class SyntaxBasedInitializeAdvice : InitializeAdvice
{
    private readonly IStatement _statement;

    public SyntaxBasedInitializeAdvice(
        IAspectInstanceInternal aspectInstance,
        TemplateClassInstance templateInstance,
        IMemberOrNamedType targetDeclaration,
        ICompilation sourceCompilation,
        IStatement statement,
        InitializerKind kind,
        string? layerName ) : base( aspectInstance, templateInstance, targetDeclaration, sourceCompilation, kind, layerName )
    {
        this._statement = statement;
    }

    protected override void AddTransformation( IMemberOrNamedType targetDeclaration, IConstructor targetCtor, Action<ITransformation> addTransformation )
    {
        addTransformation( new SyntaxBasedInitializationTransformation( this, targetDeclaration, targetCtor, _ => ((UserStatement) this._statement).Syntax ) );
    }
}