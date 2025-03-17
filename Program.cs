using System;
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
myQuery.Filter = new TournamentPageFilter();;
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


    /*    var testQuery = startggclient.Query.Tournaments(myQuery).Include(t => t.Nodes.Select(e =>
        new EventInstance
        {
            Name = e.Name,
            //numAttendees = e.NumAttendees,
            signupsOpen = e.IsRegistrationOpen,
            //numAttendees = e.Events(null, myEventFilter).First<Event>().NumEntrants,
            //TournamentID = e.Id.ToString(),
            //standings 
            eventList = e.Events(null, myEventFilter).ToList(),
            TournamentID = e.Slug,
        })).Select(e => e.Nodes.Select(e => e.Events.Select(e=>e.Name))).ExecuteAsync().Result;

        foreach (var i in testQuery)
        {
            foreach (var j in i)
            {
                Console.WriteLine(j.ToString());
            }
        }*/

    //var testQuery = startggclient.Query.Tournaments(myQuery).Select(e => e.Nodes).ExecuteAsync().Result;
    var testQuery = startggclient.Query.Tournaments(myQuery).Select(e => e.Nodes.Select(e => e.Events.Select(e=>e))).ExecuteAsync().Result;
        //    MasterList.Tournaments = testQuery.ToList();
        foreach (var myevent in MasterList.Events.Select(e=>e))
    {
        Console.WriteLine(myevent.Name);
    }
    foreach (var node in testQuery)
    {
 //       MasterList.Events.AddRange(node.Events.Select(e => e).Where(e => e.Videogame.Id == "50203"));
         Console.WriteLine(node);
    }
    foreach (var node in MasterList.Events)
    {
        Console.WriteLine(node.Name);
        MasterList.StandingConnections.Add(node.Standings);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
public static class MasterList
{
    public static List<Tournament> Tournaments { get; set; } = new List<Tournament>();  
    public static List<Event> Events { get; set; } = new List<Event>();
    public static List<StandingConnection> StandingConnections { get; set; } = new List<StandingConnection>();
}