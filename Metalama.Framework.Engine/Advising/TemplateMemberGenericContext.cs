﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.GenericContexts;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Advising;

/// <summary>
/// An ad-hoc, single-use implementation of <see cref="GenericContext"/> based on a <see cref="TemplateMember"/> and its arguments.
/// Dot not support all features.
/// </summary>
internal sealed class TemplateMemberGenericContext : GenericContext
{
    private readonly TemplateMember _templateMember;
    private readonly IObjectReader _arguments;
    private readonly IMethodBase _targetMethod;

    public TemplateMemberGenericContext( TemplateMember templateMember, IObjectReader arguments, IMethodBase targetMethod )
    {
        this._templateMember = templateMember;
        this._arguments = arguments;
        this._targetMethod = targetMethod;
    }

    public bool HasError { get; private set; }

    internal override GenericContextKind Kind => GenericContextKind.Introduced;

    internal override ImmutableArray<IFullRef<IType>> TypeArguments => throw new NotSupportedException();

    private IType MapTypeParameter( int index, string name )
    {
        if ( this._templateMember.TemplateClassMember.TypeParameters[index].IsCompileTime )
        {
            return this._arguments[name] switch
            {
                IType typeArg => typeArg,
                Type type => TypeFactory.GetType( type ),
                _ => throw new AssertionFailedException( $"Unexpected value of type '{this._arguments[name]?.GetType()}'." )
            };
        }
        else if ( this._targetMethod is IMethod method )
        {
            var typeParameter = method.TypeParameters.FirstOrDefault( t => t.Name == name );

            if ( typeParameter != null )
            {
                return typeParameter;
            }
        }

        // Fallback: we have a mismatch, but no way better to signal it to the caller.
        // TODO: We may improve GenericContext with an overload that allows for graceful error handling.
        this.HasError = true;

        return this._targetMethod.GetCompilationModel().Factory.GetSpecialType( SpecialType.Void );
    }

    internal override IType Map( ITypeParameter typeParameter ) => this.MapTypeParameter( typeParameter.Index, typeParameter.Name );

    internal override IType Map( ITypeParameterSymbol typeParameterSymbol, CompilationModel compilation )
        => this.MapTypeParameter( typeParameterSymbol.Ordinal, typeParameterSymbol.Name );

    internal override GenericContext Map( GenericContext genericContext, RefFactory refFactory ) => throw new NotSupportedException();

    public override bool Equals( GenericContext? other ) => throw new NotSupportedException();

    protected override int GetHashCodeCore() => throw new NotSupportedException();

    protected override T TranslateSymbolIfNecessary<T>( T symbol )
    {
        // We have symbols from a different source compilation than target compilation, so we may have to translate.
        return this._targetMethod.GetCompilationContext().SymbolTranslator.Translate( symbol ).AssertSymbolNotNull();
    }
}