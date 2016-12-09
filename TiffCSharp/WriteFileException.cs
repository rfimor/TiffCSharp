using System;
using System.Collections.Generic;
using System.Text;

namespace TiffCSharp
{
    /// <summary>
    /// Exceptions in file writing.
    /// </summary>
    public class WriteFileException : Exception
    {
        public WriteFileException()
            : base("Error occured during file writing.")
        {
        }

        /// <summary>
        /// Create an instance of WriteFileException with a message.
        /// </summary>
        /// <param name="msg">Message of the exception.</param>
        public WriteFileException(string msg)
            : base("Error occured during file writing: " + msg)
        {
        }

        /// <summary>
        /// Create an instance of WriteFileException with a message and an inner exception.
        /// </summary>
        /// <param name="msg">Message of the exception.</param>
        /// <param name="inner">Inner exception.</param>
        public WriteFileException(string msg, Exception inner)
            : base("Error occured during file writing: " + msg, inner) {
        }
    }
}
