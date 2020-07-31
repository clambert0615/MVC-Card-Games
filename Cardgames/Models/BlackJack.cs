using System;
using System.Collections.Generic;

namespace Cardgames.Models
{
    public partial class BlackJack
    {
        public int Id { get; set; }
        public int? Wins { get; set; }
        public int? Losses { get; set; }
        public int? Ties { get; set; }
        public string UserId { get; set; }

        public virtual AspNetUsers User { get; set; }
    }
}
