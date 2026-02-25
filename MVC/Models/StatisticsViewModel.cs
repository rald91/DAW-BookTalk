using System.Collections.Generic;

namespace MVC.Models
{
    public class StatsItem
    {
        public int IdLivro { get; set; }
        public string Titulo { get; set; } = "";
        public string Autor { get; set; } = "";
        public int Value { get; set; }
    }

    public class StatisticsViewModel
    {
        public List<StatsItem> TopClicked { get; set; } = new List<StatsItem>();
        public List<StatsItem> TopRequested { get; set; } = new List<StatsItem>();
        public Dictionary<string,int> TimeOfDayCounts { get; set; } = new Dictionary<string,int>();
        public List<UserTimeItem> UserTimes { get; set; } = new List<UserTimeItem>();
    }

    public class UserTimeItem
    {
        public string UserName { get; set; } = "";
        public string Time { get; set; } = "";
    }
}
