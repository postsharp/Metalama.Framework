// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
        Invariant.AssertNot( targetCtor.IsPrimary && targetCtor.DeclaringType.TypeKind is TypeKind.Class or TypeKind.Struct );

        addTransformation( new SyntaxBasedInitializationTransformation( this, targetDeclaration, targetCtor, _ => ((UserStatement) this._statement).Syntax ) );
    }
}