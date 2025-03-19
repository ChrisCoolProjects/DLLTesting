using System;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Linq2GraphQL.Client;
using Microsoft.Extensions.Options;
using StartGG;
using StartGG.Client;
using StartGG.Inputs;
using StartGG.Scalars;
using StartGG.Types;


// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
string myKey = File.ReadLines("../../../lib/KeyFile.txt").First();
var myClient = new HttpClient();
myClient.DefaultRequestHeaders.Add("Authorization", myKey);
myClient.BaseAddress = new Uri("https://api.start.gg/gql/alpha");

IOptions<GraphClientOptions> myOptions = Options.Create<GraphClientOptions>(new GraphClientOptions());
var startggclient = new StartGGLibrary(myClient, myOptions, null);

string myEventName = startggclient.Query.Event(null, "tournament/quickdraw-brawl-26/event/bbcf-double-elimination").Select(e => e.Name).ExecuteAsync().Result;

//Console.WriteLine(myEventName);

/*
 * EVERYTHING ABOVE THIS LINE WORKS PERFECTLY
 */


CancellationToken cancellationToken = CancellationToken.None;

TournamentQuery myQuery = new TournamentQuery();
myQuery.PerPage = 4;
myQuery.Page = 1;
myQuery.Filter = new TournamentPageFilter(); ;
myQuery.Filter.Name("Quickdraw");

EventFilter myEventFilter = new EventFilter();
myEventFilter.VideogameId = new List<string>();
myEventFilter.VideogameId.Add("50203");
//myEventFilter.Slug = "UNI2";
StandingPaginationQuery myStandingQuery = new StandingPaginationQuery();
myStandingQuery.Page = 1;
myStandingQuery.PerPage = 4;
/*
var QDBNames = await startggclient.Query.Tournaments(myQuery).Include(e => e.Nodes.Select(n => n.Name)).Select(e => e.Nodes).ExecuteAsync();
foreach (var q in QDBNames)
{
    Console.WriteLine(q);
}

Okay honestly not entirely sure what this section is.
I saw an example query in the documentation that formatted the query like this and I figured it'd be a good reference to go off of.
I haven't really dug in and analyzed this commented code at all (lack of time) but it may make more sense as a formatted query than what I've broken up below.
*/
try
{
    /*    GraphQuery<TournamentConnection> partOne = startggclient.Query.Tournaments(myQuery);
        GraphQueryExecute<TournamentConnection, TournamentConnection> partTwo = partOne.Select();
        Task<TournamentConnection> partThree = partTwo.ExecuteAsync(cancellationToken);
        TournamentConnection partFour = partThree.Result; //This is where the exception happens
        List<Tournament> partFive = partFour.Nodes;
        IEnumerable<string> partSix = partFive.Select(n => n.Name);
        foreach (var name in partSix)
        {
            Console.WriteLine(name);
        }
    /*  The above translates to the following:
        var testQuery = startggclient.Query.Tournaments(myQuery).Select().ExecuteAsync().Result.Nodes.Select(n => n.Name);

        If I take this and try to format it to match the QDBNames query I get:
    */

    //var testQuery = startggclient.Query.Tournaments(myQuery).Include(e=>e.Nodes.Select(n=>n.Name)).Select(e=>e.Nodes.Select(e=>e.NumAttendees)).ExecuteAsync().Result;
    //  var testQuery = startggclient.Query.Tournaments(myQuery).Select(e => e.Nodes.Select(e => e.Events.Select(e => e.Standings(myStandingQuery).Nodes.Select(e=>e.Player.GamerTag)))).ExecuteAsync().Result; //THIS WORKS!!!
    //Console.WriteLine(testQuery);
    Console.WriteLine("Hello World");


    var testQuery = startggclient.Query.Tournaments(myQuery).
    Select(tourneyConn => tourneyConn.Nodes.
    Select(tournament => tournament.Events.
    Select(myEvent => //myEvent.Standings(myStandingQuery).Nodes.Select(myStanding =>
    new InfoStorage
    {
        numAttendees = myEvent.NumEntrants,
        slug = myEvent.Slug,
        //topPlacers = (myEvent.Standings(myStandingQuery).Nodes.Select(a => a.Entrant.Name)),
    })))//)
    .ExecuteAsync().Result;
    InfoStorage myInfo = new InfoStorage();
    myInfo.prizePayout = myInfo.calcPayout(31);
    myInfo.topPlacers = new List<string?> { "Evangeline", "autodidact", "Cadedac", "Akai", "JuneBug", "ULTxSuperior" };
    myInfo.outputPayouts(myInfo.topPlacers, myInfo.prizePayout);
    foreach(var i in testQuery)
    {
        foreach(var i2 in i)
        {
                var podiumQuery = startggclient.Query.Event(null, i2.slug).
                    Select(a => a.Standings(myStandingQuery)).ExecuteAsync().Result;
                //i2.playerPlacement = podiumQuery;
                i2.prizePayout = i2.calcPayout(i2.numAttendees);
                Console.WriteLine(i2.numAttendees);
            var placementInfo = podiumQuery.Nodes.Select(e => e.Entrant.Name);
            i2.topPlacers = placementInfo;
            foreach(var placement in placementInfo)
            {
                Console.WriteLine(placement);
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
public class TournamentDTO
{
    public string Name;
    public string Id;
    List<EventDTO> Nodes;

}
public class EventDTO
{
    public string? Id;
    public string? Name
    {
        get {  return this.Name; }
        set { this.Name = value; }
    }
    public int? numAttendees;
    public bool? signupsOpen;

}

public class InfoStorage
{
    public string? Id;
    public string? Name
    {
        get {  return this.Name; }
        set { this.Name = value; }
    }
    public int? numAttendees;
    public bool? signupsOpen;
    public string? slug;
    public string? playerName;
    public int? playerPlacement;
    public List<PodiumSpots>? Nodes;
    public List<double?> prizePayout;
    public IEnumerable<string?>? topPlacers = new List<string>();
    //public List<string?>? topPlacers = new List<string>();
    public List<double?> calcPayout(int? numAttendees)
    {
        double? prizePool = numAttendees >= 5 ? 50 : 0 ;
        prizePool += (numAttendees * 5);
        Console.WriteLine(prizePool);
        List<double?> payouts = numAttendees switch
        {
            < 5           => new List<double?> { 0 },
            >= 5 and < 20 => new List<double?> { .50, .30, .20 },                           // Top 3 Payout
            >=20 and < 30 => new List<double?> { .50, .25, .15, .10 },                      // Top 4 Payout
            >=30 and < 40 => new List<double?> { .45, .20, .15, .10, .05, .05 },            // Top 6 Payout
            >=40          => new List<double?> { .42, .19, .14, .09, .05, .05, .03, .03 },  // Top 8 Payout
        };
        return payouts.Select(x => x * prizePool).ToList();
    }
    public void outputPayouts(IEnumerable<string> topPlacers, List<double?> prizePayout)
    {
        for (int i = 0; i < prizePayout.Count; i++)
        {
            Console.WriteLine($"{topPlacers.ElementAt(i)} won ${prizePayout.ElementAt(i)} in today's bracket.");
        }
    }
}
public class PodiumSpots
{
    public int? placing {  get; set; }
    public Entrant? podiumEntrant {  get; set; }

    public PodiumSpots(int? placing, Entrant? PodiumEntrant)
    {
        this.placing = placing;
        this.podiumEntrant = podiumEntrant;
    }
}