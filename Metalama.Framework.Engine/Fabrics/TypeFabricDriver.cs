// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Fabrics;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Fabrics;

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
        var compilation = aspectBuilder.Target.GetCompilationModel();

        // Prepare declarative advice.
        var declarativeAdvice = templateClass
            .GetDeclarativeAdvices( aspectBuilder.ServiceProvider, compilation )
            .ToList();

        // Execute the AmendType.
        var builder = new Amender( targetType, this.FabricManager, aspectBuilder, templateInstance, fabricInstance );

        var executionContext = new UserCodeExecutionContext(
            aspectBuilder.ServiceProvider,
            aspectBuilder.DiagnosticAdder,
            UserCodeMemberInfo.FromDelegate( new Action<ITypeAmender>( ((TypeFabric) this.Fabric).AmendType ) ) );

        return this.FabricManager.UserCodeInvoker.TryInvoke(
            () =>
            {
                // Execute declarative advice.
                foreach ( var advice in declarativeAdvice )
                {
                    ((DeclarativeAdviceAttribute) advice.AdviceAttribute.AssertNotNull()).BuildAdvice(
                        advice.Declaration.AssertNotNull(),
                        advice.TemplateClassMember.Key,
                        (IAspectBuilder<IDeclaration>) aspectBuilder );
                }

                if ( !aspectBuilder.IsAspectSkipped )
                {
                    ((TypeFabric) this.Fabric).AmendType( builder );
                }
            },
            executionContext );
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