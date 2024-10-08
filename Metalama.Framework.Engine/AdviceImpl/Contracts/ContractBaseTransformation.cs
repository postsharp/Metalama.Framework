﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.AdviceImpl.Contracts;

internal abstract class ContractBaseTransformation : BaseSyntaxTreeTransformation, IInsertStatementTransformation
{
    private readonly TemplateMember<IMethod> _template;
    private readonly IObjectReader _templateArguments;
    private readonly IObjectReader _tags;

    /// <summary>
    /// Gets the target member of the contract into which contract statements will be inserted.
    /// </summary>
    public abstract IFullRef<IMember> TargetMember { get; }

    /// <summary>
    /// Gets the declaration on which the contract was applied on.
    /// </summary>
    protected IRef<IDeclaration> ContractTarget { get; }

    /// <summary>
    /// Gets the contract direction of inserted statements.
    /// </summary>
    protected ContractDirection ContractDirection { get; }

    public TemplateProvider TemplateProvider { get; }

    protected ContractBaseTransformation(
        AdviceInfo advice,
        IFullRef<IDeclaration> contractTarget,
        ContractDirection contractDirection,
        TemplateMember<IMethod> template,
        TemplateProvider templateProvider,
        IObjectReader templateArguments,
        IObjectReader tags ) : base( advice, contractTarget )
    {
        Invariant.Assert( contractDirection is not ContractDirection.None );

        this.ContractTarget = contractTarget;
        this.ContractDirection = contractDirection;
        this.TemplateProvider = templateProvider;
        this._template = template;
        this._templateArguments = templateArguments;
        this._tags = tags;
    }

    public override IRef<IDeclaration> TargetDeclaration => this.TargetMember;

    public abstract IReadOnlyList<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context );

    protected bool TryExecuteTemplate(
        TransformationContext context,
        ExpressionSyntax valueExpression,
        IType valueType,
        [NotNullWhen( true )] out BlockSyntax? contractBlock )
    {
        var annotatedValueExpression = TypeAnnotationMapper.AddExpressionTypeAnnotation( valueExpression, valueType );
        var boundTemplate = this._template.ForContract( annotatedValueExpression, this.TemplateProvider, this._templateArguments );

        var metaApiProperties = new MetaApiProperties(
            this.OriginalCompilation,
            context.DiagnosticSink,
            this._template.AsMemberOrNamedType(),
            this._tags,
            this.AspectLayerId,
            context.SyntaxGenerationContext,
            this.AspectInstance,
            context.ServiceProvider,
            MetaApiStaticity.Default );

        var metaApi = MetaApi.ForDeclaration(
            this.ContractTarget.GetTarget( context.Compilation ),
            metaApiProperties,
            this.ContractDirection );

        var expansionContext = new TemplateExpansionContext(
            context,
            metaApi,
            this.TargetMember.GetTarget( context.Compilation ),
            boundTemplate,
            null,
            this.AspectLayerId );

        var templateDriver = this._template.Driver;

        return templateDriver.TryExpandDeclaration( expansionContext, boundTemplate.TemplateArguments, out contractBlock );
    }

    public override TransformationObservability Observability => TransformationObservability.None;

    public override IntrospectionTransformationKind TransformationKind => IntrospectionTransformationKind.InsertStatement;
}