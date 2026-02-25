using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public partial class Livro
{
    [Key]
    public int IdLivro { get; set; }

    [Required] 
    public string Titulo { get; set; } = null!;

    public string? ISBN { get; set; }

    public string? Idioma { get; set; }

    public DateOnly? AnoPublicacao { get; set; }
     
    public string? Sinopse { get; set; }
    
    public string Estado { get; set; } = "ativo";
    
    public string? CapaUrl { get; set; }

    public int? IdEditora { get; set; }

    public int Clicks { get; set; } = 0;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Reserva> IdReservas { get; set; } = new List<Reserva>();

    public virtual ICollection<Genero> Generos { get; set; } = new List<Genero>();

    public virtual ICollection<Autor> Autores { get; set; } = new List<Autor>();

    [ForeignKey(nameof(IdEditora))]
    public virtual Editora? EditoraNavigation { get; set; }
}
