using System;
using System.Collections.Generic;
using AutoMapper;
using CourseLibrary.API.Entities;
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

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [HttpHead]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {
            var allAuthors = _courseLibraryRepository.GetAuthors(authorsResourceParameters);
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
    }
}
