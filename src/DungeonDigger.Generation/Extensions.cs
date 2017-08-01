using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonDigger.Generation
{
    public static class Extensions
    {
        public static double RandomDouble(this Random rnd ,double lowest, double highest)
        {
            return rnd.NextDouble() * (highest - lowest) + lowest;
        }
    }
}