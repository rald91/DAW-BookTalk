using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public partial class Utilizador
{
    [Key]
    public int IdUtilizador { get; set; }

    [Required]
    public string TipoUtilizador { get; set; } = null!;

    [Required]
    public string Nome { get; set; } = null!;

    [Required] 
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
