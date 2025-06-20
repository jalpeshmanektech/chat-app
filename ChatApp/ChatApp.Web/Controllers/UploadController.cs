using ChatApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        [HttpPost]
        [RequestSizeLimit(10_485_760)] // 10 MB limit
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Server-side validation
            if (!FileTypeHelper.IsFileTypeAllowed(file.FileName))
            {
                return BadRequest($"File type not allowed. Allowed types: {FileTypeHelper.GetAllowedExtensionsString()}");
            }

            if (!FileTypeHelper.IsFileSizeAllowed(file.Length))
            {
                return BadRequest("File size exceeds the 10MB limit.");
            }

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/{fileName}";
            return Ok(new { url });
        }
    }
} 