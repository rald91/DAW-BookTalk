using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Models;

public partial class Editora
{
    [Key]
    [Display(Name = "ID da Editora")]
    public int IdEditora { get; set; }

    [Required]
    [Display(Name = "Nome")]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public virtual ICollection<Livro> Livros { get; set; } = new List<Livro>();
}
