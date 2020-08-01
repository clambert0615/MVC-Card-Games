using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Cardgames.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace Cardgames.Controllers
{
    public class BlackJackController : Controller
    {
        public BlackJackController(GamesContext context)
        {
            _context = context;
        }
        private readonly GamesContext _context;
        private CardsDAL cd = new CardsDAL();
        public Match match;
        public Player1 player1;
        public Player2 player2;
        public DeckCards deck1;

        public IActionResult BJIndex()
        {
            return View();
        }
        public async Task<IActionResult> BJNewGame()
        {
            var deck1 = await cd.GetDeck(1);
            DeckCards cards = await cd.GetCards(deck1.Deck_Id, 2);
            DeckCards cards2 = await cd.GetCards(deck1.Deck_Id, 2);
            player1 = new Player1 { CardList = cards.Cards };
            player2 = new Player2 { CardList = cards2.Cards };
            match = new Match { Player1 = player1, Player2 = player2, Carddeck = deck1 };
            HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
            HttpContext.Session.SetString("deck1", JsonConvert.SerializeObject(deck1));
            return RedirectToAction("FirstMove", match);

        }
        [Authorize]
        public IActionResult FirstMove()
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));
            deck1 = JsonConvert.DeserializeObject<DeckCards>(HttpContext.Session.GetString("deck1"));
            return View(match);
        }
        [Authorize]
        public async Task<IActionResult> NextMove(string decision, string acechoice)
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));
            deck1 = JsonConvert.DeserializeObject<DeckCards>(HttpContext.Session.GetString("deck1"));
            int sumOfCards = 0;
            int aceValue = 0;
            int sumOfCards5 = 0;
            int cardValue;

            foreach (Card c in match.Player1.CardList)
            {
                if (c.Value == "ACE")
                {
                    aceValue = Convert.ToInt32(acechoice);
                }
                else
                {
                    ConvertFaceCards(c);
                    cardValue = Convert.ToInt32(c.Value);
                    sumOfCards += cardValue;
                }

            }

            match.Player1.HandSum = sumOfCards + aceValue;
            GetPlayer2Sum(match.Player2.CardList);

            if (decision == "stay")
            {

                if (match.Player2.HandSum > 16)
                {

                    HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                    HttpContext.Session.SetString("deck1", JsonConvert.SerializeObject(deck1));
                    return RedirectToAction("GameOver", match);
                }
                else
                {
                    DeckCards card = await cd.GetCards(deck1.Deck_Id, 1);
                    match.Player2.CardList.Add(card.Cards[0]);
                    GetPlayer2Sum(match.Player2.CardList);

                    match.Player1.HandSum = sumOfCards + aceValue;
                    HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                    HttpContext.Session.SetString("deck1", JsonConvert.SerializeObject(deck1));
                    return RedirectToAction("GameOver", match);
                }
            }
            else
            {
                if (match.Player1.HandSum <= 21)
                {
                    DeckCards card = await cd.GetCards(deck1.Deck_Id, 1);
                    match.Player1.CardList.Add(card.Cards[0]);
                    DeckCards card2 = await cd.GetCards(deck1.Deck_Id, 1);
                    match.Player2.CardList.Add(card2.Cards[0]);
                    GetPlayer2Sum(match.Player2.CardList);
                    foreach (Card c in match.Player1.CardList)
                    {
                        if (c.Value != "ACE")
                        {

                            ConvertFaceCards(c);
                            int cvalues = Convert.ToInt32(c.Value);
                            sumOfCards5 += cvalues;
                        }

                    }
                    match.Player1.HandSum = sumOfCards5 + aceValue;

                    if (match.Player2.HandSum >= 21 || match.Player1.HandSum >= 20)
                    {

                        HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                        HttpContext.Session.SetString("deck1", JsonConvert.SerializeObject(deck1));
                        return RedirectToAction("GameOver", match);
                    }

                    else
                    {

                        HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                        HttpContext.Session.SetString("deck1", JsonConvert.SerializeObject(deck1));
                        return View(match);
                    }
                }
                else
                {
                    HttpContext.Session.SetString("match", JsonConvert.SerializeObject(match));
                    HttpContext.Session.SetString("deck1", JsonConvert.SerializeObject(deck1));
                    return RedirectToAction("GameOver", match);
                }
            }
        }
        [Authorize]
        public IActionResult GameOver()
        {
            match = JsonConvert.DeserializeObject<Match>(HttpContext.Session.GetString("match"));
            deck1 = JsonConvert.DeserializeObject<DeckCards>(HttpContext.Session.GetString("deck1"));
            if ((match.Player1.HandSum > match.Player2.HandSum && match.Player1.HandSum <= 21) || (match.Player2.HandSum > 21 && match.Player1.HandSum <= 21))
            {
                string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                BlackJack results = new BlackJack
                {
                    UserId = id,
                    Wins = 1
                };
                if (ModelState.IsValid)
                {
                    _context.BlackJack.Add(results);
                    _context.SaveChanges();
                }
                match.Message = "Game Over: You Won!";
                return View(match);
            }
            else if ((match.Player1.HandSum < match.Player2.HandSum && match.Player2.HandSum <= 21) || (match.Player1.HandSum > 21 && match.Player2.HandSum <= 21))
            {
                string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                BlackJack results = new BlackJack
                {
                    UserId = id,
                    Losses = 1
                };
                if (ModelState.IsValid)
                {
                    _context.BlackJack.Add(results);
                    _context.SaveChanges();
                }
                match.Message = "Game Over: You Lost, Better Luck Next Time";
                return View(match);
            }
            else if (match.Player1.HandSum > 21 && match.Player2.HandSum > 21)
            {
                string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                BlackJack results = new BlackJack
                {
                    UserId = id,
                    Losses = 1
                };
                if (ModelState.IsValid)
                {
                    _context.BlackJack.Add(results);
                    _context.SaveChanges();
                }
                match.Message = "Game Over: You Both Lost";
                return View(match);
            }
            else
            {
                string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                BlackJack results = new BlackJack
                {
                    UserId = id,
                    Ties = 1
                };
                if (ModelState.IsValid)
                {
                    _context.BlackJack.Add(results);
                    _context.SaveChanges();
                }
                match.Message = "Game Over: You Tied";
                return View(match);
            }
        }

        public string ConvertFaceCards(Card cardToConvert)
        {
            if (cardToConvert.Value == "JACK")
            {
                return cardToConvert.Value = "10";
            }
            else if (cardToConvert.Value == "QUEEN")
            {
                return cardToConvert.Value = "10";
            }
            else if (cardToConvert.Value == "KING")
            {
                return cardToConvert.Value = "10";
            }

            else
            {
                return cardToConvert.Value;
            }
        }
        public int GetPlayer2Sum(List<Card> cards)
        {
            int cardValue2;
            int sumOfCards2 = 0;
            int aceValue2 = 0;
                foreach (Card c in cards)
                {
                    if (c.Value != "ACE")
                    {

                        ConvertFaceCards(c);
                        cardValue2 = Convert.ToInt32(c.Value);
                        sumOfCards2 += cardValue2;
                    }
                }
                foreach (Card c in cards)
                {
                    if (c.Value == "ACE")
                    {
                        if (sumOfCards2 > 10)
                        {
                            aceValue2 = 1;
                        }
                        else
                        {
                            aceValue2 = 11;
                        }
                    }
                }

            
           return match.Player2.HandSum = sumOfCards2 + aceValue2;

        }
    }
}
