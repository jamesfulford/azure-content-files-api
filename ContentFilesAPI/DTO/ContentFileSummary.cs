using System;

namespace ContentFilesAPI.DTO {
    /// <summary>
    /// Data Transfer Object to serialize a content file descriptor.
    /// </summary>
    public class ContentFileSummary {
        /// <summary>
        /// Name of file
        /// </summary>
        public string name;

        /// <summary>
        /// Creates an error.
        /// </summary>
        /// <param name="name">Name of file.</param>
        public ContentFileSummary (string name) {
            this.name = name;
        }
    }
}
