﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Builders;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Advices
{
    internal class InitializeAdvice : Advice
    {
        public TemplateMember<IMethod> Template { get; }

        public InitializationReason Reason { get; }

        public new IMemberOrNamedType TargetDeclaration => (IMemberOrNamedType) base.TargetDeclaration;

        public InitializeAdvice(
            IAspectInstanceInternal aspect,
            TemplateClassInstance templateInstance,
            IMemberOrNamedType targetDeclaration,
            TemplateMember<IMethod> template,
            InitializationReason reason,
            string? layerName,
            Dictionary<string, object?>? tags ) : base( aspect, templateInstance, targetDeclaration, layerName, tags )
        {
            this.Template = template;
            this.Reason = reason;
        }

        public override void Initialize( IDiagnosticAdder diagnosticAdder )
        {
            // TODO: Everything.
        }

        public override AdviceResult ToResult( ICompilation compilation, IReadOnlyList<IObservableTransformation> observableTransformations )
        {
            var containingType =
                this.TargetDeclaration switch
                {
                    INamedType t => t,
                    IMember m => m.DeclaringType,
                    _ => throw new AssertionFailedException(),
                };

            // TODO: We don't want to include constructors that call other constructor of this class, i.e. ": this(...)".
            var syntaxTrees =
                ( containingType.StaticConstructor != null && this.Reason.HasFlag( InitializationReason.TypeConstructing )
                    ? new IConstructor[] { containingType.StaticConstructor }
                    : Array.Empty<IConstructor>() )
                .Concat( containingType.Constructors.Where( x => !x.IsStatic && this.Reason.HasFlag( InitializationReason.Constructing ) ) )
                .GroupBy(x => (x, x.GetSymbol()) switch 
                    { 
                        (_, not null and var s) => s.GetPrimarySyntaxReference()?.SyntaxTree ?? s.ContainingType.GetPrimarySyntaxReference().AssertNotNull().SyntaxTree,
                        (ISyntaxTreeTransformation t, null) => t.TargetSyntaxTree,
                        _ => throw new AssertionFailedException(),
                    } );

            var initializations = new List<InitializationTransformation>();
            InitializationTransformation? mainInitialization = null;

            foreach (var syntaxTreeConstructors in syntaxTrees)
            {
                var initialization = new InitializationTransformation( 
                    this, 
                    mainInitialization,
                    containingType, 
                    (TypeDeclarationSyntax)containingType.GetSymbol().DeclaringSyntaxReferences.Single( x=> x.SyntaxTree == syntaxTreeConstructors.Key ).GetSyntax(), 
                    syntaxTreeConstructors.ToArray(), 
                    this.Template,
                    this.Reason );

                if (mainInitialization == null)
                {
                    mainInitialization = initialization;
                }

                initializations.Add( initialization );
            }

            return AdviceResult.Create( initializations );
        }
    }
}