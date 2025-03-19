using System;
using System.Linq;
using System.Threading;
using Linq2GraphQL.Client;
using Microsoft.Extensions.Options;
using StartGG;
using StartGG.Client;
using StartGG.Inputs;
using StartGG.Types;


// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
string myKey = File.ReadLines("../../../lib/KeyFile.txt").First();
var myClient = new HttpClient();
myClient.DefaultRequestHeaders.Add("Authorization", myKey);
myClient.BaseAddress = new Uri("https://api.start.gg/gql/alpha");

IOptions<GraphClientOptions> myOptions = Options.Create<GraphClientOptions>(new GraphClientOptions());
var startggclient = new StartGGLibrary(myClient, myOptions, null);


/*
 * string myEventName = startggclient.Query.Event(null, "tournament/quickdraw-brawl-26/event/bbcf-double-elimination").Select(e => e.Name).ExecuteAsync().Result
 * //Console.WriteLine(myEventName);
 * EVERYTHING ABOVE THIS LINE WORKS PERFECTLY
 */


CancellationToken cancellationToken = CancellationToken.None;

// Get the first 5 tournaments that have "Genesis" in the name.
TournamentQuery myQuery = new TournamentQuery();
myQuery.PerPage = 5;
myQuery.Page = 1;

myQuery.Filter = new TournamentPageFilter(); ;
myQuery.Filter.Name("Genesis");

// Get the 4 highest placers for each event in the tournament
StandingPaginationQuery myStandingQuery = new StandingPaginationQuery();
myStandingQuery.Page = 1;
myStandingQuery.PerPage = 4;

//getting information from the API and storing it in a DTO(?) to use for my own purposes later
try
{

    var testQuery = startggclient.Query.Tournaments(myQuery).
    Select(tourneyConn => tourneyConn.Nodes.
    Select(tournament => tournament.Events.
    Select(myEvent =>
    new InfoStorage
    {
        numAttendees = myEvent.NumEntrants,
        slug = myEvent.Slug,
        topPlacers = myEvent.Standings(myStandingQuery).Nodes.Select(e=> e.Entrant.Name), //error occurs here
    })))
    .ExecuteAsync().Result;
    foreach (var item in testQuery)
    {
        foreach (var e in item)
        {
            Console.WriteLine(e.slug);          //works fine
            Console.WriteLine(e.numAttendees);  //works fine
            foreach (var f in e.topPlacers)    //breaks because (i'm assuming) you're trying to iterate through a null IEnumberable? I'm not 100% sure if it's that or if it's poor syntax on the API call or both. I've tried quite a few variations and I'm not sure how to fix it.
            {
                Console.WriteLine(f);
            }
        }
    }

}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

public class InfoStorage
{
    public int? numAttendees;
    public string? slug;
    public IEnumerable<string?>? topPlacers = new List<string>();
}

/*
 * 
 * The query I'm trying to replicate is as follows:
query TournamentEventPlacements($name: String!, $perPageT: Int, $perPageS: Int, $page: Int) {
  tournaments(query: {perPage: $perPageT, filter: {name: $name}}) {
    nodes {
      name
      events {
        name
        numEntrants
        standings(query: {perPage: $perPaggS, page: $page}) {
          nodes {
            placement
            entrant {
              id
              name
            }
          }
        }
      }
    }
  }
},{
  "name": "Genesis",
  "page": 1,
  "perPageT": 5,
  "perPageS": 4
},{
  "Authorization": "Bearer bf01876582bb578d64a02e1202b28f26"
}
 *
 * using the L2GQL library to get information from multiple levels and/or get information while inserting multiple query statements like I have in this GQL query I'm trying to replicate has been a consistent problem. If you know what subjects or examples I need to understand to make this process smoother I'd greatly appreciate that (alongside direct help too honestly please I've been working around not properly understanding this for around a month at this point but ill appreciate any help I can get.)
 */ 