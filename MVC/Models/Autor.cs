using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Models;

public partial class Autor
{
    [Key]
    [Display(Name = "ID do Autor")]
    public int IdAutor { get; set; }

    [Required]
    [Display(Name = "Nome")]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public virtual ICollection<Livro> Livros { get; set; } = new List<Livro>();
}
