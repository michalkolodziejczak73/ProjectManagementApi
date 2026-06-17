using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApi.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        public ApplicationUser Owner { get; set; } = null!;

        public ICollection<TaskItem> Tasks { get; set; }
            = new List<TaskItem>();
    }
}