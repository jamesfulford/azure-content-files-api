using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentFilesAPI.Controllers {
    /// <summary>
    /// API for managing ContentFile resources
    /// </summary>
    [Route ("api/v1/{containername}/contentfiles")]
    [ApiController]
    public class ContentFilesController : ControllerBase {
        private const string GetContentFileByIdRoute = "GetContentFileByIdRoute";
        private const string ContainernameParamName = "containername";
        private const string FilenameParamName = "filename";

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The configuration instance
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Instantiate a ContentFilesController with Dependency Injection
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public ContentFilesController (
            ILogger<ContentFilesController> logger,
            IConfiguration configuration
        ) {
            _logger = logger;
            _configuration = configuration;
        }

        private string StorageConnectionString {
            get {
                return _configuration.GetConnectionString ("DefaultStorageConnection");
            }
        }

        /// <summary>
        /// Creates/updates file with given filename.
        /// </summary>
        /// <param name="containername"></param>
        /// <param name="filename"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPut ("{filename}")]
        [ProducesResponseType (typeof (void), (int) HttpStatusCode.Created)]
        [ProducesResponseType (typeof (void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public IActionResult PutFile ([FromRoute] string containername, [FromRoute] string filename, IFormFile file) {
            try {
                DTO.ErrorResponse err;
                err = DetectErrorInResourceName (containername, ContainernameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                err = DetectErrorInResourceName (filename, FilenameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                // TODO(jamesfulford): Implement cloud storage
                return CreatedAtRoute (GetContentFileByIdRoute, new { containername, filename });
            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.GetItem, ex, $"Error while putting {containername}:{filename}");
                return StatusCode ((int) HttpStatusCode.InternalServerError, MakeUnknownErrorResponse ());
            }
        }

        /// <summary>
        /// Updates file with given filename.
        /// </summary>
        /// <param name="containername"></param>
        /// <param name="filename"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPatch ("{filename}")]
        [ProducesResponseType (typeof (void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.NotFound)]
        public IActionResult UpdateFile ([FromRoute] string containername, [FromRoute] string filename, IFormFile file) {
            try {
                DTO.ErrorResponse err;
                err = DetectErrorInResourceName (containername, ContainernameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                err = DetectErrorInResourceName (filename, FilenameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                // TODO(jamesfulford): Implement cloud storage
                return NoContent ();
                // return NotFound();
            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.GetItem, ex, $"Error while updating {containername}:{filename}");
                return StatusCode ((int) HttpStatusCode.InternalServerError, MakeUnknownErrorResponse ());
            }
        }

        /// <summary>
        /// Deletes file with given filename.
        /// </summary>
        /// <param name="containername"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpDelete ("{filename}")]
        [ProducesResponseType (typeof (void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.NotFound)]
        public IActionResult DeleteFile ([FromRoute] string containername, [FromRoute] string filename) {
            try {
                DTO.ErrorResponse err;
                err = DetectErrorInResourceName (containername, ContainernameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                err = DetectErrorInResourceName (filename, FilenameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                // TODO(jamesfulford): Implement cloud storage
                return NoContent ();
                // return NotFound();
            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.GetItem, ex, $"Error while deleting contentfile {containername}:{filename}");
                return StatusCode ((int) HttpStatusCode.InternalServerError, MakeUnknownErrorResponse ());
            }
        }

        /// <summary>
        /// Get a ContentFile by name
        /// </summary>
        /// <param name="containername"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpGet ("{filename}")]
        [ProducesResponseType (typeof (string), (int) HttpStatusCode.OK)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.NotFound)]
        public ActionResult<string> GetFileByFileName ([FromRoute] string containername, [FromRoute] string filename) {
            try {
                DTO.ErrorResponse err;
                err = DetectErrorInResourceName (containername, ContainernameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                err = DetectErrorInResourceName (filename, FilenameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                // TODO(jamesfulford): Implement cloud storage
                return containername + filename;
                // return NotFound();
            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.GetItem, ex, $"Error while getting contentfile {filename}");
                return StatusCode ((int) HttpStatusCode.InternalServerError, MakeUnknownErrorResponse ());
            }
        }

        /// <summary>
        /// Get all ContentFiles
        /// </summary>
        /// <param name="containername"></param>
        /// <returns>A list of all content files.</returns>
        [HttpGet]
        [ProducesResponseType (typeof (IEnumerable<DTO.ContentFileSummary>), (int) HttpStatusCode.OK)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.NotFound)]
        public ActionResult<IEnumerable<DTO.ContentFileSummary>> GetAllFiles ([FromRoute] string containername) {
            try {
                DTO.ErrorResponse err;
                err = DetectErrorInResourceName (containername, ContainernameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                return new DTO.ContentFileSummary[] {
                    new DTO.ContentFileSummary (containername),
                        new DTO.ContentFileSummary (containername),
                };
            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.GetItem, ex, "Error while getting all contentfiles");
                return StatusCode ((int) HttpStatusCode.InternalServerError, MakeUnknownErrorResponse ());
            }
        }

        private DTO.ErrorResponse DetectErrorInResourceName (string val, string parameterName, int min = 1, int max = 75) {
            if (val == null) {
                return new DTO.ErrorResponse (DTO.ErrorNumber.NOTNULL, parameterName, val);
            }
            if (val == "") {
                return new DTO.ErrorResponse (DTO.ErrorNumber.REQUIRED, parameterName, val);
            }
            if (val.Length < min) {
                return new DTO.ErrorResponse (DTO.ErrorNumber.TOOSMALL, parameterName, val);
            }
            if (val.Length > max) {
                return new DTO.ErrorResponse (DTO.ErrorNumber.TOOLARGE, parameterName, val);
            }
            return null;
        }

        private DTO.ErrorResponse MakeUnknownErrorResponse () {
            return new DTO.ErrorResponse (DTO.ErrorNumber.UNKNOWN);
        }
    }
}
