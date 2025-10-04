using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogoController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string nombre, int? creditosMin, int? creditosMax, TimeSpan? horaInicio, TimeSpan? horaFin)
        {
            var query = _context.Cursos
                                .Where(c => c.Activo) 
                                .AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(c => c.Nombre.Contains(nombre));

            if (creditosMin.HasValue)
                query = query.Where(c => c.Creditos >= creditosMin);

            if (creditosMax.HasValue)
                query = query.Where(c => c.Creditos <= creditosMax);

            var cursos = await query.ToListAsync();

            if (horaInicio.HasValue)
                cursos = cursos.Where(c => c.HorarioInicio >= horaInicio).ToList();

            if (horaFin.HasValue)
                cursos = cursos.Where(c => c.HorarioFin <= horaFin).ToList();

            return View(cursos);
        }


        public async Task<IActionResult> Detalle(int id)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id && c.Activo);
            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }
    }
}
