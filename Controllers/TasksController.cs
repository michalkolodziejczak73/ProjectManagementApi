using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.Data;
using ProjectManagementApi.DTOs.Tasks;
using ProjectManagementApi.Models;
using System.Security.Claims;

namespace ProjectManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Pobiera zadania dostępne dla zalogowanego użytkownika.
        /// </summary>
        /// <remarks>
        /// Właściciel projektu widzi wszystkie zadania w swoich projektach.
        /// Przypisany użytkownik widzi zadania przypisane bezpośrednio do niego.
        /// </remarks>
        /// <response code="200">Lista dostępnych zadań.</response>
        /// <response code="401">Użytkownik nie jest zalogowany.</response>
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasks()
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var tasks = await _context.TaskItems
                .AsNoTracking()
                .Where(task =>
                    task.Project.OwnerId == userId ||
                    task.AssignedUserId == userId)
                .Select(task => new TaskResponseDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    IsCompleted = task.IsCompleted,
                    CreatedAt = task.CreatedAt,
                    DueDate = task.DueDate,
                    ProjectId = task.ProjectId,
                    ProjectName = task.Project.Name,
                    AssignedUserId = task.AssignedUserId,
                    AssignedUserEmail = task.AssignedUser != null
                        ? task.AssignedUser.Email
                        : null,
                    IsProjectOwner = task.Project.OwnerId == userId,
                    IsAssignedToCurrentUser =
                        task.AssignedUserId == userId
                })
                .OrderByDescending(task => task.CreatedAt)
                .ToListAsync();

            return Ok(tasks);
        }

        /// <summary>
        /// Pobiera pojedyncze zadanie.
        /// </summary>
        /// <param name="id">Identyfikator zadania.</param>
        /// <response code="200">Zadanie zostało znalezione.</response>
        /// <response code="401">Użytkownik nie jest zalogowany.</response>
        /// <response code="404">Zadanie nie istnieje lub użytkownik nie ma do niego dostępu.</response>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var task = await _context.TaskItems
                .AsNoTracking()
                .Where(task =>
                    task.Id == id &&
                    (
                        task.Project.OwnerId == userId ||
                        task.AssignedUserId == userId
                    ))
                .Select(task => new TaskResponseDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    IsCompleted = task.IsCompleted,
                    CreatedAt = task.CreatedAt,
                    DueDate = task.DueDate,
                    ProjectId = task.ProjectId,
                    ProjectName = task.Project.Name,
                    AssignedUserId = task.AssignedUserId,
                    AssignedUserEmail = task.AssignedUser != null
                        ? task.AssignedUser.Email
                        : null,
                    IsProjectOwner = task.Project.OwnerId == userId,
                    IsAssignedToCurrentUser =
                        task.AssignedUserId == userId
                })
                .FirstOrDefaultAsync();

            if (task is null)
            {
                return NotFound(new
                {
                    message = "Zadanie nie istnieje lub nie masz do niego dostępu."
                });
            }

            return Ok(task);
        }

        /// <summary>
        /// Pobiera zadania należące do wskazanego projektu.
        /// </summary>
        /// <param name="projectId">Identyfikator projektu.</param>
        /// <remarks>
        /// Właściciel projektu widzi wszystkie zadania.
        /// Użytkownik przypisany do zadania widzi tylko zadania przypisane do niego.
        /// </remarks>
        /// <response code="200">Lista zadań projektu.</response>
        /// <response code="401">Użytkownik nie jest zalogowany.</response>
        /// <response code="404">Projekt nie istnieje lub użytkownik nie ma do niego dostępu.</response>

        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetProjectTasks(
            int projectId)
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var hasProjectAccess = await _context.Projects
                .AnyAsync(project =>
                    project.Id == projectId &&
                    (
                        project.OwnerId == userId ||
                        project.Tasks.Any(task =>
                            task.AssignedUserId == userId)
                    ));

            if (!hasProjectAccess)
            {
                return NotFound(new
                {
                    message = "Projekt nie istnieje lub nie masz do niego dostępu."
                });
            }

            var tasks = await _context.TaskItems
                .AsNoTracking()
                .Where(task =>
                    task.ProjectId == projectId &&
                    (
                        task.Project.OwnerId == userId ||
                        task.AssignedUserId == userId
                    ))
                .Select(task => new TaskResponseDto
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    IsCompleted = task.IsCompleted,
                    CreatedAt = task.CreatedAt,
                    DueDate = task.DueDate,
                    ProjectId = task.ProjectId,
                    ProjectName = task.Project.Name,
                    AssignedUserId = task.AssignedUserId,
                    AssignedUserEmail = task.AssignedUser != null
                        ? task.AssignedUser.Email
                        : null,
                    IsProjectOwner = task.Project.OwnerId == userId,
                    IsAssignedToCurrentUser =
                        task.AssignedUserId == userId
                })
                .OrderByDescending(task => task.CreatedAt)
                .ToListAsync();

            return Ok(tasks);
        }

        /// <summary>
        /// Tworzy nowe zadanie w projekcie.
        /// </summary>
        /// <param name="taskDto">Dane nowego zadania.</param>
        /// <remarks>
        /// Zadanie może utworzyć wyłącznie właściciel projektu.
        /// </remarks>
        /// <response code="201">Zadanie zostało utworzone.</response>
        /// <response code="400">Dane są nieprawidłowe lub użytkownik przypisany do zadania nie istnieje.</response>
        /// <response code="401">Użytkownik nie jest zalogowany.</response>
        /// <response code="403">Użytkownik nie jest właścicielem projektu.</response>
        /// <response code="404">Projekt nie istnieje.</response>

        [HttpPost]
        public async Task<ActionResult<TaskResponseDto>> CreateTask(
            TaskCreateDto taskDto)
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(project =>
                    project.Id == taskDto.ProjectId);

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

            ApplicationUser? assignedUser = null;

            if (!string.IsNullOrWhiteSpace(taskDto.AssignedUserEmail))
            {
                var normalizedEmail = taskDto.AssignedUserEmail
                    .Trim()
                    .ToLowerInvariant();

                assignedUser = await _userManager
                    .FindByEmailAsync(normalizedEmail);

                if (assignedUser is null)
                {
                    return BadRequest(new
                    {
                        message = "Nie znaleziono użytkownika o podanym adresie e-mail."
                    });
                }
            }

            var task = new TaskItem
            {
                Title = taskDto.Title.Trim(),
                Description = taskDto.Description?.Trim(),
                DueDate = taskDto.DueDate,
                ProjectId = taskDto.ProjectId,
                AssignedUserId = assignedUser?.Id,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            var response = new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                ProjectId = project.Id,
                ProjectName = project.Name,
                AssignedUserId = assignedUser?.Id,
                AssignedUserEmail = assignedUser?.Email,
                IsProjectOwner = true,
                IsAssignedToCurrentUser =
                    assignedUser?.Id == userId
            };

            return CreatedAtAction(
                nameof(GetTask),
                new { id = task.Id },
                response);
        }

        /// <summary>
        /// Aktualizuje istniejące zadanie.
        /// </summary>
        /// <param name="id">Identyfikator zadania.</param>
        /// <param name="taskDto">Nowe dane zadania.</param>
        /// <remarks>
        /// Właściciel projektu może edytować całe zadanie i zmienić przypisanego użytkownika.
        /// Przypisany użytkownik może zmienić treść, termin i status swojego zadania.
        /// </remarks>
        /// <response code="204">Zadanie zostało zaktualizowane.</response>
        /// <response code="400">Dane są nieprawidłowe lub użytkownik nie istnieje.</response>
        /// <response code="401">Użytkownik nie jest zalogowany.</response>
        /// <response code="403">Użytkownik nie ma prawa edytować zadania.</response>
        /// <response code="404">Zadanie nie istnieje.</response>

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTask(
            int id,
            TaskUpdateDto taskDto)
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var task = await _context.TaskItems
                .Include(task => task.Project)
                .FirstOrDefaultAsync(task => task.Id == id);

            if (task is null)
            {
                return NotFound(new
                {
                    message = "Zadanie nie istnieje."
                });
            }

            var isProjectOwner = task.Project.OwnerId == userId;
            var isAssignedUser = task.AssignedUserId == userId;

            if (!isProjectOwner && !isAssignedUser)
            {
                return Forbid();
            }

            task.Title = taskDto.Title.Trim();
            task.Description = taskDto.Description?.Trim();
            task.IsCompleted = taskDto.IsCompleted;
            task.DueDate = taskDto.DueDate;

            if (isProjectOwner)
            {
                if (string.IsNullOrWhiteSpace(taskDto.AssignedUserEmail))
                {
                    task.AssignedUserId = null;
                }
                else
                {
                    var normalizedEmail = taskDto.AssignedUserEmail
                        .Trim()
                        .ToLowerInvariant();

                    var assignedUser = await _userManager
                        .FindByEmailAsync(normalizedEmail);

                    if (assignedUser is null)
                    {
                        return BadRequest(new
                        {
                            message = "Nie znaleziono użytkownika o podanym adresie e-mail."
                        });
                    }

                    task.AssignedUserId = assignedUser.Id;
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Usuwa wskazane zadanie.
        /// </summary>
        /// <param name="id">Identyfikator zadania.</param>
        /// <remarks>
        /// Zadanie może usunąć wyłącznie właściciel projektu.
        /// </remarks>
        /// <response code="204">Zadanie zostało usunięte.</response>
        /// <response code="401">Użytkownik nie jest zalogowany.</response>
        /// <response code="403">Użytkownik nie jest właścicielem projektu.</response>
        /// <response code="404">Zadanie nie istnieje.</response>

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetCurrentUserId();

            if (userId is null)
            {
                return Unauthorized();
            }

            var task = await _context.TaskItems
                .Include(task => task.Project)
                .FirstOrDefaultAsync(task => task.Id == id);

            if (task is null)
            {
                return NotFound(new
                {
                    message = "Zadanie nie istnieje."
                });
            }

            if (task.Project.OwnerId != userId)
            {
                return Forbid();
            }

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}