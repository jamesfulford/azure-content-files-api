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
    [Route ("api/v1/{containername:length(1,75)}/contentfiles")]
    [ApiController]
    public class ContentFilesController : ControllerBase {
        private string GetContentFileByIdRoute = "GetContentFileByIdRoute";

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

        /// <summary>
        /// Creates/updates file with given filename.
        /// </summary>
        /// <param name="containername"></param>
        /// <param name="filename"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPut ("{filename:length(1,75)}")]
        [ProducesResponseType (typeof (void), (int) HttpStatusCode.Created)]
        [ProducesResponseType (typeof (void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public IActionResult PutFile (string containername, string filename, IFormFile file) {
            try {
                if (ModelState.IsValid) {
                    // TODO(jamesfulford): Implement cloud storage
                    return CreatedAtRoute (GetContentFileByIdRoute, new { containername, filename });
                    // return NoContent();
                } else {
                    return BadRequest (MakeInvalidDataAttributeResponse ());
                }
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
        [HttpPatch ("{filename:length(1,75)}")]
        [ProducesResponseType (typeof (void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.NotFound)]
        public IActionResult UpdateFile (string containername, string filename, IFormFile file) {
            try {
                if (ModelState.IsValid) {
                    // TODO(jamesfulford): Implement cloud storage
                    return NoContent ();
                    // return NotFound();
                } else {
                    return BadRequest (MakeInvalidDataAttributeResponse ());
                }
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
        [HttpDelete ("{filename:length(1,75)}")]
        [ProducesResponseType (typeof (void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.NotFound)]
        public IActionResult DeleteFile (string containername, string filename) {
            try {
                if (ModelState.IsValid) {
                    // TODO(jamesfulford): Implement cloud storage
                    return NoContent ();
                    // return NotFound();
                } else {
                    return BadRequest (MakeInvalidDataAttributeResponse ());
                }
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
        [HttpGet ("{filename:length(1,75)}")]
        [ProducesResponseType (typeof (string), (int) HttpStatusCode.OK)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.NotFound)]
        public ActionResult<string> GetByFileName (string containername, string filename) {
            try {
                if (ModelState.IsValid) {
                    // TODO(jamesfulford): Implement cloud storage
                    return containername + filename;
                } else {
                    return BadRequest (MakeInvalidDataAttributeResponse ());
                }
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
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.NotFound)]
        public ActionResult<IEnumerable<DTO.ContentFileSummary>> GetAll (string containername) {
            try {
                if (ModelState.IsValid) {
                    // TODO(jamesfulford): Implement cloud storage
                    return new DTO.ContentFileSummary[] {
                        new DTO.ContentFileSummary (containername),
                            new DTO.ContentFileSummary (containername),
                    };
                } else {
                    return BadRequest (MakeInvalidDataAttributeResponse ());
                }
            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.GetItem, ex, "Error while getting all contentfiles");
                return StatusCode ((int) HttpStatusCode.InternalServerError, MakeUnknownErrorResponse ());
            }
        }

        private DTO.ErrorNumber ErrorMessageToErrorNumber (string errorText) {
            // These strings are set in data attributes on input payload as ErrorMessages.
            switch (errorText) {
                case "2":
                    return DTO.ErrorNumber.TOOLARGE;
                case "3":
                    return DTO.ErrorNumber.REQUIRED;
                case "5":
                    return DTO.ErrorNumber.TOOSMALL;
                case "6":
                    return DTO.ErrorNumber.NOTNULL;
                default:
                    return DTO.ErrorNumber.UNKNOWN;

            }
        }

        private DTO.ErrorResponse MakeInvalidDataAttributeResponse () {
            foreach (var key in ModelState.Keys) {
                if (ModelState[key].ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid) {
                    foreach (var error in ModelState[key].Errors) {
                        return new DTO.ErrorResponse (
                            ErrorMessageToErrorNumber (error.ErrorMessage),
                            key,
                            ModelState[key].RawValue == null ? null : ModelState[key].RawValue.ToString ()
                        );
                    }
                }
            }
            throw new Exception ("ModelState is invalid, but has no erroring keys!");
        }

        private DTO.ErrorResponse MakeUnknownErrorResponse () {
            return new DTO.ErrorResponse (DTO.ErrorNumber.UNKNOWN);
        }
    }
}
