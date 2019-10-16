using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.ValidationAttributes;

namespace CourseLibrary.API.Models
{
    [CourseTitleMustBeDifferentFromDescription(ErrorMessage = "Title must be different from description.")]
    public class CourseForCreationDto
    {
        [Required(ErrorMessage = "You should fill out a title.")]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}