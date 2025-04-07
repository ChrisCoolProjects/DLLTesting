using System.Text;
using Newtonsoft.Json;
using DLLTesting.StartGGLibrary;
using static DLLTesting.PayoutCalculator;

class Program
{
    static async Task Main()
    {
        string query = @"
        query TournamentEventPlacements($name: String!, $perPageT: Int, $perPageS: Int, $page: Int) {
          tournaments(query: {perPage: $perPageT, filter: {name: $name}}) {
            nodes {
              name
              events {
                name
                numEntrants
                standings(query: {perPage: $perPageS, page: $page}) {
                  nodes {
                    placement
                    player {
                      id
                      gamerTag
                    }
                  }
                }
              }
            }
          }
        }";

        var variables = new
        {
            name = "Quickdraw",
            perPageS = 8,
            perPageT = 5,
            page = 1
        };

        var requestBody = new
        {
            query,
            variables
        };

        string jsonRequestBody = JsonConvert.SerializeObject(requestBody);

        var myClient = new HttpClient();
        string myKey = File.ReadLines("../../../lib/KeyFile.txt").First();
        myClient.DefaultRequestHeaders.Add("Accept", "application/json");
        myClient.DefaultRequestHeaders.Add("Authorization", myKey);

        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await myClient.PostAsync("https://api.start.gg/gql/alpha", content);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            TournamentResponse tournamentResponse = JsonConvert.DeserializeObject<TournamentResponse>(responseContent);
            if (tournamentResponse?.Data?.Tournaments != null)
            {
                foreach (var tournament in tournamentResponse.Data.Tournaments.Nodes)
                {
                    Console.WriteLine($"Tournament Name: {tournament.Name}");
                    foreach (var eventItem in tournament.Events)
                    {
                        if (eventItem.Name == "QDB Dinner Headcount") continue;
                        Console.WriteLine($"  Event Name: {eventItem.Name}");
                        Console.WriteLine($"  Number of Entrants: {eventItem.NumEntrants}");
                        var payoutList = CalcPayout(eventItem.NumEntrants);
                        Console.WriteLine($"  Total prize pool: ${payoutList.Sum()}");
                        foreach (var standingItem in eventItem.Standings.Nodes)
                        {
                            if (standingItem.Placement <= payoutList.Count())
                            {
                                Console.WriteLine($"   {IntToPodiumName(standingItem.Placement)} place: {standingItem.Player.gamerTag} won ${payoutList.ElementAt(standingItem.Placement - 1)} ");
                            }
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }
            }
        }
        else
        {
            Console.WriteLine("Error: " + response.ReasonPhrase);
            string errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Content: {errorContent}");
        }
    }
}