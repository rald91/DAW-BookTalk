using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public partial class Reserva
{
    [Key]
    public int IdReserva { get; set; }

    [Required]
    public int IdUtilizador { get; set; }

    [Required]
    public DateOnly DataInicio { get; set; }

    public DateOnly? DataFim { get; set; }

    public bool Confirmada { get; set; } = false;

    [ForeignKey(nameof(IdUtilizador))]
    public virtual Utilizador IdUtilizadorNavigation { get; set; } = null!;

    public virtual ICollection<Livro> IdLivros { get; set; } = new List<Livro>();
}
