// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed class DesignTimeReferenceValidatorInstance
{
    private ulong _longHashCode;

    public SymbolDictionaryKey ValidatedDeclaration { get; }

    private readonly ReferenceKinds _referenceKinds;
    private readonly ValidatorDriver _driver;

    internal ValidatorImplementation Implementation { get; }

    internal DesignTimeReferenceValidatorInstance(
        ISymbol validatedDeclaration,
        ReferenceKinds referenceKinds,
        ValidatorDriver driver,
        ValidatorImplementation implementation )
    {
        this.ValidatedDeclaration = SymbolDictionaryKey.CreatePersistentKey( validatedDeclaration );
        this._referenceKinds = referenceKinds;
        this._driver = driver;
        this.Implementation = implementation;
    }

    internal ReferenceValidatorInstance ToReferenceValidationInstance( CompilationModel compilation )
        => new(
            compilation.Factory.GetDeclaration( this.ValidatedDeclaration ).AssertNotNull(),
            this._driver,
            this.Implementation,
            this._referenceKinds );

    internal ulong GetLongHashCode( XXH64 hasher )
    {
        if ( this._longHashCode == 0 )
        {
            // We expect a zero hasher, but if we receive something else, this does not matter. We are just returning the cumulative digest.

            this.ValidatedDeclaration.UpdateHash( hasher );
            hasher.Update( (int) this._referenceKinds );
            this.Implementation.UpdateHash( hasher );
            hasher.Update( this._driver.GetHashCode() );

            this._longHashCode = hasher.Digest();
        }

        return this._longHashCode;
    }

    public TransitiveValidatorInstance ToTransitiveValidatorInstance()
        => new(
            this.ValidatedDeclaration.ToRef(),
            this._referenceKinds,
            this.Implementation.Implementation,
            this.Implementation.State,
            this._driver.MethodName );
}