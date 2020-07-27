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
        //public Piles Piles { get; set; }
        public string Message { get; set; }


        //public Match(Player1 player1, Player2 player2, DeckCards carddeck, Piles piles, string message )
        //{
        //    Player1 = player1;
        //    Player2 = player2;
        //    Carddeck = carddeck;
        //    Piles = piles;
        //    Message = message;
        //}
}
}
