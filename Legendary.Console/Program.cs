using System;
using System.Collections.Generic;
using Legendary.Core.Models;
using Legendary.Core.Types;
using Newtonsoft.Json;

namespace Legendary.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var mob = new Mobile()
            {
                FirstName = "Virgil",
                Level = 60,
                Description = "You see a man in brown robes standing before you. He appears eager to guide you through Hell.",
                Health = new MaxCurrent(10000, 10000),
                Mana = new MaxCurrent(10000, 10000),
                Movement = new MaxCurrent(10000, 10000),
                MobileId = 1,
                Currency = 1000,
                Experience = 10000000,
                MobileFlags = new List<MobileFlags>()               
            };

            var jsonObj = JsonConvert.SerializeObject(mob);
        }
    }
}
