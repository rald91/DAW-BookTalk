using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Models;

public partial class Review
{
    [Key]
    [Display(Name = "ID da Review")]
    public int IdReview { get; set; }

    [Required]
    [Display(Name = "ID do Utilizador")]
    public int IdUtilizador { get; set; }

    [Required]
    [Display(Name = "ID do Livro")]
    public int IdLivro { get; set; }

    [Display(Name = "Texto da Review")]
    [DataType(DataType.MultilineText)]
    public string? TextoReview { get; set; }

    [Range(1, 5, ErrorMessage = "A classificação deve ser entre 1 e 5")]
    [Display(Name = "Classificação")]
    public byte? Rating { get; set; }

    [Required]
    [AllowedValues("Aprovada", "Rejeitada", "Pendente")]
    [Display(Name = "Status")]
    public string Status { get; set; } = null!;

    [Required]
    [Display(Name = "Data de Submissão")]
    [DataType(DataType.Date)]
    public DateOnly DataSubmissao { get; set; }

    [Display(Name = "Data de Aprovação")]
    [DataType(DataType.Date)]
    public DateOnly? DataAprovacao { get; set; }

    [ForeignKey(nameof(IdLivro))]
    public virtual Livro IdLivroNavigation { get; set; } = null!;

    [ForeignKey(nameof(IdUtilizador))]
    public virtual Utilizador IdUtilizadorNavigation { get; set; } = null!;
}
