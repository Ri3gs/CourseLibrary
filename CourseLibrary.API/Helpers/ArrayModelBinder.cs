using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CourseLibrary.API.Helpers
{
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            string possibleValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();

            if (string.IsNullOrWhiteSpace(possibleValue))
            {
                 bindingContext.Result = ModelBindingResult.Success(null);
                 return Task.CompletedTask;
            }

            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            var converter = TypeDescriptor.GetConverter(elementType);

            var convertedValues = possibleValue.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
                .Select(singleValue => converter.ConvertFromString(singleValue))
                .ToArray();

            var typedValues = Array.CreateInstance(elementType, convertedValues.Length);
            convertedValues.CopyTo(typedValues, 0);
            bindingContext.Model = typedValues;
            bindingContext.Result = ModelBindingResult.Success(typedValues);
            return Task.CompletedTask;
        }
    }
}