// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Exceptions.cs - C# port of Exceptions.h
//
// This file defines the exception hierarchy for OpenVDB.

using System;

namespace OpenVDB
{
    /// <summary>
    /// Base class for all OpenVDB exceptions.
    /// </summary>
    public class OpenVDBException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the OpenVDBException class.
        /// </summary>
        public OpenVDBException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the OpenVDBException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public OpenVDBException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the OpenVDBException class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public OpenVDBException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when an arithmetic error occurs.
    /// </summary>
    public class ArithmeticException : OpenVDBException
    {
        /// <summary>Initializes a new instance of the ArithmeticException class.</summary>
        public ArithmeticException() : base() { }
        /// <summary>Initializes a new instance with a specified error message.</summary>
        public ArithmeticException(string message) : base(message) { }
        /// <summary>Initializes a new instance with a message and inner exception.</summary>
        public ArithmeticException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when an index is out of range.
    /// </summary>
    public class IndexException : OpenVDBException
    {
        /// <summary>Initializes a new instance of the IndexException class.</summary>
        public IndexException() : base() { }
        /// <summary>Initializes a new instance with a specified error message.</summary>
        public IndexException(string message) : base(message) { }
        /// <summary>Initializes a new instance with a message and inner exception.</summary>
        public IndexException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when an I/O error occurs.
    /// </summary>
    public class IoException : OpenVDBException
    {
        /// <summary>Initializes a new instance of the IoException class.</summary>
        public IoException() : base() { }
        /// <summary>Initializes a new instance with a specified error message.</summary>
        public IoException(string message) : base(message) { }
        /// <summary>Initializes a new instance with a message and inner exception.</summary>
        public IoException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a key is not found.
    /// </summary>
    public class KeyException : OpenVDBException
    {
        /// <summary>Initializes a new instance of the KeyException class.</summary>
        public KeyException() : base() { }
        /// <summary>Initializes a new instance with a specified error message.</summary>
        public KeyException(string message) : base(message) { }
        /// <summary>Initializes a new instance with a message and inner exception.</summary>
        public KeyException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a lookup fails.
    /// </summary>
    public class LookupException : OpenVDBException
    {
        /// <summary>Initializes a new instance of the LookupException class.</summary>
        public LookupException() : base() { }
        /// <summary>Initializes a new instance with a specified error message.</summary>
        public LookupException(string message) : base(message) { }
        /// <summary>Initializes a new instance with a message and inner exception.</summary>
        public LookupException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a feature is not implemented.
    /// </summary>
    public class NotImplementedException : OpenVDBException
    {
        /// <summary>Initializes a new instance of the NotImplementedException class.</summary>
        public NotImplementedException() : base() { }
        /// <summary>Initializes a new instance with a specified error message.</summary>
        public NotImplementedException(string message) : base(message) { }
        /// <summary>Initializes a new instance with a message and inner exception.</summary>
        public NotImplementedException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a reference error occurs.
    /// </summary>
    public class ReferenceException : OpenVDBException
    {
        /// <summary>Initializes a new instance of the ReferenceException class.</summary>
        public ReferenceException() : base() { }
        /// <summary>Initializes a new instance with a specified error message.</summary>
        public ReferenceException(string message) : base(message) { }
        /// <summary>Initializes a new instance with a message and inner exception.</summary>
        public ReferenceException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a runtime error occurs.
    /// </summary>
    public class RuntimeException : OpenVDBException
    {
        /// <summary>Initializes a new instance of the RuntimeException class.</summary>
        public RuntimeException() : base() { }
        /// <summary>Initializes a new instance with a specified error message.</summary>
        public RuntimeException(string message) : base(message) { }
        /// <summary>Initializes a new instance with a message and inner exception.</summary>
        public RuntimeException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a type error occurs.
    /// </summary>
    public class TypeException : OpenVDBException
    {
        /// <summary>Initializes a new instance of the TypeException class.</summary>
        public TypeException() : base() { }
        /// <summary>Initializes a new instance with a specified error message.</summary>
        public TypeException(string message) : base(message) { }
        /// <summary>Initializes a new instance with a message and inner exception.</summary>
        public TypeException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a value error occurs.
    /// </summary>
    public class ValueException : OpenVDBException
    {
        /// <summary>Initializes a new instance of the ValueException class.</summary>
        public ValueException() : base() { }
        /// <summary>Initializes a new instance with a specified error message.</summary>
        public ValueException(string message) : base(message) { }
        /// <summary>Initializes a new instance with a message and inner exception.</summary>
        public ValueException(string message, Exception innerException) : base(message, innerException) { }
    }
}
