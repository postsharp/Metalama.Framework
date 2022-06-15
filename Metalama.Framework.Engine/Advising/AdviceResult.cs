using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Engine.Advising;

/// <summary>
/// Represents the result of a method of <see cref="IAdviceFactory"/>.
/// </summary>
/// <typeparam name="T">The type of declaration returned by the advice method.</typeparam>
internal class AdviceResult<T> : IIntroductionAdviceResult<T>, IOverrideAdviceResult<T>, IImplementInterfaceAdviceResult, IAddContractAdviceResult<T>, IAddInitializerAdviceResult,
                                 IAddAttributeAdviceResult, IRemoveAttributesAdviceResult, IAppendParameterAdviceResult
    where T : class, IDeclaration
{
    private readonly IRef<T> _declaration;
    private readonly ICompilation _compilation;

    /// <summary>
    /// Gets the declaration created or transformed by the advice method. For introduction advice methods, this is the introduced declaration when a new
    /// declaration is introduced, or the existing declaration when a declaration of the same name and signature already exists. For advice that modify a field,
    /// this is the property that now represents the field.
    /// </summary>
    public T Declaration
        => this.Outcome != AdviceOutcome.Error
            ? this._declaration.GetTarget( this._compilation, ReferenceResolutionOptions.CanBeMissing )
            : throw new InvalidOperationException( "Cannot get the resulting declaration when the outcome is Error." );

    public AdviceOutcome Outcome { get; }

    internal AdviceResult( IRef<T> declaration, ICompilation compilation, AdviceOutcome outcome )
    {
        this._declaration = declaration;
        this._compilation = compilation;
        this.Outcome = outcome;
    }
}