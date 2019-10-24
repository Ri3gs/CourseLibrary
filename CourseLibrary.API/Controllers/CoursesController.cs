using System;
using System.Collections.Generic;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId:guid}/courses")]
    //[ResponseCache(CacheProfileName = "240SecondsCacheProfile")]
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
    [HttpCacheValidation(MustRevalidate = true)]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository;
            _mapper = mapper;
        }

        [HttpGet(Name = nameof(GetCoursesForAuthor))]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var coursesForAuthorFromRepo = _courseLibraryRepository.GetCourses(authorId);
            return Ok(_mapper.Map<IEnumerable<CourseDto>>(coursesForAuthorFromRepo));
        }

        [HttpGet("{courseId:guid}", Name = nameof(GetCourseForAuthor))]
        //[ResponseCache(Duration = 120)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 1000)]
        [HttpCacheValidation(MustRevalidate = false)]
        public ActionResult<CourseDto> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            Course course = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (course == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CourseDto>(course));
        }

        [HttpPost(Name = nameof(CreateCourseForAuthor))]
        public ActionResult<CourseDto> CreateCourseForAuthor(Guid authorId, [FromBody]CourseForCreationDto courseForCreationDto)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            Course courseToBeCreated = _mapper.Map<Course>(courseForCreationDto);
            _courseLibraryRepository.AddCourse(authorId, courseToBeCreated);
            _courseLibraryRepository.Save();

            CourseDto courseToReturn = _mapper.Map<CourseDto>(courseToBeCreated);
            return CreatedAtRoute(
                nameof(GetCourseForAuthor),
                new { authorId = courseToReturn.AuthorId, courseId = courseToReturn.Id },
                courseToReturn);
        }

        [HttpPut("{courseId:guid}")]
        public ActionResult<CourseDto> UpdateCourseForAuthor(Guid authorId, Guid courseId, [FromBody] CourseForUpdateDto courseForUpdateDto)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            Course courseToBeUpdated = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (courseToBeUpdated == null)
            {
                Course courseToAdd = _mapper.Map<Course>(courseForUpdateDto);
                courseToAdd.Id = courseId;
                _courseLibraryRepository.AddCourse(authorId, courseToAdd);
                _courseLibraryRepository.Save();

                return CreatedAtRoute(
                    nameof(GetCourseForAuthor),
                    new { authorId = courseToAdd.AuthorId, courseId = courseToAdd.Id },
                    _mapper.Map<CourseDto>(courseToAdd));
            }

            _mapper.Map(courseForUpdateDto, courseToBeUpdated);
            _courseLibraryRepository.UpdateCourse(courseToBeUpdated);
            _courseLibraryRepository.Save();

            return Ok(_mapper.Map<CourseDto>(courseToBeUpdated));
        }

        [HttpPatch("{courseId:guid}")]
        public ActionResult<CourseDto> PartiallyUpdateCourseForAuthor(
            Guid authorId,
            Guid courseId,
            JsonPatchDocument<CourseForUpdateDto> patchDocument)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            Course courseToBePatched = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (courseToBePatched == null)
            {
                var courseForUpdateDto = new CourseForUpdateDto();
                patchDocument.ApplyTo(courseForUpdateDto, ModelState);

                if (!TryValidateModel(courseForUpdateDto))
                {
                    return ValidationProblem(ModelState);
                }

                Course courseToAdd = _mapper.Map<Course>(courseForUpdateDto);
                courseToAdd.Id = courseId;
                _courseLibraryRepository.AddCourse(authorId, courseToAdd);
                _courseLibraryRepository.Save();

                return CreatedAtRoute(
                    nameof(GetCourseForAuthor),
                    new { authorId = courseToAdd.AuthorId, courseId = courseToAdd.Id },
                    _mapper.Map<CourseDto>(courseToAdd));
            }

            CourseForUpdateDto courseToPatch = _mapper.Map<CourseForUpdateDto>(courseToBePatched);
            patchDocument.ApplyTo(courseToPatch, ModelState);

            if (!TryValidateModel(courseToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(courseToPatch, courseToBePatched);
            _courseLibraryRepository.UpdateCourse(courseToBePatched);
            _courseLibraryRepository.Save();

            return Ok(_mapper.Map<CourseDto>(courseToBePatched));
        }

        [HttpDelete("{courseId:guid}")]
        public IActionResult DeleteCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            Course courseToBeDeleted = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (courseToBeDeleted == null)
            {
                return NotFound();
            }

            _courseLibraryRepository.DeleteCourse(courseToBeDeleted);
            _courseLibraryRepository.Save();
            return NoContent();
        }

        public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}