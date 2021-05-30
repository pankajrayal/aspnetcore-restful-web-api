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

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/people")]
    public class PeopleController : ControllerBase
    {
        private readonly MoviesDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly string containerName = "people";

        public PeopleController(MoviesDbContext context, IMapper mapper,
            IFileStorageService fileStorageService)
        {
            _context = context;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PersonDTO>>> Get([FromQuery] PaginationDTO pagination) 
        {
            var queryable = _context.People.AsQueryable();
            await HttpContext.InsertPaginationParemetersInResponse(queryable, pagination.RecordsPerPage);

            var people = await queryable.Paginate(pagination).ToListAsync();
            return _mapper.Map<List<PersonDTO>>(people);
        }

        [HttpGet("{id}", Name = "getPerson")]
        public async Task<ActionResult<PersonDTO>> Get(int id)
        {
            var person = await _context.People.FirstOrDefaultAsync(x => x.Id == id);
            if(person == null)
            {
                return NotFound();
            }

            return _mapper.Map<PersonDTO>(person);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] PersonCreationDTO personCreationDTO)
        {
            var person = _mapper.Map<Person>(personCreationDTO);

            if (personCreationDTO.Picture != null) 
            {
                using (var memoryStream = new MemoryStream())
                {
                    await personCreationDTO.Picture.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(personCreationDTO.Picture.FileName);
                    person.Picture = await _fileStorageService.SaveFile(content, extension, containerName, personCreationDTO.Picture.ContentType);
                }
            }

            _context.Add(person);
            await _context.SaveChangesAsync();
            var personDTO = _mapper.Map<PersonDTO>(person);
            return new CreatedAtRouteResult("getPerson", new { id = person.Id }, personDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromForm] PersonCreationDTO personCreationDTO)
        {
            var personDB = await _context.People.FirstOrDefaultAsync(x => x.Id == id);
            
            if(personDB == null) { return NotFound(); }

            personDB = _mapper.Map<Person>(personCreationDTO);
            if(personCreationDTO.Picture != null)
            {
                using(var memoryStream = new MemoryStream())
                {
                    await personCreationDTO.Picture.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(personCreationDTO.Picture.FileName);
                    personDB.Picture = await _fileStorageService.EditFile(content, extension,
                        containerName, personDB.Picture, personCreationDTO.Picture.ContentType);
                }
            }
            await _context.SaveChangesAsync();
            return NoContent();

        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<PersonPatchDTO> patchDocument)
        {
            if(patchDocument == null)
            {
                return BadRequest();
            }

            var entityFromDB = await _context.People.FirstOrDefaultAsync(x => x.Id == id);

            if(entityFromDB == null)
            {
                return NotFound();
            }

            var entityDTO = _mapper.Map<PersonPatchDTO>(entityFromDB);
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var exists = await _context.People.AnyAsync(x => x.Id == id);
            if(!exists)
            {
                return NotFound();
            }
            _context.Remove(new Person() { Id = id });
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
