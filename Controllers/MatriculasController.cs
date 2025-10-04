using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    [Authorize]
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MatriculasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            // Obtener el Id del usuario logueado
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                // Esto no debería pasar por [Authorize], pero es seguro validarlo
                TempData["Error"] = "Debe iniciar sesión para inscribirse.";
                return RedirectToAction("Index", "Catalogo");
            }

            // Buscar el curso activo
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == cursoId && c.Activo);
            if (curso == null)
            {
                TempData["Error"] = "El curso no existe o no está activo.";
                return RedirectToAction("Index", "Catalogo");
            }

            // Validar cupo máximo
            var inscritos = await _context.Matriculas.CountAsync(m => m.CursoId == cursoId && m.Estado != EstadoMatricula.Cancelada);
            if (inscritos >= curso.CupoMaximo)
            {
                TempData["Error"] = "No se puede inscribir: cupo máximo alcanzado.";
                return RedirectToAction("Detalle", "Catalogo", new { id = cursoId });
            }

            // Validar solapamiento de horario
            var matriculasUsuario = await _context.Matriculas
            .Include(m => m.Curso)
            .Where(m => m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada)
            .ToListAsync(); // esto devuelve Task<List<Matricula>> y puedes usar await

            // luego filtras en memoria con Any
            var solapados = matriculasUsuario.Any(m => m.Curso != null &&
                                                    (curso.HorarioInicio < m.Curso.HorarioFin) &&
                                                    (m.Curso.HorarioInicio < curso.HorarioFin));

            if (solapados)
            {
                TempData["Error"] = "No se puede inscribir: el horario se solapa con otro curso ya matriculado.";
                return RedirectToAction("Detalle", "Catalogo", new { id = cursoId });
            }

            // Crear la matrícula
            var matricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = userId,
                FechaRegistro = DateTime.Now,
                Estado = EstadoMatricula.Pendiente
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Inscripción realizada correctamente.";
            return RedirectToAction("Detalle", "Catalogo", new { id = cursoId });
        }

    }
}
