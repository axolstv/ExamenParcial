using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.Models
{
    public class Matricula
    {
        public int Id { get; set; }

        [Required]
        public int CursoId { get; set; }
        public Curso? Curso { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        public IdentityUser Usuario { get; set; } = default!; 
        [DataType(DataType.DateTime)]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Required]
        public EstadoMatricula Estado { get; set; }
    }

    public enum EstadoMatricula
    {
        Pendiente,
        Confirmada,
        Cancelada
    }
}
