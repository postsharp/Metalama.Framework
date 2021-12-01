// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    /// <summary>
    /// Interface that can be implemented by serializable classes.
    /// It defines methods <see cref="OnDeserialized"/> and <see cref="OnSerializing"/> called during serialization.
    /// </summary>
    public interface ISerializationCallback
    {
        /// <summary>
        /// Method called after the object has been deserialized.
        /// </summary>
        void OnDeserialized();

        /// <summary>
        /// Method called before the object is being serialized.
        /// </summary>
        void OnSerializing();
    }

}