using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;
using MoviesAPI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Logging;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/movies")]
    public class MoviesController : ControllerBase
    {
        private readonly MoviesDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<MoviesController> _logger;
        private readonly string containerName = "movies";

        public MoviesController(MoviesDbContext context, IMapper mapper, IFileStorageService fileStorageService, ILogger<MoviesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IndexMoviePageDTO>> Get()
        {
            var top = 6;
            var today = DateTime.Today;
            var upcomingReleases = await _context.Movies
                .Where(x => x.ReleaseDate > today)
                .OrderBy(x => x.ReleaseDate)
                .Take(top)
                .ToListAsync();
            var inTheathers = await _context.Movies
                .Where(x => x.InTheatre)
                .Take(top)
                .ToListAsync();

            var result = new IndexMoviePageDTO();
            result.InTheaters = _mapper.Map<List<MovieDTO>>(inTheathers);
            result.UpcomingReleases = _mapper.Map<List<MovieDTO>>(upcomingReleases);

            return result;
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<MovieDTO>>> Filter([FromQuery] FilterMoviesDTO filterMoviesDTO)
        {
            var moviesQueryable = _context.Movies.AsQueryable();
            if(!string.IsNullOrWhiteSpace(filterMoviesDTO.Title))
            {
                moviesQueryable = moviesQueryable.Where(x => x.Title.Contains(filterMoviesDTO.Title));
            }
            if(filterMoviesDTO.InTheaters)
            {
                moviesQueryable = moviesQueryable.Where(x => x.InTheatre);
            }

            if(filterMoviesDTO.UpcomingReleses)
            {
                var today = DateTime.Today;
                moviesQueryable = moviesQueryable.Where(x => x.ReleaseDate > today);
            }

            if(filterMoviesDTO.GenreId != 0)
            {
                moviesQueryable = moviesQueryable
                    .Where(x => x.MoviesGenres.Select(y => y.GenreId).Contains(filterMoviesDTO.GenreId));
            }

            //if(!string.IsNullOrWhiteSpace(filterMoviesDTO.OrderingField))
            //{
            //    if(filterMoviesDTO.OrderingField == "title")
            //    {
            //        if (filterMoviesDTO.AscendingOrder)
            //        {
            //            moviesQueryable = moviesQueryable.OrderBy(x => x.Title);
            //        }
            //        else 
            //        {
            //            moviesQueryable = moviesQueryable.OrderByDescending(x => x.Title);
            //        }
            //    }
            //}

            if(!string.IsNullOrWhiteSpace(filterMoviesDTO.OrderingField))
            {
                try
                {
                    moviesQueryable = moviesQueryable
                        .OrderBy($"{filterMoviesDTO.OrderingField} {(filterMoviesDTO.AscendingOrder ? "ascending" : "descending")}");

                }
                catch (Exception)
                {
                    // error
                    _logger.LogWarning("Couldn't order by firle: " + filterMoviesDTO.OrderingField);
                }

            }


            await HttpContext.InsertPaginationParametersInResponse(moviesQueryable, 
                filterMoviesDTO.RecordsPerPage);

            var movies = await moviesQueryable.Paginate(filterMoviesDTO.Pagination).ToListAsync();
            return _mapper.Map<List<MovieDTO>>(movies);
        }

        [HttpGet("{id}", Name = "getMovie")]
        public async Task<ActionResult<MovieDetailsDTO>> Get(int id)
        {
            var movie = await _context.Movies
                .Include(x => x.MoviesActors).ThenInclude(x => x.Person)
                .Include(x=>x.MoviesGenres).ThenInclude(x=>x.Genre)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            return _mapper.Map<MovieDetailsDTO>(movie);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movie = _mapper.Map<Movie>(movieCreationDTO);
            if (movieCreationDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await movieCreationDTO.Poster.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(movieCreationDTO.Poster.FileName);
                    movie.Poster = await _fileStorageService.SaveFile(content, extension, containerName,
                        movieCreationDTO.Poster.ContentType);
                }
            }
            AnnotateActorsOrder(movie);
            _context.Add(movie);
            await _context.SaveChangesAsync();
            var movieDTO = _mapper.Map<MovieDTO>(movie);
            return new CreatedAtRouteResult("getMovie", new { id = movie.Id }, movieDTO);
        }

        private static void AnnotateActorsOrder(Movie movie)
        {
            if(movie.MoviesActors !=null)
            {
                for(int i=0;i<movie.MoviesActors.Count; i++)
                {
                    movie.MoviesActors[i].Order = i;
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movieDB = await _context.Movies.FirstOrDefaultAsync(x => x.Id == id);
            if (movieDB == null)
            {
                return NotFound();
            }

            movieDB = _mapper.Map(movieCreationDTO, movieDB);

            if (movieCreationDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await movieCreationDTO.Poster.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(movieCreationDTO.Poster.FileName);
                    movieDB.Poster = await _fileStorageService.EditFile(content, extension, containerName,
                        movieDB.Poster, movieCreationDTO.Poster.ContentType);
                }
            }

             _context.Database.ExecuteSqlRaw($"DELETE FROM MoviesActors WHERE MovieId = ${movieDB.Id}; DELETE FROM MoviesGenres WHERE MovieId = ${movieDB.Id};");
            AnnotateActorsOrder(movieDB);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var exists = await _context.Movies.AnyAsync(x => x.Id == id);
            if (!exists)
            {
                return NotFound();
            }

            _context.Remove(new Movie() { Id = id });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<MoviePatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var entityFromDB = await _context.Movies.FirstOrDefaultAsync(x => x.Id == id);
            if(entityFromDB==null)
            {
                return NotFound();
            }

            var entityDTO = _mapper.Map<MoviePatchDTO>(entityFromDB);
            patchDocument.ApplyTo(entityDTO, ModelState);
            var isValid = TryValidateModel(entityDTO);

            if(!isValid)
            {
                return BadRequest(ModelState);
            }
            _mapper.Map(entityDTO, entityFromDB);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
