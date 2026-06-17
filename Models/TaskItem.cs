using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApi.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public Project Project { get; set; } = null!;

        public string? AssignedUserId { get; set; }

        public ApplicationUser? AssignedUser { get; set; }
    }
}