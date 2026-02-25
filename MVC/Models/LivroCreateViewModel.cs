using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public class LivroCreateViewModel
{
    // Propriedades do Livro
    public int? IdLivro { get; set; }
    
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
    
    // Campos de texto separados por vírgulas (sem JavaScript necessário)
    [Display(Name = "Autor(es)")]
    public string? AuthorNamesString { get; set; } // Ex: "Autor1, Autor2, Autor3"
    
    [Display(Name = "Género(s)")]
    public string? GenreNamesString { get; set; } // Ex: "Ficção, Romance"
    
    // Campo para criar nova editora (se IdEditora não for selecionado)
    [Display(Name = "Ou criar nova editora")]
    public string? NovaEditoraNome { get; set; }
}
