﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper, IPropertyMappingService propertyMappingService, IPropertyCheckerService propertyCheckerService)
        {
            _courseLibraryRepository = courseLibraryRepository;
            _mapper = mapper;
            _propertyMappingService = propertyMappingService;
            _propertyCheckerService = propertyCheckerService;
        }

        [HttpGet(Name = nameof(GetAuthors))]
        [HttpHead]
        public IActionResult GetAuthors([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(authorsResourceParameters.Fields))
            {
                return BadRequest();
            }

            PagedList<Author> allAuthors = _courseLibraryRepository.GetAuthors(authorsResourceParameters);
            IEnumerable<LinkDto> links = CreateLinksForAuthors(authorsResourceParameters, allAuthors.HasNext, allAuthors.HasPrevious);
            var shapedAuthors = _mapper
                .Map<IEnumerable<AuthorDto>>(allAuthors)
                .ShapeData(authorsResourceParameters.Fields)
                .Select(shapedAuthor =>
                {
                    var shapedAuthorAsDictionary = shapedAuthor as IDictionary<string, object>;
                    var authorLinks = CreateLinksForAuthor((Guid)shapedAuthorAsDictionary["Id"], null);
                    shapedAuthorAsDictionary.Add("links", authorLinks);
                    return shapedAuthorAsDictionary;
                });

            return Ok(new { value = shapedAuthors, links });
        }

        [HttpGet("{authorId:guid}", Name = nameof(GetAuthor))]
        public IActionResult GetAuthor(Guid authorId, string fields)
        {
            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
            {
                return BadRequest();
            }

            Author author = _courseLibraryRepository.GetAuthor(authorId);

            if (author == null)
            {
                return NotFound();
            }

            IEnumerable<LinkDto> links = CreateLinksForAuthor(authorId, fields);
            var resourceToReturn = _mapper.Map<AuthorDto>(author).ShapeData(fields) as IDictionary<string, object>;
            resourceToReturn.Add("links", links);

            return Ok(resourceToReturn);
        }

        [HttpPost(Name = nameof(CreateAuthor))]
        public ActionResult<AuthorDto> CreateAuthor([FromBody] AuthorForCreationDto authorForCreationDto)
        {
            Author authorToBeCreated = _mapper.Map<Author>(authorForCreationDto);
            _courseLibraryRepository.AddAuthor(authorToBeCreated);
            _courseLibraryRepository.Save();

            AuthorDto authorToReturn = _mapper.Map<AuthorDto>(authorToBeCreated);
            IEnumerable<LinkDto> links = CreateLinksForAuthor(authorToReturn.Id, null);
            var resourceToReturn = authorToReturn.ShapeData(null) as IDictionary<string, object>;
            resourceToReturn.Add("links", links);

            return CreatedAtRoute(nameof(GetAuthor), new { authorId = authorToReturn.Id }, resourceToReturn);
        }

        [HttpDelete("{authorId:guid}", Name = nameof(DeleteAuthor))]
        public IActionResult DeleteAuthor(Guid authorId)
        {
            Author authorToDelete = _courseLibraryRepository.GetAuthor(authorId);

            if (authorToDelete == null)
            {
                return NotFound();
            }

            _courseLibraryRepository.DeleteAuthor(authorToDelete);
            _courseLibraryRepository.Save();
            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetAuthorOptions()
        {
            Response.Headers.Add("Allow", "GET,POST,OPTIONS");
            return Ok();
        }

        private string CreateAuthorsResourceUri(
            AuthorsResourceParameters authorsResourceParameters,
            ResourceUriType uri)
        {
            switch (uri)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link(nameof(GetAuthors), new
                    {
                        pageNumber = authorsResourceParameters.PageNumber - 1,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery,
                        orderBy = authorsResourceParameters.OrderBy,
                        fields = authorsResourceParameters.Fields
                    });
                case ResourceUriType.NextPage:
                    return Url.Link(nameof(GetAuthors), new
                    {
                        pageNumber = authorsResourceParameters.PageNumber + 1,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery,
                        orderBy = authorsResourceParameters.OrderBy,
                        fields = authorsResourceParameters.Fields
                    });
                case ResourceUriType.Current:
                default:
                    return Url.Link(nameof(GetAuthors), new
                    {
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery,
                        orderBy = authorsResourceParameters.OrderBy,
                        fields = authorsResourceParameters.Fields
                    });
            }
        }

        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid authorId, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(new LinkDto(Url.Link(nameof(GetAuthor), new { authorId }), "self", "GET"));
            }
            else
            {
                links.Add(new LinkDto(Url.Link(nameof(GetAuthor), new { authorId, fields }), "self", "GET"));
            }

            links.Add(new LinkDto(
                Url.Link(nameof(DeleteAuthor),
                    new { authorId }),
                "delete_author",
                "DELETE"));
            links.Add(new LinkDto(
                Url.Link(nameof(CoursesController.CreateCourseForAuthor), new { authorId }),
                "create_course_for_author",
                "POST"));
            links.Add(new LinkDto(
                Url.Link(nameof(CoursesController.GetCoursesForAuthor), new { authorId }),
                "courses",
                "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors(
            AuthorsResourceParameters authorsResourceParameters,
            bool hasNext,
            bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(new LinkDto(
                CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.Current),
                "self",
                "GET"));

            if (hasNext)
            {
                links.Add(new LinkDto(
                    CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage),
                    "nextPage",
                    "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(
                    CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage),
                    "previousPage",
                    "GET"));
            }

            return links;
        }
    }
}
