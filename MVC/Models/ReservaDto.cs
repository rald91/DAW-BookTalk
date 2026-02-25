using System;

namespace MVC.Models
{
    public class ReservaDto
    {
        public int LivroId { get; set; }
        public DateOnly DataInicio { get; set; }
        public DateOnly DataFim { get; set; }
    }
}
