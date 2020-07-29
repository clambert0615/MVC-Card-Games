using Cardgames.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
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

        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> NewGAme()
        {

            var deck1 = await cd.GetDeck(1);
            DeckCards cards = await cd.GetCards(deck1.Deck_Id, 5);
            DeckCards cards2 = await cd.GetCards(deck1.Deck_Id, 5);
            player1 = new Player1 { CardList = cards.Cards };
            player2 = new Player2 { CardList = cards2.Cards };
            match = new Match { Player1 = player1, Player2 = player2, Carddeck = deck1 };

            DupeCheck(match.Player1.CardList, match.Player1.Pile);
            DupeCheck(match.Player2.CardList, match.Player2.Pile);
            HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
            HttpContext.Session.SetString("deck1", JsonConvert.SerializeObject(deck1));

            return View(match);
        }
        [Authorize]
        public async Task<IActionResult> CardPick(string value)
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));
            deck1 = JsonConvert.DeserializeObject<DeckCards>(HttpContext.Session.GetString("deck1"));
            if (match.Player1.CardList.Count() > 0 && match.Player2.CardList.Count() > 0)
            {
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

                    DupeCheck(match.Player1.CardList, match.Player1.Pile);
                    match.Message = "Go fish";
                    HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                    HttpContext.Session.SetString("deck1", JsonConvert.SerializeObject(deck1));
                    return RedirectToAction("CardPick2", match);
                }
            }
            else 
            {
                HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                return RedirectToAction("GameOver", match);
            }
        }
        [Authorize]
        public IActionResult NewCardPick()
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));
            deck1 = JsonConvert.DeserializeObject<DeckCards>(HttpContext.Session.GetString("deck1"));
            if (match.Player1.CardList.Count() > 0 && match.Player2.CardList.Count() > 0)
            {
                return View(match);
            }
            else
            {
                HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                return RedirectToAction("GameOver", match);
            }
        }
        [Authorize]
        public async Task<IActionResult> CardPick2()
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));
            deck1 = JsonConvert.DeserializeObject<DeckCards>(HttpContext.Session.GetString("deck1"));
            if (match.Player1.CardList.Count() > 0 && match.Player2.CardList.Count() > 0)
            { 
                bool goAgain = true;
                while (goAgain)
                {
                    var random = new Random();
                    if (match.Player1.CardList.Count() > 0 && match.Player2.CardList.Count() > 0)
                    {
                        int index = random.Next(match.Player2.CardList.Count - 1);
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
                            DupeCheck(match.Player2.CardList, match.Player2.Pile);
                            match.Message = "Go fish";
                            HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                            HttpContext.Session.SetString("deck1", JsonConvert.SerializeObject(deck1));

                            goAgain = false;

                            return RedirectToAction("NewCardPick", match);
                        }
                    }
                    else
                    {
                        HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                        return RedirectToAction("GameOver", match);
                    }
                }
               
            }
            else
            {
                HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                return RedirectToAction("GameOver", match);
            }
            return RedirectToAction("NewCardPick", match);
        }
        public void DupeCheck(List<Card> cardList, List<Card> pileList)
        {
            if (cardList.GroupBy(x => x.Value).Any(n => n.Count() > 1))
            {
                var duplicates = cardList.GroupBy(s => s.Value)
                                                       .Where(g => g.Count() > 1)
                                                       .Select(s => s.Key);
                foreach (var dupeValue in duplicates)
                {
                    List<Card> dupes = cardList.Where(x => x.Value == dupeValue).ToList();
                    foreach (Card d in dupes)
                    {
                        pileList.Add(d);
                        cardList.Remove(d);
                    }

                }
            }
        }
        [Authorize]
        public IActionResult GameOver()
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));
            int player1Pairs = match.Player1.Pile.Count() / 2;
            int player2Pairs = match.Player2.Pile.Count() / 2;
            if (player1Pairs > player2Pairs)
            {
                string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                GoFish results = new GoFish
                {
                    UserId = id,
                    Wins = 1
                };
                if (ModelState.IsValid)
                {
                    _context.GoFish.Add(results);
                    _context.SaveChanges();
                }
                match.Message = $"Game Over: You won!  You had {player1Pairs} pairs and the computer had {player2Pairs} pairs";
            }
            else if (player1Pairs < player2Pairs)
            {
                string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                GoFish results = new GoFish
                {
                    UserId = id,
                    Losses = 1
                };
                if (ModelState.IsValid)
                {
                    _context.GoFish.Add(results);
                    _context.SaveChanges();
                }
                match.Message = $"Game Over: The computer won.  You had {player1Pairs} pairs and the computer had {player2Pairs} pairs";
            }
            else if(player1Pairs == player2Pairs)
            {
                string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                GoFish results = new GoFish
                {
                    UserId = id,
                    Ties = 1
                };
                if (ModelState.IsValid)
                {
                    _context.GoFish.Add(results);
                    _context.SaveChanges();
                }
                match.Message = $"Game Over: You tied!  You had {player1Pairs} pairs and the computer had {player2Pairs} pairs";
            }
            return View(match);

        }
        //public IActionResult Statistics()
        //{

        //    string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var stats = _context.GoFish.Where(x => x.UserId == id).ToList();
        //    foreach (var s in stats)
        //    {
        //        ViewBag.Wins = (ViewBag.Wins ?? 0) + (s.Wins ?? 0);
        //        ViewBag.Losses = (ViewBag.Losses ?? 0) + (s.Losses ?? 0);
        //        ViewBag.Ties = (ViewBag.Ties ?? 0) + (s.Ties ?? 0);
        //    }
        //    return View();
        //}
    }
}
