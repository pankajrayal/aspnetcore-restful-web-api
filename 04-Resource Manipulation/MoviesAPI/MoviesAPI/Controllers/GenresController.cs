using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoviesAPI.Entities;
using MoviesAPI.Filters;
using MoviesAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoviesAPI.Controllers
{
    [Route("api/genres")]
    [ApiController]
    public class GenresController: ControllerBase
    {
        private readonly IRepository repository;
        private readonly ILogger<GenresController> logger;

        public GenresController(IRepository repository, ILogger<GenresController> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        [HttpGet] // api/genres
        public async Task<ActionResult<List<Genre>>> Get()
        {
            return await repository.GetAllGenres();
        }

        [HttpGet("{Id:int}", Name = "getGenre")] // api/genres/example
        [ServiceFilter(typeof(MyActionFilter))]
        public ActionResult<Genre> Get(int Id, string param2)
        {
            var genre = repository.GetGenreById(Id);

            if (genre == null)
            {
                return NotFound();
            }

            return genre;
        }

        [HttpPost]
        public ActionResult Post([FromBody] Genre genre)
        {
            repository.AddGenre(genre);

            return new CreatedAtRouteResult("getGenre", new { Id = genre.Id }, genre);
        }

        [HttpPut]
        public ActionResult Put([FromBody] Genre genre)
        {

            return NoContent();
        }

        [HttpDelete]
        public ActionResult Delete()
        {
            return NoContent();

        }
    }
}
