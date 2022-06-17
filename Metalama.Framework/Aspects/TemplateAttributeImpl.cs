using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Aspects;

internal struct TemplateAttributeImpl
{
    private Accessibility? _accessibility;
    private bool? _isVirtual;
    private bool? _isSealed;

    public string? Name { get; set; }

    public Accessibility Accessibility
    {
        get
            => this._accessibility
               ?? throw new InvalidOperationException(
                   $"The '{nameof(this.Accessibility)}' was not set, use {nameof(this.GetAccessibility)} to get nullable value." );
        set => this._accessibility = value;
    }

    public bool IsVirtual
    {
        get
            => this._isVirtual
               ?? throw new InvalidOperationException( $"The 'Virtual' property was not set, use {nameof(this.GetIsVirtual)} to get nullable value." );
        set => this._isVirtual = value;
    }

    public bool IsSealed
    {
        get
            => this._isSealed
               ?? throw new InvalidOperationException( $"The 'IsSealed' property was not set, use {nameof(this.GetIsSealed)} to get nullable value." );
        set => this._isSealed = value;
    }

    public bool? GetIsVirtual() => this._isVirtual;

    public bool? GetIsSealed() => this._isSealed;

    public Accessibility? GetAccessibility() => this._accessibility;
}