using Cardgames.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Cardgames.Controllers
{
    public class GoFishController : Controller
    {
        private readonly GamesContext _context;
        private CardsDAL cd = new CardsDAL();
        public Match match;
        public Player1 player1;
        public Player2 player2;
        public Piles piles;
        public DeckCards deck1;
        

        public GoFishController(GamesContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {

            var deck1 = await cd.GetDeck(1);
            DeckCards cards = await cd.GetCards(deck1.Deck_Id, 5);
            DeckCards cards2 = await cd.GetCards(deck1.Deck_Id, 5);
            player1 = new Player1 { CardList = cards.Cards, Remaining = cards.Cards.Count };
            player2 = new Player2 { CardList = cards2.Cards, Remaining = cards2.Cards.Count };
            match = new Match { Player1 = player1, Player2 = player2, Carddeck = deck1 };

            HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
            HttpContext.Session.SetString("deck1", JsonConvert.SerializeObject(deck1));

            return View(match);
        }

        public async Task<IActionResult> CardPick(string value)
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));
            deck1 = JsonConvert.DeserializeObject<DeckCards>(HttpContext.Session.GetString("deck1"));

            Card cardToMatch = match.Player1.CardList.Find(x => x.Value == value);
            if (match.Player2.CardList.Any(x => x.Value == value))
            {
                Card matched = match.Player2.CardList.Find(x => x.Value == value);
                match.Player1.Pile.Add(cardToMatch);
                match.Player1.Pile.Add(matched);
                match.Player1.CardList.Remove(cardToMatch);
                match.Player2.CardList.Remove(matched);
                match.Message = "Go again";

                HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                HttpContext.Session.SetString("deck1", JsonConvert.SerializeObject(deck1));

                return RedirectToAction("NewCardPick", match);
            }
            else
            {
                DeckCards card = await cd.GetCards(deck1.Deck_Id, 1);
                match.Player1.CardList.Add(card.Cards[0]);
                match.Message = "Go fish";
                HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                HttpContext.Session.SetString("deck1", JsonConvert.SerializeObject(deck1));
                return RedirectToAction("CardPick2", match);
            }
        }

        public IActionResult NewCardPick()
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));
            deck1 = JsonConvert.DeserializeObject<DeckCards>(HttpContext.Session.GetString("deck1"));
            return View(match);
        }
        public async Task<IActionResult> CardPick2()
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));
                 deck1 = JsonConvert.DeserializeObject<DeckCards>(HttpContext.Session.GetString("deck1"));
            bool goAgain = true;
            while (goAgain)
            {
                var random = new Random();
                int index = random.Next(match.Player2.CardList.Count);
                Card cardAtIndex = match.Player2.CardList[index];
                if (match.Player1.CardList.Any(x => x.Value == cardAtIndex.Value))
                {
                    Card matched = match.Player1.CardList.Find(x => x.Value == cardAtIndex.Value);
                    match.Player2.Pile.Add(cardAtIndex);
                    match.Player2.Pile.Add(matched);
                    match.Player2.CardList.Remove(cardAtIndex);
                    match.Player1.CardList.Remove(matched);
                    match.Message = "Go again";
                    goAgain = true;
                }
                else
                {
                    DeckCards card = await cd.GetCards(deck1.Deck_Id, 1);
                    match.Player2.CardList.Add(card.Cards[0]);
                    match.Message = "Go fish";
                    HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                    HttpContext.Session.SetString("deck1", JsonConvert.SerializeObject(deck1));

                    goAgain = false;

                    return RedirectToAction("NewCardPick", match);
                }

            }
            return RedirectToAction("NewCardPick", match);
        }
    }
}
