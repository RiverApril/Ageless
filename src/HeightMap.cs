namespace Ageless {
    public class HeightMap {
        
        public float[,] heights = new float[Chunk.CHUNK_SIZE_X + 1, Chunk.CHUNK_SIZE_Z + 1];
        
        public byte[,] tiles = new byte[Chunk.CHUNK_SIZE_X, Chunk.CHUNK_SIZE_Z];

		public float min, max;

        public bool isFloor = true;
        public bool isCeiling = false;
        public bool isSolid = true;

        
        public Tile getTile(int x, int z) {
            return Tile.fromIndex(tiles[x, z]);
        }

        public byte getIndex(int x, int z) {
            return tiles[x, z];
        }


    }
}