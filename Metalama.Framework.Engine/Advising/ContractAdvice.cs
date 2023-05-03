// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed class ContractAdvice : Advice
    {
        public ContractAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IDeclaration targetDeclaration,
            ICompilation sourceCompilation,
            string? layerName )
            : base( aspect, templateInstance, targetDeclaration, sourceCompilation, layerName ) { }

        public override AdviceKind AdviceKind => AdviceKind.AddContract;

        public override AdviceImplementationResult Implement(
            ProjectServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
#if DEBUG
            if ( this.LastAdviceImplementationResult != null )
            {
                throw new AssertionFailedException( "Implement has already been called." );
            }
#endif
            return this.LastAdviceImplementationResult = this.ImplementCore( serviceProvider, compilation, addTransformation );
        }

        public AdviceImplementationResult? LastAdviceImplementationResult { get; private set; }

        private AdviceImplementationResult ImplementCore(
            ProjectServiceProvider serviceProvider,
            CompilationModel compilation,
            Action<ITransformation> addTransformation )
        {
            var targetDeclaration = this.TargetDeclaration.GetTarget( compilation );

            switch ( targetDeclaration )
            {
                case IMethod { ContainingDeclaration: IProperty containingProperty } propertyAccessor:
                    addTransformation( new ContractPropertyTransformation( this, containingProperty, propertyAccessor.MethodKind ) );

                    return AdviceImplementationResult.Success( containingProperty );

                case IProperty property:
                    addTransformation( new ContractPropertyTransformation( this, property, null ) );

                    return AdviceImplementationResult.Success( property );

                case IMethod { ContainingDeclaration: IIndexer containingIndexer } indexerAccessor:
                    addTransformation( new ContractIndexerTransformation( this, containingIndexer, indexerAccessor.MethodKind ) );

                    return AdviceImplementationResult.Success( containingIndexer );

                case IIndexer indexer:
                    addTransformation( new ContractIndexerTransformation( this, indexer, null ) );

                    return AdviceImplementationResult.Success( indexer );

                case IMethod method:
                    addTransformation( new ContractMethodTransformation( this, method ) );

                    return AdviceImplementationResult.Success( method );

                case IConstructor constructor:
                    addTransformation( new ContractConstructorTransformation( this, constructor ) );

                    return AdviceImplementationResult.Success( constructor );

                case IField field:
                    var promotedField = new PromotedField( serviceProvider, field, ObjectReader.Empty, this );
                    addTransformation( promotedField.ToTransformation() );
                    OverrideHelper.AddTransformationsForStructField( field.DeclaringType, this, addTransformation );
                    addTransformation( new ContractPropertyTransformation( this, promotedField, null ) );

                    return AdviceImplementationResult.Success( promotedField );

                default:
                    throw new AssertionFailedException( $"Unexpected kind of declaration: '{targetDeclaration}'." );
            }
        }

        public List<Contract> Contracts { get; } = new();

        public bool TryExecuteTemplates(
            IDeclaration targetMember,
            TransformationContext context,
            ContractDirection direction,
            string? returnValueLocalName,
            Func<Contract, bool>? contractFilter,
            [NotNullWhen( true )] out List<StatementSyntax>? statements )
        {
            statements = null;

            foreach ( var contract in this.Contracts )
            {
                if ( !contract.AppliesTo( direction ) )
                {
                    continue;
                }

                var contractTarget = contract.TargetDeclaration.GetTarget( targetMember.Compilation );

                if ( contractFilter != null && !contractFilter( contract ) )
                {
                    continue;
                }

                var parameterName = contractTarget switch
                {
                    IParameter { IsReturnParameter: true } => returnValueLocalName.AssertNotNull(),
                    IParameter parameter => parameter.Name,
                    IFieldOrPropertyOrIndexer when direction == ContractDirection.Input => "value",
                    IFieldOrPropertyOrIndexer when direction == ContractDirection.Output => returnValueLocalName.AssertNotNull(),
                    _ => throw new AssertionFailedException( $"Unexpected kind of declaration: '{contractTarget}'." )
                };

                var parameterType = ((IHasType) contractTarget).Type;
                ExpressionSyntax parameterExpression = SyntaxFactory.IdentifierName( parameterName );
                parameterExpression = SymbolAnnotationMapper.AddExpressionTypeAnnotation( parameterExpression, parameterType.GetSymbol() );

                statements ??= new List<StatementSyntax>();

                var metaApiProperties = new MetaApiProperties(
                    this.SourceCompilation,
                    context.DiagnosticSink,
                    contract.Template.Cast(),
                    contract.Tags,
                    this.AspectLayerId,
                    context.SyntaxGenerationContext,
                    this.Aspect,
                    context.ServiceProvider,
                    MetaApiStaticity.Default );

                var metaApi = MetaApi.ForDeclaration(
                    contractTarget,
                    metaApiProperties,
                    direction );

                var boundTemplate = contract.Template.ForContract( parameterExpression, contract.TemplateArguments );

                var expansionContext = new TemplateExpansionContext(
                    context.ServiceProvider,
                    this.TemplateInstance.Instance,
                    metaApi,
                    context.LexicalScopeProvider.GetLexicalScope( targetMember ),
                    context.ServiceProvider.GetRequiredService<SyntaxSerializationService>(),
                    context.SyntaxGenerationContext,
                    boundTemplate,
                    null,
                    this.AspectLayerId );

                var templateDriver = this.TemplateInstance.TemplateClass.GetTemplateDriver( contract.Template.Declaration );

                if ( !templateDriver.TryExpandDeclaration( expansionContext, boundTemplate.TemplateArguments, out var filterBody ) )
                {
                    statements = null;

                    return false;
                }
                else
                {
                    statements.AddRange( filterBody.Statements );
                }
            }

            return statements is { Count: > 0 };
        }
    }
}