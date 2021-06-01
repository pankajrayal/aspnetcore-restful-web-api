using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoviesAPI.DTOs;
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
        private readonly ILogger<GenresController> logger;
        private readonly MoviesDbContext _context;
        private readonly IMapper _mapper;

        public GenresController(ILogger<GenresController> logger, MoviesDbContext context,
            IMapper mapper)
        {
            this.logger = logger;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet] // api/genres
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<GenreDTO>>> Get()
        {
            var genres = await _context.Genres.AsNoTracking().ToListAsync();
            //var genresDTOs = new List<GenreDTO>();
            //foreach (var genre in genres) {
            //    genresDTOs.Add(new GenreDTO() { 
            //        Id= genre.Id,
            //        Name = genre.Name
            //    });
            //}
            var genresDTOs = _mapper.Map<List<GenreDTO>>(genres);
            return genresDTOs;
        }

        [HttpGet("{Id:int}", Name = "getGenre")] // api/genres/example
        public async Task<ActionResult<GenreDTO>> Get(int Id) {
            var genre = await _context.Genres.FirstOrDefaultAsync(x => x.Id == Id);

            if (genre == null) {
                return NotFound();
            }

            var genreDTO = _mapper.Map<GenreDTO>(genre);

            return genreDTO;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] GenreCreationDTO genreCreation)
        {
            var genre = _mapper.Map<Genre>(genreCreation);
            _context.Add(genre);
            await _context.SaveChangesAsync();
            var genreDto = _mapper.Map<GenreDTO>(genre);

            return new CreatedAtRouteResult("getGenre", new { genreDto.Id }, genreDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] GenreCreationDTO genreCreation) {
            var genre = _mapper.Map<Genre>(genreCreation);
            genre.Id = id;
            _context.Entry(genre).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var exists = await _context.Genres.AnyAsync(x => x.Id == id);
            if (!exists) {
                return NotFound();
            }

            _context.Remove(new Genre() { Id = id });
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
