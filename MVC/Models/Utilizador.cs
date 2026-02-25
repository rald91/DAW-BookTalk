using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Models;

public partial class Utilizador
{
    [Key]
    [Display(Name = "ID do Utilizador")]
    public int IdUtilizador { get; set; }

    [Required]
    [Display(Name = "Tipo de Utilizador")]
    [AllowedValues("Bibliotecário", "Leitor")]
    public string TipoUtilizador { get; set; } = null!;

    [Required]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = null!;

    [Required] 
    [Display(Name = "Email")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A palavra-passe deve ter entre 6 e 100 caracteres")]
    [Display(Name = "Palavra-passe")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
