// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Introspection;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.AdviceImpl.Initialization;

internal sealed class TemplateBasedInitializationTransformation : BaseSyntaxTreeTransformation, IInsertStatementTransformation
{
    private readonly IRef<IConstructor> _targetConstructor;
    private readonly BoundTemplateMethod _boundTemplate;

    private IRef<IMemberOrNamedType> ContextDeclaration { get; }

    public IRef<IMember> TargetMember => this._targetConstructor;

    public TemplateBasedInitializationTransformation(
        Advice advice,
        IRef<IMemberOrNamedType> initializedDeclaration,
        IRef<IConstructor> targetConstructor,
        BoundTemplateMethod boundTemplate,
        IObjectReader tags ) : base( advice )
    {
        this.ContextDeclaration = initializedDeclaration;
        this._targetConstructor = targetConstructor;
        this._boundTemplate = boundTemplate;
        this.Tags = tags;
    }

    public IReadOnlyList<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context )
    {
        var targetConstructor = this._targetConstructor.GetTarget( context.Compilation );
        var contextDeclaration = this.ContextDeclaration.GetTarget( context.Compilation );

        var metaApi = MetaApi.ForConstructor(
            targetConstructor,
            new MetaApiProperties(
                this.OriginalCompilation,
                context.DiagnosticSink,
                this._boundTemplate.TemplateMember.AsMemberOrNamedType(),
                this.Tags,
                this.AspectLayerId,
                context.SyntaxGenerationContext,
                this.AspectInstance,
                context.ServiceProvider,
                targetConstructor.IsStatic ? MetaApiStaticity.AlwaysStatic : MetaApiStaticity.AlwaysInstance ) );

        var expansionContext = new TemplateExpansionContext(
            context,
            metaApi,
            contextDeclaration,
            this._boundTemplate,
            null,
            this.AspectLayerId );

        var templateDriver = this._boundTemplate.TemplateMember.Driver;

        if ( !templateDriver.TryExpandDeclaration( expansionContext, this._boundTemplate.TemplateArguments, out var expandedBody ) )
        {
            // Template expansion error.
            return Array.Empty<InsertedStatement>();
        }

        return
        [
            new InsertedStatement(
                expandedBody
                    .AssertNotNull()
                    .WithGeneratedCodeAnnotation(
                        metaApi.AspectInstance?.AspectClass.GeneratedCodeAnnotation ?? FormattingAnnotations.SystemGeneratedCodeAnnotation )
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                contextDeclaration,
                this,
                InsertedStatementKind.Initializer )
        ];
    }

    private IObjectReader Tags { get; }

    public override IRef<IDeclaration> TargetDeclaration => this.TargetMember;

    public override TransformationObservability Observability => TransformationObservability.None;

    public override IntrospectionTransformationKind TransformationKind => IntrospectionTransformationKind.InsertStatement;

    public override FormattableString ToDisplayString( CompilationModel compilation ) => $"Add a statement to '{this._targetConstructor}'.";
}