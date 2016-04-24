namespace Ageless {
    public class HeightMap {

        public static readonly uint CHUNK_SIZE_X = 128;
        public static readonly uint CHUNK_SIZE_Z = 128;
        
        public float[,] heights = new float[CHUNK_SIZE_X + 1, CHUNK_SIZE_Z + 1];
        
        public byte[,] tiles = new byte[CHUNK_SIZE_X, CHUNK_SIZE_Z];

        public bool isFloor = true;
        public bool isCeiling = false;

        
        public Tile getTile(int x, int z) {
            return Tile.fromIndex(tiles[x, z]);
        }


    }
}