using System;
using System.Collections.Generic;
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

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper, IPropertyMappingService propertyMappingService)
        {
            _courseLibraryRepository = courseLibraryRepository;
            _mapper = mapper;
            _propertyMappingService = propertyMappingService;
        }

        [HttpGet(Name = nameof(GetAuthors))]
        [HttpHead]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            PagedList<Author> allAuthors = _courseLibraryRepository.GetAuthors(authorsResourceParameters);

            string previousPageLink = allAuthors.HasPrevious
                ? CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage)
                : null;
            string nextPageLink = allAuthors.HasNext
                ? CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage)
                : null;

            var paginationMetadata = new
            {
                totalCount = allAuthors.TotalCount,
                pageSize = allAuthors.PageSize,
                currentPage = allAuthors.CurrentPage,
                totalPages = allAuthors.TotalPages,
                previousPageLink,
                nextPageLink
            };
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
            return Ok(_mapper.Map<IEnumerable<AuthorDto>>(allAuthors));
        }

        [HttpGet("{authorId:guid}", Name = nameof(GetAuthor))]
        public IActionResult GetAuthor(Guid authorId)
        {
            Author author = _courseLibraryRepository.GetAuthor(authorId);

            if (author == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<AuthorDto>(author));
        }

        [HttpPost]
        public ActionResult<AuthorDto> CreateAuthor([FromBody] AuthorForCreationDto authorForCreationDto)
        {
            Author authorToBeCreated = _mapper.Map<Author>(authorForCreationDto);
            _courseLibraryRepository.AddAuthor(authorToBeCreated);
            _courseLibraryRepository.Save();

            AuthorDto authorToReturn = _mapper.Map<AuthorDto>(authorToBeCreated);
            return CreatedAtRoute(nameof(GetAuthor), new { authorId = authorToReturn.Id }, authorToReturn);
        }

        [HttpDelete("{authorId:guid}")]
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
                        orderBy = authorsResourceParameters.OrderBy
                    });
                case ResourceUriType.NextPage:
                    return Url.Link(nameof(GetAuthors), new
                    {
                        pageNumber = authorsResourceParameters.PageNumber + 1,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery,
                        orderBy = authorsResourceParameters.OrderBy
                    });
                default:
                    return Url.Link(nameof(GetAuthors), new
                    {
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery,
                        orderBy = authorsResourceParameters.OrderBy
                    });
            }
        }
    }
}
