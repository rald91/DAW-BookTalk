using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Models;

public partial class Livro
{
    [Key]
    [Display(Name = "ID do Livro")]
    public int IdLivro { get; set; }

    [Required] 
    [Display(Name = "Título")]
    public string Titulo { get; set; } = null!;

    [Display(Name = "ISBN")]
    public string? ISBN { get; set; }

    [Display(Name = "Idioma")]
    public string? Idioma { get; set; }

    [Display(Name = "Ano de Publicação")]
    [DataType(DataType.Date)]
    public DateOnly? AnoPublicacao { get; set; }
     
    [Display(Name = "Sinopse")]
    [DataType(DataType.MultilineText)]
    public string? Sinopse { get; set; }
    [Display(Name = "Estado")]
    public string Estado { get; set; } = "ativo";
    [Display(Name = "URL da Capa")]
    [DataType(DataType.ImageUrl)]
    public string? CapaUrl { get; set; }

    [Display(Name = "ID da Editora")]
    public int? IdEditora { get; set; }

    [Display(Name = "Cliques")]
    public int Clicks { get; set; } = 0;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Reserva> IdReservas { get; set; } = new List<Reserva>();

    public virtual ICollection<Genero> Generos { get; set; } = new List<Genero>();

    public virtual ICollection<Autor> Autores { get; set; } = new List<Autor>();

    [ForeignKey(nameof(IdEditora))]
    public virtual Editora? EditoraNavigation { get; set; }
}
