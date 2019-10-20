using System.Collections.Generic;

namespace CourseLibrary.API.Services
{
    public class PropertyMapping<TSource, TDestination> : IPropertyMapping
    {
        public Dictionary<string, PropertyMappingValue> _mapping;

        public PropertyMapping(Dictionary<string, PropertyMappingValue> mapping)
        {
            _mapping = mapping;
        }
    }
}