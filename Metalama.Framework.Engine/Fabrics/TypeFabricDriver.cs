// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Fabrics;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Fabrics
{
    /// <summary>
    /// Implementation of <see cref="FabricAspect{T}"/> for type-level fabrics.
    /// </summary>
    internal class TypeFabricDriver : FabricDriver
    {
        public TypeFabricDriver( FabricManager fabricManager, Fabric fabric, Compilation runTimeCompilation ) : base(
            fabricManager,
            fabric,
            runTimeCompilation ) { }

        private ISymbol TargetSymbol => this.FabricSymbol.ContainingType;

        public bool TryExecute( IAspectBuilderInternal aspectBuilder, FabricTemplateClass templateClass, FabricInstance fabricInstance )
        {
            var templateInstance = new TemplateClassInstance( this.Fabric, templateClass );
            var targetType = (INamedType) aspectBuilder.Target;

            // Add declarative advices.
            var aspectInstance = (IAspectInstanceInternal) aspectBuilder.AspectInstance;

            var declarativeAdvices =
                templateClass.GetDeclarativeAdvices()
                    .Select(
                        x => CreateDeclarativeAdvice(
                            aspectInstance,
                            aspectInstance.TemplateInstances[x.TemplateClass],
                            aspectBuilder.DiagnosticAdder,
                            targetType,
                            x.TemplateInfo,
                            x.Symbol ) )
                    .WhereNotNull();

            aspectBuilder.AdviceFactory.Advices.AddRange( declarativeAdvices );

            // Execute the AmendType.
            var builder = new Amender( targetType, this.FabricManager, aspectBuilder, templateInstance, fabricInstance );

            var executionContext = new UserCodeExecutionContext(
                this.FabricManager.ServiceProvider,
                aspectBuilder.DiagnosticAdder,
                UserCodeMemberInfo.FromDelegate( new Action<ITypeAmender>( ((TypeFabric) this.Fabric).AmendType ) ) );

            return this.FabricManager.UserCodeInvoker.TryInvoke( () => ((TypeFabric) this.Fabric).AmendType( builder ), executionContext );
        }

        private static Advice? CreateDeclarativeAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IDiagnosticAdder diagnosticAdder,
            INamedType aspectTarget,
            TemplateInfo template,
            ISymbol templateDeclaration )
        {
            template.TryCreateAdvice(
                aspect,
                templateInstance,
                diagnosticAdder,
                aspectTarget,
                ((CompilationModel) aspectTarget.Compilation).Factory.GetDeclaration( templateDeclaration ),
                null,
                out var advice );

            return advice;
        }

        public override FabricKind Kind => FabricKind.Type;

        public IDeclaration GetTarget( CompilationModel compilation ) => compilation.Factory.GetNamedType( (INamedTypeSymbol) this.TargetSymbol );

        public override FormattableString FormatPredecessor() => $"type fabric on '{this.TargetSymbol}'";

        private class Amender : BaseAmender<INamedType>, ITypeAmender
        {
            private readonly IAspectBuilderInternal _aspectBuilder;

            public Amender(
                INamedType namedType,
                FabricManager fabricManager,
                IAspectBuilderInternal aspectBuilder,
                TemplateClassInstance templateClassInstance,
                FabricInstance fabricInstance ) : base(
                namedType.Compilation.Project,
                fabricManager,
                fabricInstance,
                fabricInstance.TargetDeclaration.As<INamedType>() )
            {
                this._aspectBuilder = aspectBuilder;
                this.Type = namedType;
                this.Advices = aspectBuilder.AdviceFactory.WithTemplateClassInstance( templateClassInstance );
            }

            public INamedType Type { get; }

            public IAdviceFactory Advices { get; }

            public override void AddAspectSource( IAspectSource aspectSource ) => this._aspectBuilder.AddAspectSource( aspectSource );

            public override void AddValidatorSource( IValidatorSource validatorSource ) => this._aspectBuilder.AddValidatorSource( validatorSource );
        }
    }
}