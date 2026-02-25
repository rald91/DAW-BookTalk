using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public partial class Autor
{
    [Key]
    public int IdAutor { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public virtual ICollection<Livro> Livros { get; set; } = new List<Livro>();
}
