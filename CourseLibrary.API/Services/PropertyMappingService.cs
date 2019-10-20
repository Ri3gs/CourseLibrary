using System;
using System.Collections.Generic;
using System.Linq;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly Dictionary<string, PropertyMappingValue> _authorPropertyMapping = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
        {
            { nameof(AuthorDto.Id), new PropertyMappingValue(new List<string> { nameof(Author.Id) })},
            { nameof(AuthorDto.MainCategory), new PropertyMappingValue(new List<string> { nameof(Author.MainCategory) })},
            { nameof(AuthorDto.Age), new PropertyMappingValue(new List<string> { nameof(Author.DateOfBirth) }, true)},
            { nameof(AuthorDto.Name), new PropertyMappingValue(new List<string> { nameof(Author.FirstName), nameof(Author.LastName) })}
        };

        private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMapping));
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                var trimmedField = field.Trim();

                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1
                    ? trimmedField
                    : trimmedField.Remove(indexOfFirstSpace);

                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }

            return true;
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            var matchingMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>().ToList();

            if (matchingMapping.Count == 1)
            {
                return matchingMapping.First()._mapping;
            }

            throw new InvalidOperationException();
        }
    }
}