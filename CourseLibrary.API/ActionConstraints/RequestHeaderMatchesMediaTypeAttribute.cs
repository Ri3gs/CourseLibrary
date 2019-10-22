using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace CourseLibrary.API.ActionConstraints
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
    {
        private readonly string _requestHeaderToMatch;
        private readonly MediaTypeCollection _mediaTypes = new MediaTypeCollection();

        public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch, string mediaType, params string[] otherMediaTypes)
        {
            _requestHeaderToMatch = requestHeaderToMatch;

            if (MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedValue))
            {
                _mediaTypes.Add(parsedValue);
            }
            else
            {
                throw new ArgumentException();
            }

            foreach (var otherMediaType in otherMediaTypes)
            {
                if (MediaTypeHeaderValue.TryParse(otherMediaType, out MediaTypeHeaderValue parsed))
                {
                    _mediaTypes.Add(parsed);
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }

        public bool Accept(ActionConstraintContext context)
        {
            var requestHeaders = context.RouteContext.HttpContext.Request.Headers;
            if (!requestHeaders.ContainsKey(_requestHeaderToMatch))
            {
                return false;
            }

            var parsedRequestMediaType = new MediaType(requestHeaders[_requestHeaderToMatch]);

            foreach (var mediaType in _mediaTypes)
            {
                var parsedMediaType = new MediaType(mediaType);

                if (parsedRequestMediaType.Equals(parsedMediaType))
                {
                    return true;
                }
            }

            return false;
        }

        public int Order => 0;
    }
}