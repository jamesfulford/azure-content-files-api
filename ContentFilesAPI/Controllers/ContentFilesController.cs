using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

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
        public async Task<IActionResult> PutFile ([FromRoute] string containerName, [FromRoute] string fileName, IFormFile fileData) {
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

                CloudBlobContainer container = GetContainer (containerName);
                IdempotentCreateContainer (container, containerName.ToLower ().Contains ("public"));
                CloudBlockBlob blob = GetBlob (container, fileName);
                bool preExisting = await BlobExists (blob);
                await OverwriteBlob (blob, fileData);

                if (preExisting) {
                    _logger.LogInformation (Common.LoggingEvents.InsertItem, $"Inserted {containerName}:{fileName}");
                    return NoContent ();
                } else {
                    _logger.LogInformation (Common.LoggingEvents.UpdateItem, $"Updated {containerName}:{fileName} via put");
                    return CreatedAtRoute (GetContentFileByIdRoute, new { containerName, fileName }, null);
                }
            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.InsertItem, ex, $"Error while putting {containerName}:{fileName}");
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
        public async Task<IActionResult> UpdateFile ([FromRoute] string containerName, [FromRoute] string fileName, IFormFile fileData) {
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

                CloudBlobContainer container = GetContainer (containerName);
                if (!await ContainerExists (container)) {
                    _logger.LogWarning (Common.LoggingEvents.UpdateItemNotFound, $"Could not find containerName: {containerName}:{fileName}");
                    return NotFound (new DTO.ErrorResponse (DTO.ErrorNumber.NOTFOUND, ContainerNameParamName, containerName));
                }
                CloudBlockBlob blob = GetBlob (container, fileName);
                if (await BlobExists (blob)) {
                    _logger.LogInformation (Common.LoggingEvents.UpdateItem, $"Updated {containerName}:{fileName} via patch");
                    await OverwriteBlob (blob, fileData);
                    return NoContent ();
                } else {
                    _logger.LogWarning (Common.LoggingEvents.UpdateItemNotFound, $"Could not find fileName: {containerName}:{fileName}");
                    return NotFound (new DTO.ErrorResponse (DTO.ErrorNumber.NOTFOUND, FileNameParamName, fileName));
                }

            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.UpdateItem, ex, $"Error while updating {containerName}:{fileName}");
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
        public async Task<IActionResult> DeleteFile ([FromRoute] string containerName, [FromRoute] string fileName) {
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

                CloudBlobContainer container = GetContainer (containerName);
                if (!await ContainerExists (container)) {
                    _logger.LogWarning (Common.LoggingEvents.DeleteItemNotFound, $"Could not find containerName: {containerName}:{fileName}");
                    return NotFound (new DTO.ErrorResponse (DTO.ErrorNumber.NOTFOUND, ContainerNameParamName, containerName));
                }
                CloudBlockBlob blob = GetBlob (container, fileName);
                if (await BlobExists (blob)) {
                    _logger.LogWarning (Common.LoggingEvents.DeleteItem, $"Deleted {containerName}:{fileName}");
                    await blob.DeleteAsync ();
                    return NoContent ();
                } else {
                    _logger.LogWarning (Common.LoggingEvents.DeleteItemNotFound, $"Could not find fileName: {containerName}:{fileName}");
                    return NotFound (new DTO.ErrorResponse (DTO.ErrorNumber.NOTFOUND, FileNameParamName, fileName));
                }
            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.DeleteItem, ex, $"Error while deleting contentfile {containerName}:{fileName}");
                return StatusCode ((int) HttpStatusCode.InternalServerError, MakeUnknownErrorResponse ());
            }
        }

        /// <summary>
        /// Get a ContentFile by name
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpGet ("{fileName}", Name = GetContentFileByIdRoute)]
        [ProducesResponseType (typeof (string), (int) HttpStatusCode.OK)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType (typeof (DTO.ErrorResponse), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetFileByFileName ([FromRoute] string containerName, [FromRoute] string fileName) {
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

                CloudBlobContainer container = GetContainer (containerName);
                if (!await ContainerExists (container)) {
                    _logger.LogWarning (Common.LoggingEvents.GetItemNotFound, $"Could not find containerName: {containerName}:{fileName}");
                    return NotFound (new DTO.ErrorResponse (DTO.ErrorNumber.NOTFOUND, ContainerNameParamName, containerName));
                }
                CloudBlockBlob blob = GetBlob (container, fileName);
                if (await BlobExists (blob)) {
                    _logger.LogWarning (Common.LoggingEvents.GetItem, $"Got {containerName}:{fileName}");
                    return File (await blob.OpenReadAsync (), blob.Properties.ContentType);
                } else {
                    _logger.LogWarning (Common.LoggingEvents.GetItemNotFound, $"Could not find fileName: {containerName}:{fileName}");
                    return NotFound (new DTO.ErrorResponse (DTO.ErrorNumber.NOTFOUND, FileNameParamName, fileName));
                }
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
        public async Task<ActionResult> GetAllFiles ([FromRoute] string containerName) {
            try {
                DTO.ErrorResponse err;
                err = DetectErrorInResourceName (containerName, ContainerNameParamName);
                if (err != null) {
                    return BadRequest (err);
                }

                CloudBlobContainer container = GetContainer (containerName);
                if (!await ContainerExists (container)) {
                    _logger.LogWarning (Common.LoggingEvents.GetItemNotFound, $"Could not find containerName: {containerName} for get all");
                    return NotFound (new DTO.ErrorResponse (DTO.ErrorNumber.NOTFOUND, ContainerNameParamName, containerName));
                }

                BlobResultSegment result = await container.ListBlobsSegmentedAsync (null, true, BlobListingDetails.Metadata, null, null, new BlobRequestOptions (), new OperationContext ());
                if (result?.Results != null) {
                    return Ok (
                        result.Results
                        .Where (b => b is CloudBlockBlob)
                        .Select (b => new DTO.ContentFileSummary (((CloudBlockBlob) b).Name))
                    );
                }
                return Ok (new List<DTO.ContentFileSummary> { });
            } catch (Exception ex) {
                _logger.LogError (Common.LoggingEvents.GetItem, ex, $"Error while getting all contentfiles from {containerName}");
                return StatusCode ((int) HttpStatusCode.InternalServerError, MakeUnknownErrorResponse ());
            }
        }

        private DTO.ErrorResponse DetectErrorInResourceName (string val, string parameterName, int min = 1, int max = 75) {
            if (val == null) {
                return new DTO.ErrorResponse (DTO.ErrorNumber.NOTNULL, parameterName, val);
            }
            if (string.IsNullOrWhiteSpace (val)) {
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

        private CloudBlobContainer GetContainer (string containerName) {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse (StorageConnectionString);
            CloudBlobClient BlobClient = storageAccount.CreateCloudBlobClient ();
            return BlobClient.GetContainerReference (containerName.ToLower ());
        }

        private async Task<bool> ContainerExists (CloudBlobContainer container) {
            return container != null && await container.ExistsAsync ();
        }

        private async void IdempotentCreateContainer (CloudBlobContainer container, bool makePublic) {
            await container.CreateIfNotExistsAsync ();
            BlobContainerPermissions perm = new BlobContainerPermissions { PublicAccess = makePublic ? BlobContainerPublicAccessType.Blob : BlobContainerPublicAccessType.Off };
            await container.SetPermissionsAsync (perm);
        }

        private CloudBlockBlob GetBlob (CloudBlobContainer container, string blobId) {
            return container.GetBlockBlobReference (blobId);
        }

        private async Task<bool> BlobExists (CloudBlockBlob blob) {
            return blob != null && !blob.IsDeleted && await blob.ExistsAsync ();
        }

        private async Task OverwriteBlob (CloudBlockBlob blob, IFormFile file) {
            using (Stream uploadedFileStream = file.OpenReadStream ()) {
                blob.Properties.ContentType = file.ContentType;
                await blob.UploadFromStreamAsync (uploadedFileStream);
            }
        }
    }
}
