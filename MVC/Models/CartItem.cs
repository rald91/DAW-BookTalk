using System;
using System.Collections.Generic;

namespace MVC.Models
{
    public class CartItem
    {
        public int LivroId { get; set; }
        public string LivroText { get; set; } = string.Empty;
        public DateOnly DataInicio { get; set; }
        public DateOnly DataFim { get; set; }
    }
}
