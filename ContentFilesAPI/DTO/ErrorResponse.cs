using System;

namespace ContentFilesAPI.DTO {
    /// <summary>
    /// Enumeration of errors to error numbers.
    /// </summary>
    public enum ErrorNumber : long {
        /// <summary>
        /// Item already exists.
        /// </summary>
        EXISTS = 1,

        /// <summary>
        /// Parameter is too large/long.
        /// </summary>
        TOOLARGE = 2,

        /// <summary>
        /// Parameter is required.
        /// </summary>
        REQUIRED = 3,

        /// <summary>
        /// Item with given id is not found.
        /// </summary>
        NOTFOUND = 4,

        /// <summary>
        /// Parameter too small.
        /// </summary>
        TOOSMALL = 5,

        /// <summary>
        /// Parameter cannot be null.
        /// </summary>
        NOTNULL = 6,

        /// <summary>
        /// An unexpected error occurred.
        /// </summary>
        UNKNOWN = 7,
    }

    /// <summary>
    /// Data Transfer Object to serialize various error responses.
    /// </summary>
    public class ErrorResponse {
        /// <summary>
        /// Number corresponding to type of error.
        /// </summary>
        public ErrorNumber errorNumber;

        /// <summary>
        /// Parameter name being referenced in the error, if relevant to error.
        /// </summary>
        public string parameterName;

        /// <summary>
        /// Parameter value violating validation, if relevant to error.
        /// </summary>
        public string parameterValue;

        /// <summary>
        /// A developer-only description of the error.
        /// </summary>
        public string errorDescription;

        /// <summary>
        /// Creates an error.
        /// </summary>
        /// <param name="errorNumber">Error number, indicating type of problem.</param>
        /// <param name="parameterName">Key holding faulty value, if relevant to error.</param>
        /// <param name="parameterValue">Faulty value converted to a string, if relevant to error.</param>
        public ErrorResponse (ErrorNumber errorNumber, string parameterName = null, string parameterValue = null) {
            this.errorNumber = errorNumber;
            this.parameterName = parameterName;
            this.parameterValue = parameterValue;
            switch (errorNumber) {
                case ErrorNumber.EXISTS:
                    this.errorDescription = "The entity already exists";
                    break;
                case ErrorNumber.TOOLARGE:
                    this.errorDescription = "The parameter value is too large";
                    break;
                case ErrorNumber.REQUIRED:
                    this.errorDescription = "The parameter is required";
                    break;
                case ErrorNumber.NOTFOUND:
                    this.errorDescription = "The entity could not be found";
                    break;
                case ErrorNumber.TOOSMALL:
                    this.errorDescription = "The parameter value is too small";
                    break;
                case ErrorNumber.NOTNULL:
                    this.errorDescription = "The parameter cannot be null";
                    break;
                case ErrorNumber.UNKNOWN:
                default:
                    this.errorDescription = "An unknown error occurred";
                    break;
            }
        }
    }
}
