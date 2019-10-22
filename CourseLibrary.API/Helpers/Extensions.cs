using System;

namespace CourseLibrary.API.Helpers
{
    public static class Extensions
    {
        public static int GetCurrentAge(this DateTimeOffset dateTimeOffset, DateTimeOffset? dateOfDeath)
        {
            var dateToCalculateTo = DateTime.UtcNow;

            if (dateOfDeath.HasValue)
            {
                dateToCalculateTo = dateOfDeath.Value.UtcDateTime;
            }

            int age = dateToCalculateTo.Year - dateTimeOffset.Year;

            if (dateToCalculateTo < dateTimeOffset.AddYears(age))
            {
                age--;
            }

            return age;
        }
    }
}