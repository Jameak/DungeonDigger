using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonDigger.Generation
{
    public interface IGenerator
    {
        Tile[,] Construct();
    }
}
