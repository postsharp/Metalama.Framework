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
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Transformations
{
    internal class InitializationTransformation : INonObservableTransformation, IInsertStatementTransformation
    {
        private readonly IConstructor _targetConstructor;
        private readonly TemplateMember<IMethod> _template;

        public Advice Advice { get; }

        public IMemberOrNamedType ContextDeclaration { get; private set; }

        public SyntaxTree TargetSyntaxTree => this._targetConstructor.GetPrimaryDeclaration()?.SyntaxTree ?? this._targetConstructor.DeclaringType.GetPrimaryDeclaration().AssertNotNull().SyntaxTree;

        public IMethodBase TargetDeclaration => this._targetConstructor;

        public InitializationTransformation(
            Advice advice,
            IMemberOrNamedType initializedDeclaration,
            IConstructor targetConstructor,
            TemplateMember<IMethod> template,
            InitializationReason reason )
        {
            this.ContextDeclaration = initializedDeclaration;
            this._targetConstructor = targetConstructor;
            this._template = template;
            this.Advice = advice;
        }

        public InsertedStatement? GetInsertedStatement( InsertStatementTransformationContext context )
        {
            var metaApi = MetaApi.ForDeclaration(
                this.ContextDeclaration,
                new MetaApiProperties(
                    context.DiagnosticSink,
                    this._template.Cast(),
                    this.Advice.ReadOnlyTags,
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
                this._template,
                null,
                this.Advice.AspectLayerId );

            var templateDriver = this.Advice.TemplateInstance.TemplateClass.GetTemplateDriver( this._template.Declaration! );

            if ( !templateDriver.TryExpandDeclaration( expansionContext, context.DiagnosticSink, out var expandedBody ) )
            {
                // Template expansion error.
                return default;
            }

            return new InsertedStatement( 
                InsertedStatementPosition.Beginning, 
                expandedBody
                .WithGeneratedCodeAnnotation()
                .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ) );
        }
    }
}