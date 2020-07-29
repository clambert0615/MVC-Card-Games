using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cardgames.Models
{
    public class Match
    {
        public Player1 Player1 { get; set; }
        public Player2 Player2 { get; set; }
        public DeckCards Carddeck { get; set; }
        public List<Card> TieList { get; set; }

        public string Message { get; set; }


    }
}
