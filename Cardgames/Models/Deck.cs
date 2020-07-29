using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardgames.Models
{

    public class DeckCards
    {
        public bool Success { get; set; }
        public string Deck_Id { get; set; }
        public bool Shuffled { get; set; }
        public int Remaining { get; set; }
        public List<Card> Cards { get; set; }
        public Piles Piles { get; set; }

    }
    public class Card
    {
        public string Image { get; set; }
        public string Value { get; set; }
        public string Suit { get; set; }
        public string Code { get; set; }
    }


    public class Piles
    {
        public Discard Discard { get; set; }
        public Player1 Player1 { get; set; }
        public Player2 Player2 { get; set; }
    }

    public class Discard
    {
        public int Remaining { get; set; }
    }


    public class Player1
    {
        public int Remaining { get; set; }
        public List<Card> CardList { get; set; }
        public List<Card> Pile { get; set; } = new List<Card>();
        public Card Flipped { get; set; }
        public Card Middle { get; set; }
        public Card Bottom { get; set;  }
    }

    public class Player2
    {
        public List<Card> CardList { get; set; }
        public int Remaining { get; set; }
        public List<Card> Pile { get; set; } = new List<Card>();
        public Card Computer { get; set; }
        public Card Middle { get; set; }
        public Card Bottom { get; set; }
    }

    



}