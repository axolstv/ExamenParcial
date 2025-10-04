using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Los créditos deben ser mayores que 0")]
        public int Creditos { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El cupo máximo debe ser mayor que 0")]
        public int CupoMaximo { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan HorarioInicio { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan HorarioFin { get; set; }

        public bool Activo { get; set; }

        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

    }
}
