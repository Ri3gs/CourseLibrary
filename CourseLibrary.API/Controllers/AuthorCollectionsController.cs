using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public AuthorCollectionsController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository;
            _mapper = mapper;
        }

        [HttpGet("({ids})", Name = nameof(GetAuthorCollection))]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthorCollection(
            [FromRoute]
            [ModelBinder(BinderType = typeof(ArrayModelBinder))]
            IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            IEnumerable<Author> authors = _courseLibraryRepository.GetAuthors(ids);

            if (ids.Count() != authors.Count())
            {
                return NotFound();
            }

            return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authors));
        }

        [HttpPost]
        public ActionResult<IEnumerable<AuthorDto>> CreateAuthorCollection([FromBody] IEnumerable<AuthorForCreationDto> authorsForCreationDtos)
        {
            IEnumerable<Author> authorsToBeCreated = _mapper.Map<IEnumerable<Author>>(authorsForCreationDtos);
            foreach (Author author in authorsToBeCreated)
            {
                _courseLibraryRepository.AddAuthor(author);
            }

            _courseLibraryRepository.Save();

            IEnumerable<AuthorDto> authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorsToBeCreated);
            string commaSeparatedIds = string.Join(",", authorsToReturn.Select(author => author.Id));
            return CreatedAtRoute(nameof(GetAuthorCollection), new { ids = commaSeparatedIds }, authorsToReturn);
        }
    }
}