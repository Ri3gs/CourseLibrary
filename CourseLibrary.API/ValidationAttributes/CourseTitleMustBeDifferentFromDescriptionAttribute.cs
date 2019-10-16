using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.ValidationAttributes
{
    public class CourseTitleMustBeDifferentFromDescriptionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var course = (dynamic)validationContext.ObjectInstance;

            if (course.Title == course.Description)
            {
                return new ValidationResult(ErrorMessage, new List<string> { nameof(CourseForCreationDto) });
            }

            return ValidationResult.Success;
        }
    }
}