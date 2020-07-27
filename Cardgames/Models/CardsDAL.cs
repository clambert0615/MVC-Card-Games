using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cardgames.Models
{
    public class CardsDAL
    {
        public HttpClient GetHttpClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://deckofcardsapi.com/api/");
            return client;
        }

        //Get a new deck and specify how many decks you want
        public async Task<DeckCards> GetDeck(int deckcount)
        {
            var client = GetHttpClient();
            var response = await client.GetAsync($"deck/new/shuffle/?deck_count={deckcount}");
            //install-package Microsoft.AspNet.WebAPI.Client
            var deck = await response.Content.ReadAsAsync<DeckCards>();

            return deck;
        }
        //Draw/Deal a specific number of cards
        public async Task<DeckCards> GetCards(string deck_id, int cardcount)
        {
            
            var client = GetHttpClient();
            var response = await client.GetAsync($"deck/{deck_id}/draw/?count={cardcount}");
            var cardjson = await response.Content.ReadAsStringAsync();
            DeckCards cards = JsonConvert.DeserializeObject<DeckCards>(cardjson.ToString());
            return cards;
        }
    }
}
