// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Transformations;

internal abstract class ContractBaseTransformation : BaseTransformation, IInsertStatementTransformation
{
    /// <summary>
    /// Gets the target member of the contract into which contract statements will be inserted.
    /// </summary>
    public IMember TargetMember { get; }

    /// <summary>
    /// Gets the declaration on which the contract was applied on.
    /// </summary>
    public IDeclaration ContractTarget { get; }

    /// <summary>
    /// Gets the contract direction of inserted statements.
    /// </summary>
    public ContractDirection ContractDirection { get; }

    /// <summary>
    /// Gets the template of the contract.
    /// </summary>
    public TemplateMember<IMethod> Template { get; }

    /// <summary>
    /// Gets the tags that will be passed to the template.
    /// </summary>
    public IObjectReader TemplateArguments { get; }

    /// <summary>
    /// Gets the tags that will be passed to the template.
    /// </summary>
    public IObjectReader Tags { get; }

    public ContractBaseTransformation( 
        Advice advice, 
        IMember targetMember, 
        IDeclaration contractTarget, 
        ContractDirection contractDirection,
        TemplateMember<IMethod> template, 
        IObjectReader templateArguments,
        IObjectReader tags ) : base( advice )
    {
        Invariant.Assert( contractDirection is not ContractDirection.None or ContractDirection.Default );

        this.TargetMember = targetMember;
        this.ContractTarget = contractTarget;
        this.ContractDirection = contractDirection;
        this.Template = template;
        this.TemplateArguments = templateArguments;
        this.Tags = tags;
    }

    public override IDeclaration TargetDeclaration => this.TargetMember;

    public abstract IReadOnlyList<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context );

    public bool TryExecuteTemplate(
        TransformationContext context,
        ExpressionSyntax valueExpression,
        IType valueType,
        [NotNullWhen( true )] out BlockSyntax? contractBlock )
    {
        var annotatedValueExpression = SymbolAnnotationMapper.AddExpressionTypeAnnotation( valueExpression, valueType.GetSymbol() );
        var boundTemplate = this.Template.ForContract( annotatedValueExpression, this.TemplateArguments );

        var metaApiProperties = new MetaApiProperties(
            this.ParentAdvice.SourceCompilation,
            context.DiagnosticSink,
            this.Template.Cast(),
            this.Tags,
            this.ParentAdvice.AspectLayerId,
            context.SyntaxGenerationContext,
            this.ParentAdvice.Aspect,
            context.ServiceProvider,
            MetaApiStaticity.Default );

        var metaApi = MetaApi.ForDeclaration(
            this.ContractTarget,
            metaApiProperties,
            this.ContractDirection );

        var expansionContext = new TemplateExpansionContext(
            context,
            this.ParentAdvice.TemplateInstance.TemplateProvider,
            metaApi,
            this.TargetMember,
            boundTemplate,
            null,
            this.ParentAdvice.AspectLayerId );

        var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( this.Template.Declaration );

        return templateDriver.TryExpandDeclaration( expansionContext, boundTemplate.TemplateArguments, out contractBlock );
    }

    public override TransformationObservability Observability => TransformationObservability.None;

    public override TransformationKind TransformationKind => TransformationKind.InsertStatement;
}