using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// A base class for all custom attributes that influence the scope (compile-time or run-time) of the code
    /// or its role in an aspect.
    /// </summary>
    public abstract class ScopeAttribute : Attribute { }
}