using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Invokers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Substituted;

internal sealed class SubstitutedParameterList : IParameterList
{
    private readonly SubstitutedMethod _method;
    private readonly ImmutableArray<SubstitutedParameter> _parameters;

    public SubstitutedParameterList( SubstitutedMethod method, ImmutableArray<SubstitutedParameter> parameters )
    {
        this._method = method;
        this._parameters = parameters;
    }

    public IParameter this[ string name ]
        => this._parameters.SingleOrDefault( p => p.Name == name ) ??
           throw new ArgumentOutOfRangeException( nameof(name), $"The method '{this._method}' does not contain a parameter named '{name}'" );

    public IParameter this[ int index ] => this._parameters[index];

    public int Count => this._parameters.Length;

    public IEnumerator<IParameter> GetEnumerator() => this._parameters.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public object ToValueArray() => new ValueArrayExpression( this );
}