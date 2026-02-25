using System;
using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class AccessLog
    {
        [Key]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int? UserId { get; set; }
        public string? Event { get; set; }
    }
}
