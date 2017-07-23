using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonDigger.Generation
{
    public enum Tile
    {
        Unassigned,
        Wall,
        Room,
        Hallway,
        Stair,
        StairPartOne,
        StairPartTwo,
        DoorClosed,
        DoorOpen,
        DoorSecret,
        Unknown
    }
}
