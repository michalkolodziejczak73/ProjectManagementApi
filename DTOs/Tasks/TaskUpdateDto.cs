using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApi.DTOs.Tasks
{
    public class TaskUpdateDto
    {
        [Required(ErrorMessage = "Tytuł zadania jest wymagany.")]
        [MaxLength(150, ErrorMessage = "Tytuł może mieć maksymalnie 150 znaków.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Opis może mieć maksymalnie 1000 znaków.")]
        public string? Description { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? DueDate { get; set; }

        [EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail użytkownika.")]
        public string? AssignedUserEmail { get; set; }
    }
}