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
    [Route ("api/v1/{containerName}/contentfiles")]
    [ApiController]
    public class ContentFilesController : ControllerBase {
        private const string GetContentFileByIdRoute = "GetContentFileByIdRoute";
        private const string ContainerNameParamName = "containerName";
        private const string FileNameParamName = "fileName";

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
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPut ("{fileName}")]
        [ProducesResponseType (typeof (void), (int) HttpStatusCode.Created)]
        [ProducesResponseType (typeof (void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public IActionResult PutFile ([FromRoute] string containerName, [FromRoute] string fileName, IFormFile fileData) {
            try {
                DTO.ErrorResponse err;
                err = DetectErrorInResourceName (containerName, ContainerNameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                err = DetectErrorInResourceName (fileName, FileNameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                err = DetectErrorInFile (fileData);
                if (err != null) {
                    return BadRequest (err);
                }

                // TODO(jamesfulford): Implement cloud storage
                return CreatedAtRoute (GetContentFileByIdRoute, new { containerName, fileName });
            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.GetItem, ex, $"Error while putting {containerName}:{fileName}");
                return StatusCode ((int) HttpStatusCode.InternalServerError, MakeUnknownErrorResponse ());
            }
        }

        /// <summary>
        /// Updates file with given filename.
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPatch ("{fileName}")]
        [ProducesResponseType (typeof (void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.NotFound)]
        public IActionResult UpdateFile ([FromRoute] string containerName, [FromRoute] string fileName, IFormFile fileData) {
            try {
                DTO.ErrorResponse err;
                err = DetectErrorInResourceName (containerName, ContainerNameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                err = DetectErrorInResourceName (fileName, FileNameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                err = DetectErrorInFile (fileData);
                if (err != null) {
                    return BadRequest (err);
                }

                // TODO(jamesfulford): Implement cloud storage
                return NoContent ();
                // return NotFound();
            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.GetItem, ex, $"Error while updating {containerName}:{fileName}");
                return StatusCode ((int) HttpStatusCode.InternalServerError, MakeUnknownErrorResponse ());
            }
        }

        /// <summary>
        /// Deletes file with given filename.
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpDelete ("{fileName}")]
        [ProducesResponseType (typeof (void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.NotFound)]
        public IActionResult DeleteFile ([FromRoute] string containerName, [FromRoute] string fileName) {
            try {
                DTO.ErrorResponse err;
                err = DetectErrorInResourceName (containerName, ContainerNameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                err = DetectErrorInResourceName (fileName, FileNameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                // TODO(jamesfulford): Implement cloud storage
                return NoContent ();
                // return NotFound();
            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.GetItem, ex, $"Error while deleting contentfile {containerName}:{fileName}");
                return StatusCode ((int) HttpStatusCode.InternalServerError, MakeUnknownErrorResponse ());
            }
        }

        /// <summary>
        /// Get a ContentFile by name
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpGet ("{fileName}")]
        [ProducesResponseType (typeof (string), (int) HttpStatusCode.OK)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.NotFound)]
        public ActionResult<string> GetFileByFileName ([FromRoute] string containerName, [FromRoute] string fileName) {
            try {
                DTO.ErrorResponse err;
                err = DetectErrorInResourceName (containerName, ContainerNameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                err = DetectErrorInResourceName (fileName, FileNameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                // TODO(jamesfulford): Implement cloud storage
                return containerName + fileName;
                // return NotFound();
            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.GetItem, ex, $"Error while getting contentfile {fileName}");
                return StatusCode ((int) HttpStatusCode.InternalServerError, MakeUnknownErrorResponse ());
            }
        }

        /// <summary>
        /// Get all ContentFiles
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns>A list of all content files.</returns>
        [HttpGet]
        [ProducesResponseType (typeof (IEnumerable<DTO.ContentFileSummary>), (int) HttpStatusCode.OK)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.NotFound)]
        public ActionResult<IEnumerable<DTO.ContentFileSummary>> GetAllFiles ([FromRoute] string containerName) {
            try {
                DTO.ErrorResponse err;
                err = DetectErrorInResourceName (containerName, ContainerNameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                return new DTO.ContentFileSummary[] {
                    new DTO.ContentFileSummary (containerName),
                        new DTO.ContentFileSummary (containerName),
                };
            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.GetItem, ex, $"Error while getting all contentfiles from {containerName}");
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

        private DTO.ErrorResponse DetectErrorInFile (IFormFile fileData) {
            if (fileData == null) {
                return new DTO.ErrorResponse (DTO.ErrorNumber.NOTNULL, "fileData", null);
            }
            if (fileData.Length <= 0) {
                return new DTO.ErrorResponse (DTO.ErrorNumber.TOOSMALL, "fileData", "");
            }
            return null;
        }

        private DTO.ErrorResponse MakeUnknownErrorResponse () {
            return new DTO.ErrorResponse (DTO.ErrorNumber.UNKNOWN);
        }
    }
}
