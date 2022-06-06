// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Transformations
{
    internal class InitializationTransformation : IInsertStatementTransformation
    {
        private readonly IConstructor _targetConstructor;
        private readonly BoundTemplateMethod _boundTemplate;

        public Advice Advice { get; }

        public IMemberOrNamedType ContextDeclaration { get; }

        public SyntaxTree TargetSyntaxTree
            => this._targetConstructor.GetPrimaryDeclaration()?.SyntaxTree
               ?? this._targetConstructor.DeclaringType.GetPrimaryDeclaration().AssertNotNull().SyntaxTree;

        public IMethodBase TargetDeclaration => this._targetConstructor;

        public InitializationTransformation(
            Advice advice,
            IMemberOrNamedType initializedDeclaration,
            IConstructor targetConstructor,
            BoundTemplateMethod boundTemplate,
            IObjectReader tags )
        {
            this.ContextDeclaration = initializedDeclaration;
            this._targetConstructor = targetConstructor;
            this._boundTemplate = boundTemplate;
            this.Tags = tags;
            this.Advice = advice;
        }

        public InsertedStatement? GetInsertedStatement( InsertStatementTransformationContext context )
        {
            var metaApi = MetaApi.ForConstructor(
                this._targetConstructor,
                new MetaApiProperties(
                    context.DiagnosticSink,
                    this._boundTemplate.Template.Cast(),
                    this.Tags,
                    this.Advice.AspectLayerId,
                    context.SyntaxGenerationContext,
                    this.Advice.Aspect,
                    context.ServiceProvider,
                    this._targetConstructor.IsStatic ? MetaApiStaticity.AlwaysStatic : MetaApiStaticity.AlwaysInstance ) );

            var expansionContext = new TemplateExpansionContext(
                this.Advice.TemplateInstance.Instance,
                metaApi,
                (CompilationModel) this.ContextDeclaration.Compilation,
                context.LexicalScopeProvider.GetLexicalScope( this.ContextDeclaration ),
                context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                context.SyntaxGenerationContext,
                this._boundTemplate,
                null,
                this.Advice.AspectLayerId );

            var templateDriver = this.Advice.TemplateInstance.TemplateClass.GetTemplateDriver( this._boundTemplate.Template.Declaration! );

            if ( !templateDriver.TryExpandDeclaration( expansionContext, this._boundTemplate.TemplateArguments, out var expandedBody ) )
            {
                // Template expansion error.
                return default;
            }

            return new InsertedStatement(
                expandedBody
                    .WithGeneratedCodeAnnotation(
                        metaApi.AspectInstance?.AspectClass.GeneratedCodeAnnotation ?? FormattingAnnotations.SystemGeneratedCodeAnnotation )
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                this.ContextDeclaration );
        }

        public IObjectReader Tags { get; }
    }
}