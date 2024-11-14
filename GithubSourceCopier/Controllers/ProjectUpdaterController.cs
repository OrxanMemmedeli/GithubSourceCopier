using GithubSourceCopier.Models;
using GithubSourceCopier.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GithubSourceCopier.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectUpdaterController : ControllerBase
    {
        private readonly IProjectUpdaterService _projectUpdaterService;

        public ProjectUpdaterController(IProjectUpdaterService projectUpdaterService)
        {
            _projectUpdaterService = projectUpdaterService;
        }

        [HttpPost("update-project")]
        public async Task<IActionResult> UpdateProject([FromBody] ProjectUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GitHubLink) || string.IsNullOrWhiteSpace(request.LocalPath))
            {
                return BadRequest("GitHub linki və lokal yol tələb olunur.");
            }

            try
            {
                await _projectUpdaterService.DownloadAndCopyFilesAsync(request.GitHubLink, request.LocalPath, request.OldVersion, request.NewVersion);
                return Ok("Fayllar uğurla kopyalandı.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Xəta baş verdi: {ex.Message}");
            }
        }
    }
}
