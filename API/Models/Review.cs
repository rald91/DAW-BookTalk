using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public partial class Review
{
    [Key]
    public int IdReview { get; set; }

    [Required]
    public int IdUtilizador { get; set; }

    [Required]
    public int IdLivro { get; set; }

    public string? TextoReview { get; set; }

    [Range(1, 5)]
    public byte? Rating { get; set; }

    [Required]
    public string Status { get; set; } = null!;

    [Required]
    public DateOnly DataSubmissao { get; set; }

    public DateOnly? DataAprovacao { get; set; }

    [ForeignKey(nameof(IdLivro))]
    public virtual Livro IdLivroNavigation { get; set; } = null!;

    [ForeignKey(nameof(IdUtilizador))]
    public virtual Utilizador IdUtilizadorNavigation { get; set; } = null!;
}
