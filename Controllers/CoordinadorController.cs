using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoordinadorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lista todos los cursos
        public async Task<IActionResult> Cursos()
        {
            var cursos = await _context.Cursos.ToListAsync();
            return View(cursos);
        }


        public IActionResult CreateCurso()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCurso([Bind("Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin")] Curso curso)
        {
            if (ModelState.IsValid)
            {
                curso.Activo = true; 
                _context.Add(curso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Cursos));
            }
            return View(curso);
        }

  
        public async Task<IActionResult> EditCurso(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();

            return View(curso);
        }

 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCurso(int id, [Bind("Id,Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
        {
            if (id != curso.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Cursos.Any(e => e.Id == curso.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Cursos));
            }
            return View(curso);
        }

  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();

            curso.Activo = !curso.Activo;
            _context.Update(curso);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Cursos));
        }

        public async Task<IActionResult> MatriculasCurso(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null) return NotFound();

            return View(curso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarMatricula(int matriculaId)
        {
            var matricula = await _context.Matriculas.FindAsync(matriculaId);
            if (matricula == null) return NotFound();

            matricula.Estado = EstadoMatricula.Confirmada; 
            _context.Update(matricula);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MatriculasCurso), new { id = matricula.CursoId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarMatricula(int matriculaId)
        {
            var matricula = await _context.Matriculas.FindAsync(matriculaId);
            if (matricula == null) return NotFound();

            matricula.Estado = EstadoMatricula.Cancelada; // Ajusta según tu enum
            _context.Update(matricula);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MatriculasCurso), new { id = matricula.CursoId });
        }
    }
}
