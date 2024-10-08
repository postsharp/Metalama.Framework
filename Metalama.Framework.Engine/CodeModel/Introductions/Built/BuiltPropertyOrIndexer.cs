// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Generic;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal abstract class BuiltPropertyOrIndexer : BuiltMember, IPropertyOrIndexerImpl
{
    protected BuiltPropertyOrIndexer( CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext ) { }

    protected abstract PropertyOrIndexerBuilderData PropertyOrIndexerBuilder { get; }

    public RefKind RefKind => this.PropertyOrIndexerBuilder.RefKind;

    public Writeability Writeability => this.PropertyOrIndexerBuilder.Writeability;

    [Memo]
    public IType Type => this.MapType( this.PropertyOrIndexerBuilder.Type ).AssertNotNull();

    [Memo]
    public IMethod? GetMethod
        => this.PropertyOrIndexerBuilder.GetMethod != null
            ? new BuiltAccessor( this, this.PropertyOrIndexerBuilder.GetMethod )
            : null;

    [Memo]
    public IMethod? SetMethod
        => this.PropertyOrIndexerBuilder.SetMethod != null
            ? new BuiltAccessor( this, this.PropertyOrIndexerBuilder.SetMethod )
            : null;

    IRef<IFieldOrPropertyOrIndexer> IFieldOrPropertyOrIndexer.ToRef() => (IRef<IFieldOrPropertyOrIndexer>) this.ToFullDeclarationRef();

    public PropertyInfo ToPropertyInfo() => CompileTimePropertyInfo.Create( this );

    IRef<IPropertyOrIndexer> IPropertyOrIndexer.ToRef() => (IRef<IPropertyOrIndexer>) this.ToFullDeclarationRef();

    public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

    public IEnumerable<IMethod> Accessors
    {
        get
        {
            return (this.GetMethod, this.SetMethod) switch
            {
                (null, { } setMethod) => [setMethod],
                ({ } getMethod, null) => [getMethod],
                ({ } getMethod, { } setMethod) => [getMethod, setMethod],
                _ => []
            };
        }
    }
}