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

string myEventName = startggclient.Query.Event(null, "tournament/quickdraw-brawl-26/event/bbcf-double-elimination").Select(e => e.Name).ExecuteAsync().Result;

Console.WriteLine(myEventName);

/*
 * EVERYTHING ABOVE THIS LINE WORKS PERFECTLY
 */


CancellationToken cancellationToken = CancellationToken.None;

TournamentQuery myQuery = new TournamentQuery();
myQuery.PerPage = 4;
myQuery.Page = 1;
myQuery.Filter = new TournamentPageFilter();
myQuery.Filter.Name("Quickdraw Brawl");

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
    GraphQuery<TournamentConnection> partOne = startggclient.Query.Tournaments(myQuery);
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
    var testQuery = startggclient.Query.Tournaments(myQuery).Include(e => e.Nodes.Select(n => n.Name)).Select(e => e.Nodes).ExecuteAsync(cancellationToken).Result;
    foreach (var q in testQuery)
    {
        Console.WriteLine(q);
    }
idk ill look at it more later
*/
    }
catch(Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}