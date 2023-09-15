// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.AspectConfiguration;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Fabrics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Fabrics;

/// <summary>
/// Implementation of <see cref="FabricAspect{T}"/> for type-level fabrics.
/// </summary>
internal sealed class TypeFabricDriver : FabricDriver
{
    private readonly string _targetTypeFullName;

    private TypeFabricDriver( CreationData creationData, INamedTypeSymbol targetType ) : base( creationData )
    {
        this._targetTypeFullName = targetType.AssertNotNull().GetFullName().AssertNotNull();
    }

    public static IEnumerable<TypeFabricDriver> Create(
        FabricManager fabricManager,
        CompileTimeProject compileTimeProject,
        Fabric fabric,
        CompilationModel compilation )
    {
        var creationData = GetCreationData( fabricManager, compileTimeProject, fabric, compilation.RoslynCompilation );

        if ( creationData.FabricType.ContainingAssembly.Equals( compilation.RoslynCompilation.Assembly ) )
        {
            yield return new TypeFabricDriver( creationData, creationData.FabricType.ContainingType );
        }

        if ( creationData.FabricType.GetAttributes().Any( a => a.AttributeClass?.Name == nameof(InheritableAttribute) ) )
        {
            foreach ( var derivedType in compilation.GetDerivedTypes( compilation.Factory.GetNamedType( creationData.FabricType.ContainingType ) ) )
            {
                yield return new TypeFabricDriver( creationData, derivedType.GetSymbol() );
            }
        }
    }

    public bool TryExecute( IAspectBuilderInternal aspectBuilder, FabricTemplateClass templateClass, FabricInstance fabricInstance )
    {
        var templateInstance = new TemplateClassInstance( TemplateProvider.FromInstance( this.Fabric ), templateClass );
        var targetType = (INamedType) aspectBuilder.Target;
        var compilation = aspectBuilder.Target.GetCompilationModel();

        // Prepare declarative advice.
        var declarativeAdvice = templateClass
            .GetDeclarativeAdvice( aspectBuilder.ServiceProvider, compilation )
            .ToReadOnlyList();

        // Execute the AmendType.
        var builder = new Amender( targetType, this.FabricManager, aspectBuilder, templateInstance, fabricInstance );

        var executionContext = new UserCodeExecutionContext(
            aspectBuilder.ServiceProvider,
            aspectBuilder.DiagnosticAdder,
            UserCodeDescription.Create( "calling the AmendType method for the fabric {0}", this.Fabric.GetType() ),
            compilationModel: compilation );

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

    public IDeclaration? GetTargetIfInPartialCompilation( CompilationModel compilation )
    {
        var symbol = compilation.RoslynCompilation.GetTypeByMetadataName( this._targetTypeFullName );

        if ( symbol == null || (compilation.PartialCompilation.IsPartial && !compilation.PartialCompilation.Types.Contains( symbol )) )
        {
            return null;
        }

        return compilation.Factory.GetNamedType( symbol );
    }

    public override FormattableString FormatPredecessor() => $"type fabric on '{this._targetTypeFullName}'";

    private sealed class Amender : BaseAmender<INamedType>, ITypeAmender
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
            this.Advice = aspectBuilder.AdviceFactory.WithTemplateClassInstance( templateClassInstance );
        }

        public INamedType Type { get; }

        public IAdviceFactory Advice { get; }

        public override void AddAspectSource( IAspectSource aspectSource ) => this._aspectBuilder.AddAspectSource( aspectSource );

        public override void AddValidatorSource( IValidatorSource validatorSource ) => this._aspectBuilder.AddValidatorSource( validatorSource );

        public override void AddConfiguratorSource( IConfiguratorSource configuratorSource ) => this._aspectBuilder.AddConfiguratorSource( configuratorSource );
    }
}