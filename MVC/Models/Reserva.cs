using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Models;

public partial class Reserva
{
    [Key]
    [Display(Name = "ID da Reserva")]
    public int IdReserva { get; set; }

    [Required]
    [Display(Name = "ID do Utilizador")]
    public int IdUtilizador { get; set; }

    [Required]
    [Display(Name = "Data de Início")]
    [DataType(DataType.Date)]
    public DateOnly DataInicio { get; set; }

    [Display(Name = "Data de Fim")]
    [DataType(DataType.Date)]
    public DateOnly? DataFim { get; set; }

    [Display(Name = "Confirmada")]
    public bool Confirmada { get; set; } = false;

    [ForeignKey(nameof(IdUtilizador))]
    public virtual Utilizador IdUtilizadorNavigation { get; set; } = null!;

    public virtual ICollection<Livro> IdLivros { get; set; } = new List<Livro>();
}
