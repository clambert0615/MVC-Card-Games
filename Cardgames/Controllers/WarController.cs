using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Cardgames.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Cardgames.Controllers
{
    public class WarController : Controller
    {

        private readonly GamesContext _context;
        private CardsDAL cd = new CardsDAL();
        public Match match;
        public Player1 player1;
        public Player2 player2;
        public DeckCards deck1;
        public List<Card> tieList;

        public WarController(GamesContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> WarNewGame()
        {
            var deck1 = await cd.GetDeck(1);
            DeckCards cards = await cd.GetCards(deck1.Deck_Id, 26);
            DeckCards cards2 = await cd.GetCards(deck1.Deck_Id, 26);
            player1 = new Player1 { CardList = cards.Cards };
            player2 = new Player2 { CardList = cards2.Cards };
            tieList = new List<Card>();
            match = new Match { Player1 = player1, Player2 = player2, Carddeck = deck1, TieList = tieList };
            HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));


            return View(match);
        }
        [Authorize]
        public IActionResult FlipCard(string value)
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));

            if (match.Player1.CardList.Count() > 0 && match.Player2.CardList.Count() > 0)
            {
                Card flipped = match.Player1.CardList.Find(x => x.Value == value);
                match.Player1.Flipped = flipped;
                ConvertFaceCards(flipped);
                int flippedValue = Convert.ToInt32(flipped.Value);
                Card computer = match.Player2.CardList[0];
                match.Player2.Computer = computer;
                ConvertFaceCards(computer);
                int computerValue = Convert.ToInt32(computer.Value);
                if (flippedValue > computerValue)
                {
                    match.Player1.CardList.Remove(flipped);
                    match.Player2.CardList.Remove(computer);
                    match.Player1.CardList.Add(flipped);
                    match.Player1.CardList.Add(computer);
                    match.Message = "You won";
                    HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                    return View(match);
                }
                else if (flippedValue < computerValue)
                {
                    match.Player1.CardList.Remove(flipped);
                    match.Player2.CardList.Remove(computer);
                    match.Player2.CardList.Add(flipped);
                    match.Player2.CardList.Add(computer);
                    match.Message = "Computer won";
                    HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                    return View(match);
                }
                else
                {
                    HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                    return RedirectToAction("Tie", match);
                }
            }
            else
            {
                HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                return RedirectToAction("EndGame", match);
            }
        }
        [Authorize]
        public IActionResult Tie()
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));
            return View(match);
        }
        [Authorize]
        public IActionResult TieResult(string value)
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));


            if (match.Player1.CardList.Count() > 2 && match.Player2.CardList.Count() > 2)
            {
                Card flipped = match.Player1.CardList.Find(x => x.Value == value);
                match.Player1.Flipped = flipped;
                ConvertFaceCards(flipped);
                int flippedValue = Convert.ToInt32(flipped.Value);
                Card computer = new Card();
                computer = match.Player2.CardList[2];
                match.Player2.Computer = computer;
                ConvertFaceCards(computer);
                int computerValue = Convert.ToInt32(computer.Value);
                if (flippedValue > computerValue)
                {
                    List<Card> winner = match.Player1.CardList;
                    DistributeCards(winner, flipped, computer);
                    match.Message = "You won";
                    HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                    return View(match);
                }
                else if (flippedValue < computerValue)
                {
                    List<Card> winner = match.Player2.CardList;
                    DistributeCards(winner, flipped, computer);
                    match.Message = "Computer Won";
                    HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                    return View(match);
                }
                else
                {
                    Card middlePlayer = match.Player1.CardList[0];
                    match.Player1.Middle = middlePlayer;
                    Card middleComputer = match.Player2.CardList[0];
                    match.Player2.Middle = middleComputer;
                    match.Player1.CardList.Remove(flipped);
                    match.Player1.CardList.Remove(middlePlayer);
                    match.Player2.CardList.Remove(computer);
                    match.Player2.CardList.Remove(middleComputer);
                    match.TieList.Add(flipped);
                    match.TieList.Add(middlePlayer);
                    match.TieList.Add(computer);
                    match.TieList.Add(middleComputer);
                    match.Message = "Tied again";
                    HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                    return RedirectToAction("MultipleTie", match);
                }
            }

            else
            {
                HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                return RedirectToAction("EndGame", match);
            }
        }
        [Authorize]
        public IActionResult MultipleTie(string value)
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));


            if (match.Player1.CardList.Count() > 2 && match.Player2.CardList.Count() > 2)
            {
                Card flipped = match.Player1.CardList.Find(x => x.Value == value);
                match.Player1.Flipped = flipped;
                ConvertFaceCards(flipped);
                int flippedValue = Convert.ToInt32(flipped.Value);
                Card computer = new Card();
                computer = match.Player2.CardList[1];
                match.Player2.Computer = computer;
                ConvertFaceCards(computer);
                int computerValue = Convert.ToInt32(computer.Value);
                if (flippedValue > computerValue)
                {
                    List<Card> winner = match.Player1.CardList;
                    MultipleDistributeCards(winner, flipped, computer);
                    match.Message = "You won";
                    HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                    return View(match);
                }
                else if (flippedValue < computerValue)
                {
                    List<Card> winner = match.Player2.CardList;
                    DistributeCards(winner, flipped, computer);
                    match.Message = "Computer Won";
                    HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                    return View(match);
                }
            }
            else
            {
                HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                return RedirectToAction("MultipleTie", match);
            }
            return RedirectToAction("EndGame", match);
        }

        [Authorize]
        public IActionResult EndGame()
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));
            if (match.Player1.CardList.Count() > match.Player2.CardList.Count())
            {
                string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                War results = new War
                {
                    UserId = id,
                    Wins = 1
                };
                if (ModelState.IsValid)
                {
                    _context.War.Add(results);
                    _context.SaveChanges();
                }
                match.Message = "Game over: You won!!";
                return View(match);
            }
            else
            {
                string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                War results = new War
                {
                    UserId = id,
                    Losses = 1
                };
                if (ModelState.IsValid)
                {
                    _context.War.Add(results);
                    _context.SaveChanges();
                }
                match.Message = "Game over: You lost, better luck next time";
                return View(match);
            }
        }

        public string ConvertFaceCards(Card cardToConvert)
        {
            if (cardToConvert.Value == "JACK")
            {
                return cardToConvert.Value = "11";
            }
            else if (cardToConvert.Value == "QUEEN")
            {
                return cardToConvert.Value = "12";
            }
            else if (cardToConvert.Value == "KING")
            {
                return cardToConvert.Value = "13";
            }
            else if (cardToConvert.Value == "ACE")
            {
                return cardToConvert.Value = "14";
            }
            else
            {
                return cardToConvert.Value;
            }
        }
        public void DistributeCards(List<Card> winner, Card flipped, Card computer)
        {

            Card middlePlayer = match.Player1.CardList[1];
            match.Player1.Middle = middlePlayer;
            Card bottomPlayer = match.Player1.CardList[0];
            match.Player1.Bottom = bottomPlayer;
            Card middleComputer = match.Player2.CardList[1];
            match.Player2.Middle = middleComputer;
            Card bottomComputer = match.Player2.CardList[0];
            match.Player2.Bottom = bottomComputer;
            match.Player1.CardList.Remove(flipped);
            match.Player1.CardList.Remove(middlePlayer);
            match.Player1.CardList.Remove(bottomPlayer);
            match.Player2.CardList.Remove(computer);
            match.Player2.CardList.Remove(middleComputer);
            match.Player2.CardList.Remove(bottomComputer);
            winner.Add(flipped);
            winner.Add(computer);
            winner.Add(middleComputer);
            winner.Add(bottomComputer);
            winner.Add(middlePlayer);
            winner.Add(bottomPlayer);
            if (match.TieList.Count > 0)
            {
                foreach (Card c in match.TieList)
                {
                    winner.Add(c);

                }
                match.TieList.Clear();
            }

        }
        public void MultipleDistributeCards(List<Card> winner, Card flipped, Card computer)
        {
            Card middlePlayer = match.Player1.CardList[0];
            match.Player1.Middle = middlePlayer;
            Card middleComputer = match.Player2.CardList[0];
            match.Player2.Middle = middleComputer;
            match.Player1.CardList.Remove(flipped);
            match.Player1.CardList.Remove(middlePlayer);
            match.Player2.CardList.Remove(computer);
            match.Player2.CardList.Remove(middleComputer);
            winner.Add(flipped);
            winner.Add(computer);
            winner.Add(middleComputer);
            winner.Add(middlePlayer);

            if (match.TieList.Count > 0)
            {
                foreach (Card c in match.TieList)
                {
                    winner.Add(c);
                }
                match.TieList.Clear();
            }

        }

    }
}
