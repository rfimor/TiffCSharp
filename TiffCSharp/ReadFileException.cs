using System;
using System.Collections.Generic;
using System.Text;

namespace TiffCSharp
{
    /// <summary>
    /// Exceptions in file reading.
    /// </summary>
    public class ReadFileException : Exception
    {
        public ReadFileException()
            : base("Error occured during file reading.")
        {
        }

        /// <summary>
        /// Create an instance of ReadFileException with a message.
        /// </summary>
        /// <param name="msg">Message of the exception.</param>
        public ReadFileException(string msg)
            : base("Error occured during file reading:" + msg)
        {
        }

        /// <summary>
        /// Create an instance of ReadFileException with a message and an inner exception
        /// </summary>
        /// <param name="msg">Message of the exception.</param>
        /// <param name="inner">Inner exception.</param>
        public ReadFileException(string msg, Exception inner)
            : base("Error occured during file reading:" + msg, inner) {
        }
    }
}
