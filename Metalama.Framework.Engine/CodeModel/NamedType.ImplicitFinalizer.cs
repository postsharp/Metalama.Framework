// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Metrics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Accessibility = Metalama.Framework.Code.Accessibility;
using MethodKind = Metalama.Framework.Code.MethodKind;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed partial class NamedType
    {
        private class ImplicitFinalizer : IFinalizerImpl, ISdkDeclaration
        {
            public INamedType DeclaringType { get; }

            public ImplicitFinalizer( INamedType declaringType )
            {
                this.DeclaringType = declaringType;
            }

            public ISymbol? Symbol => null;

            public IAssembly DeclaringAssembly => this.DeclaringType.DeclaringAssembly;

            public DeclarationOrigin Origin => DeclarationOrigin.Source;

            public IDeclaration? ContainingDeclaration => this.DeclaringType;

            public IAttributeCollection Attributes => AttributeCollection.Empty;

            public DeclarationKind DeclarationKind => DeclarationKind.Finalizer;

            public ICompilation Compilation => this.DeclaringType.Compilation;

            public IFinalizer? OverriddenFinalizer
            {
                get
                {
                    // This will chain implicit finalizers directly, which is possible without a cycle.
                    var currentType = this.DeclaringType.BaseType;

                    if ( currentType?.Finalizer != null )
                    {
                        return currentType.Finalizer;
                    }

                    return null;
                }
            }

            public MethodKind MethodKind => MethodKind.Finalizer;

            public bool IsVirtual => true;

            public bool IsAsync => false;

            // This is true only for value types and System.Object.
            public bool IsOverride => this.OverriddenFinalizer != null;

            public bool IsExplicitInterfaceImplementation => false;

            public bool IsImplicit => true;

            public Accessibility Accessibility => Accessibility.Protected;

            public bool IsAbstract => false;

            public bool IsStatic => false;

            public bool IsSealed => false;

            public bool IsNew => false;

            public string Name => "Finalize";

            public IParameterList Parameters => ParameterList.Empty;

            public IMember? OverriddenMember => this.OverriddenFinalizer;

            public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

            public bool CanBeInherited => true;

            public SyntaxTree? PrimarySyntaxTree => this.DeclaringType.GetPrimaryDeclaration().AssertNotNull().SyntaxTree;

            public IDeclaration OriginalDefinition => this;

            public Location? DiagnosticLocation => this.DeclaringType.GetDiagnosticLocation();

            public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            {
                StringBuilder stringBuilder = new();
                stringBuilder.Append( this.DeclaringType.ToDisplayString( format, context ) );
                stringBuilder.Append( '.' );
                stringBuilder.Append( this.Name );

                return stringBuilder.ToString();
            }

            public MemberInfo ToMemberInfo()
            {
                throw new NotImplementedException();
            }

            public System.Reflection.MethodBase ToMethodBase()
            {
                throw new NotImplementedException();
            }

            public IRef<IDeclaration> ToRef() => Ref.FromImplicitMember( this.DeclaringType, t => ((INamedType) t).Finalizer.AssertNotNull() );

            Ref<IDeclaration> IDeclarationImpl.ToRef() => Ref.FromImplicitMember( this.DeclaringType, t => ((INamedType) t).Finalizer.AssertNotNull() );

            public IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true )
            {
                foreach ( var derivedType in this.Compilation.GetDerivedTypes( this.DeclaringType, true ) )
                {
                    yield return derivedType.Finalizer.AssertNotNull();
                }
            }

            public T GetMetric<T>()
                where T : IMetric
                => ((IDeclarationImpl) this.DeclaringType).GetMetric<T>();
        }
    }
}