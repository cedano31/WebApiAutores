using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;

namespace WebApiAutores.Controllers.V2
{
    [ApiController]
    [Route("api/v2/autores")]
    //[Route("api/v2/autores")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAuthorizationService authorizationService;

        public AutoresController(ApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService)
        {
            this.context = context;
            this.mapper = mapper;
            this.authorizationService = authorizationService;
        }

        [HttpGet(Name = "obtenerAutores/v2")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<List<AutorDTO>>> Get([FromHeader] string incluirHATEOAS)
        {
            var autores = await context.Autores.ToListAsync();
            return mapper.Map<List<AutorDTO>>(autores);

        }

        //            if (incluirHATEOAS)
        //    { 
        //        var esAdmin = await authorizationService.AuthorizeAsync(User, "esAdmin");
        ////dtos.ForEach(dto => GenerarEnlaces(dto, esAdmin.Succeeded));
        //var resultado = new ColeccionDeRecursos<AutorDTO> { Valores = dtos };
        //resultado.Enlaces.Add(new DatoHATEOAS(
        //    enlace: Url.Link("obtenerAutores", new { }),
        //            descripcion: "self",
        //            metodo: "GET"));

        //        if (esAdmin.Succeeded)
        //        {
        //            resultado.Enlaces.Add(new DatoHATEOAS(
        //            enlace: Url.Link("crearAutor", new { }),
        //            descripcion: "crear-autor",
        //            metodo: "POST"));
        //        }

        //        return Ok(resultado); 
        //    }

        [HttpGet("{nombre}", Name = "obtenerAutorPorNombre/v2")]
        [AllowAnonymous]
        public async Task<ActionResult<List<AutorDTO>>> GetPorNombre(string nombre)
        {
            var autores = await context.Autores.Where(x => x.Name.Contains(nombre)).ToListAsync();

            if (autores == null)
            {
                return NotFound();
            }

            return mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpGet("{id:int}", Name = "obtenerAutor/v2")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<AutorDTOConLibros>> Get(int id, [FromHeader] string incluirHATEOAS)
        {
            var autor = await context.Autores
                .Include(autor => autor.AutoresLibros)
                .ThenInclude(x => x.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (autor == null)
            {
                return NotFound();
            }

            var dto = mapper.Map<AutorDTOConLibros>(autor);
            return dto;

        }

        [HttpPost(Name = "crearAutor/v2")]
        public async Task<ActionResult> Post(AutorCreacionDTO autorCreacionDTO)
        {

            var valNameExist = await context.Autores.AnyAsync(x => x.Name == autorCreacionDTO.Name);

            if (valNameExist)
            {
                return BadRequest($"El usuario {autorCreacionDTO.Name} ya existe");
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            context.Add(autor);
            await context.SaveChangesAsync();

            var autorDTO = mapper.Map<AutorDTO>(autor);

            return CreatedAtRoute("obtenerAutor/v2", new { id = autor.Id }, autor);
        }

        [HttpPut("{id:int}", Name = "actualizarAutor/v2")]
        public async Task<ActionResult> Put(AutorCreacionDTO autorCreacionDTO, int id)
        {
            var exist = await context.Autores.AnyAsync(x => x.Id == id);

            if (!exist)
            {

                return NotFound();

            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;
            context.Update(autor);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "borrarAutor/v2")]
        public async Task<ActionResult> Delete(int id)
        {
            var exist = await context.Autores.AnyAsync(x => x.Id == id);
            if (!exist)
            {

                return NotFound();
            }

            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();

        }



    }
}
