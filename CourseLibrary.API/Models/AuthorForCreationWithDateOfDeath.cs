using System;

namespace CourseLibrary.API.Models
{
    public class AuthorForCreationWithDateOfDeath : AuthorForCreationDto
    {
        public DateTimeOffset? DateOfDeath { get; set; }
    }
}