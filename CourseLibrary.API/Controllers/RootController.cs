using System.Collections.Generic;
using CourseLibrary.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet(Name = nameof(GetRoot))]
        public IActionResult GetRoot()
        {
            var links = new List<LinkDto>();

            links.Add(new LinkDto(
                Url.Link(nameof(GetRoot), new { }),
                "self",
                "GET"));

            links.Add(new LinkDto(
                Url.Link(nameof(AuthorsController.GetAuthors), new { }),
                "authors",
                "GET"));

            links.Add(new LinkDto(
                Url.Link(nameof(AuthorsController.CreateAuthor), new { }),
                "create_author",
                "POST"));

            return Ok(links);
        }
    }
}