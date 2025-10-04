using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PortalAcademico.Data;
using PortalAcademico.Models;
using System.Text.Json;

namespace PortalAcademico.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CatalogoController(ApplicationDbContext context, IDistributedCache cache, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index(string nombre, int? creditosMin, int? creditosMax, TimeSpan? horaInicio, TimeSpan? horaFin)
        {
            const string cacheKey = "CursosActivos";

            string? cachedCursos = await _cache.GetStringAsync(cacheKey);
            List<Curso> cursos;

            if (!string.IsNullOrEmpty(cachedCursos))
            {
                cursos = JsonSerializer.Deserialize<List<Curso>>(cachedCursos)!;
            }
            else
            {
                cursos = await _context.Cursos.Where(c => c.Activo).ToListAsync();

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(cursos), options);
            }

            if (!string.IsNullOrEmpty(nombre))
                cursos = cursos.Where(c => c.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase)).ToList();

            if (creditosMin.HasValue)
                cursos = cursos.Where(c => c.Creditos >= creditosMin.Value).ToList();

            if (creditosMax.HasValue)
                cursos = cursos.Where(c => c.Creditos <= creditosMax.Value).ToList();

            if (horaInicio.HasValue)
                cursos = cursos.Where(c => c.HorarioInicio >= horaInicio.Value).ToList();

            if (horaFin.HasValue)
                cursos = cursos.Where(c => c.HorarioFin <= horaFin.Value).ToList();

            return View(cursos);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id && c.Activo);
            if (curso == null)
                return NotFound();

            _httpContextAccessor.HttpContext!.Session.SetInt32("UltimoCursoId", curso.Id);
            _httpContextAccessor.HttpContext.Session.SetString("UltimoCursoNombre", curso.Nombre);

            return View(curso);
        }
    }
}
