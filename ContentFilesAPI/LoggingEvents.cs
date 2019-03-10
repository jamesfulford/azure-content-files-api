using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentFilesAPI.Common {
    /// <summary>
    /// Enumeration of logging levels for each type of action
    /// </summary>
    public class LoggingEvents {
        /// <summary>
        /// Logging for GetItem actions
        /// </summary>
        public const int GetItem = 1000;

        /// <summary>
        /// Logging for InsertItem actions
        /// </summary>
        public const int InsertItem = 1001;

        /// <summary>
        /// Logging for UpdateItem actions
        /// </summary>
        public const int UpdateItem = 1002;

        /// <summary>
        /// Logging for DeleteItem actions
        /// </summary>
        public const int DeleteItem = 1003;

        /// <summary>
        /// Logging for GetItem errors where item is not found
        /// </summary>
        public const int GetItemNotFound = 4000;

        /// <summary>
        /// Logging for UpdateItem where target item is not found
        /// </summary>
        public const int UpdateItemNotFound = 4001;

        /// <summary>
        /// Logging for DeleteItem where target item is not found
        /// </summary>
        public const int DeleteItemNotFound = 4002;

        /// <summary>
        /// Logging for any other internal server error
        /// </summary>
        public const int InternalError = 5000;
    }
}
