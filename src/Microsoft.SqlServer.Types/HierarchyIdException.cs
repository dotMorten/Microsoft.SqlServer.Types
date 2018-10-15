using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.SqlServer.Types
{
    /// <summary>
    /// The exception that is thrown for invalid SqlHierarchyId values.
    /// </summary>
    [Serializable]
    [CLSCompliant(true)]
    public class HierarchyIdException : Exception
    {
                /// <summary>
        /// Initializes a new instance of the <see cref="HierarchyIdException"/> class.
        /// </summary>
        /// <remarks>
        /// <para>This is the default constructor for the <see cref="HierarchyIdException"/> class.</para>
        /// <para>This creates an exception object by using a default error message.</para>
        /// </remarks>
        public HierarchyIdException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchyIdException"/> class with a custom error message.
        /// </summary>
        /// <param name="message">A string that contains the custom error message. </param>
        /// <remarks>This constructor is called when an object throwing the exception is passing custom error information.</remarks>
        public HierarchyIdException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchyIdException"/> with a custom error message and the triggering exception object.
        /// </summary>
        /// <param name="message">A string that contains the error message </param>
        /// <param name="innerException">The exception instance that caused the current exception. </param>
        /// <remarks>The constructor is called by another exception object to transmit exception data upstream.</remarks>
        public HierarchyIdException(string message, Exception innerException)
            : base(message, innerException)
        {
        }   

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchyIdException"/> class with serialized data. 
        /// </summary>
        /// <param name="info">An object that contains the serialized object data about the exception that is thrown. </param>
        /// <param name="context">An object that contains the contextual information about the source or destination </param>
        /// <remarks>This constructor is called during deserialization to reconstitute the exception object transmitted over a stream.</remarks>
        protected HierarchyIdException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
