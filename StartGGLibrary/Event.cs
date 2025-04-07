using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLLTesting.StartGGLibrary
{
    public class Event
    {
        public string Name { get; set; }
        public int NumEntrants { get; set; }
        public StandingConnection Standings { get; set; }
    }
}
