﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Introspection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Transformations
{
    internal class TemplateBasedInitializationTransformation : BaseTransformation, IInsertStatementTransformation
    {
        private readonly IConstructor _targetConstructor;
        private readonly BoundTemplateMethod _boundTemplate;

        public IMemberOrNamedType ContextDeclaration { get; }

        public IMember TargetMember => this._targetConstructor;

        public TemplateBasedInitializationTransformation(
            Advice advice,
            IMemberOrNamedType initializedDeclaration,
            IConstructor targetConstructor,
            BoundTemplateMethod boundTemplate,
            IObjectReader tags ) : base( advice )
        {
            this.ContextDeclaration = initializedDeclaration;
            this._targetConstructor = targetConstructor;
            this._boundTemplate = boundTemplate;
            this.Tags = tags;
        }

        public IEnumerable<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context )
        {
            var metaApi = MetaApi.ForConstructor(
                this._targetConstructor,
                new MetaApiProperties(
                    this.ParentAdvice.SourceCompilation,
                    context.DiagnosticSink,
                    this._boundTemplate.Template.Cast(),
                    this.Tags,
                    this.ParentAdvice.AspectLayerId,
                    context.SyntaxGenerationContext,
                    this.ParentAdvice.Aspect,
                    context.ServiceProvider,
                    this._targetConstructor.IsStatic ? MetaApiStaticity.AlwaysStatic : MetaApiStaticity.AlwaysInstance ) );

            var expansionContext = new TemplateExpansionContext(
                this.ParentAdvice.TemplateInstance.Instance,
                metaApi,
                context.LexicalScopeProvider.GetLexicalScope( this.ContextDeclaration ),
                context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                context.SyntaxGenerationContext,
                this._boundTemplate.Template,
                null,
                this.ParentAdvice.AspectLayerId );

            var templateDriver = this.ParentAdvice.TemplateInstance.TemplateClass.GetTemplateDriver( this._boundTemplate.Template.Declaration );

            if ( !templateDriver.TryExpandDeclaration( expansionContext, this._boundTemplate.TemplateArguments, out var expandedBody ) )
            {
                // Template expansion error.
                return Enumerable.Empty<InsertedStatement>();
            }

            return new[]
            {
                new InsertedStatement(
                    expandedBody
                        .WithGeneratedCodeAnnotation(
                            metaApi.AspectInstance?.AspectClass.GeneratedCodeAnnotation ?? FormattingAnnotations.SystemGeneratedCodeAnnotation )
                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                    this.ContextDeclaration )
            };
        }

        public IObjectReader Tags { get; }

        public override IDeclaration TargetDeclaration => this.TargetMember;

        public override TransformationObservability Observability => TransformationObservability.None;

        public override TransformationKind TransformationKind => TransformationKind.InsertStatement;

        public override FormattableString ToDisplayString() => $"Add a statement to '{this._targetConstructor}'.";
    }
}