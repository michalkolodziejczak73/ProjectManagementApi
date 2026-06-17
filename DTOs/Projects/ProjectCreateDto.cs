using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApi.DTOs.Projects
{
    public class ProjectCreateDto
    {
        [Required(ErrorMessage = "Nazwa projektu jest wymagana.")]
        [MaxLength(150, ErrorMessage = "Nazwa może mieć maksymalnie 150 znaków.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Opis może mieć maksymalnie 1000 znaków.")]
        public string? Description { get; set; }
    }
}