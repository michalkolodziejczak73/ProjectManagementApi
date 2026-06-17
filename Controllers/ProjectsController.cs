using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.Data;
using ProjectManagementApi.DTOs.Projects;
using ProjectManagementApi.Models;
using System.Security.Claims;

namespace ProjectManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetProjects()
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var projects = await _context.Projects
                .AsNoTracking()
                .Where(project =>
                    project.OwnerId == userId ||
                    project.Tasks.Any(task =>
                        task.AssignedUserId == userId))
                .Select(project => new ProjectResponseDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    CreatedAt = project.CreatedAt,
                    OwnerId = project.OwnerId,
                    OwnerEmail = project.Owner.Email ?? string.Empty,
                    TasksCount = project.Tasks.Count,
                    IsOwner = project.OwnerId == userId
                })
                .OrderByDescending(project => project.CreatedAt)
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProjectResponseDto>> GetProject(int id)
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .AsNoTracking()
                .Where(project =>
                    project.Id == id &&
                    (
                        project.OwnerId == userId ||
                        project.Tasks.Any(task =>
                            task.AssignedUserId == userId)
                    ))
                .Select(project => new ProjectResponseDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    CreatedAt = project.CreatedAt,
                    OwnerId = project.OwnerId,
                    OwnerEmail = project.Owner.Email ?? string.Empty,
                    TasksCount = project.Tasks.Count,
                    IsOwner = project.OwnerId == userId
                })
                .FirstOrDefaultAsync();

            if (project is null)
            {
                return NotFound(new
                {
                    message = "Projekt nie istnieje lub nie masz do niego dostępu."
                });
            }

            return Ok(project);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectResponseDto>> CreateProject(
            ProjectCreateDto projectDto)
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var project = new Project
            {
                Name = projectDto.Name.Trim(),
                Description = projectDto.Description?.Trim(),
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var ownerEmail = User.FindFirstValue(ClaimTypes.Email)
                ?? string.Empty;

            var response = new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                OwnerId = project.OwnerId,
                OwnerEmail = ownerEmail,
                TasksCount = 0,
                IsOwner = true
            };

            return CreatedAtAction(
                nameof(GetProject),
                new { id = project.Id },
                response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProject(
            int id,
            ProjectUpdateDto projectDto)
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(project => project.Id == id);

            if (project is null)
            {
                return NotFound(new
                {
                    message = "Projekt nie istnieje."
                });
            }

            if (project.OwnerId != userId)
            {
                return Forbid();
            }

            project.Name = projectDto.Name.Trim();
            project.Description = projectDto.Description?.Trim();

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(project => project.Id == id);

            if (project is null)
            {
                return NotFound(new
                {
                    message = "Projekt nie istnieje."
                });
            }

            if (project.OwnerId != userId)
            {
                return Forbid();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}