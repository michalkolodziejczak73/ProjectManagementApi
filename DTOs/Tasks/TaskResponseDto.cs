namespace ProjectManagementApi.DTOs.Tasks
{
    public class TaskResponseDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? DueDate { get; set; }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public string? AssignedUserId { get; set; }

        public string? AssignedUserEmail { get; set; }

        public bool IsProjectOwner { get; set; }

        public bool IsAssignedToCurrentUser { get; set; }
    }
}