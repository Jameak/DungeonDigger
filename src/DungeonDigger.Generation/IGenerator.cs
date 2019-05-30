namespace DungeonDigger.Generation
{
    public interface IGenerator
    {
        Tile[,] Construct();
    }
}
