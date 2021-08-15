public static class Rays {

    public readonly static ulong[] NORTH;
    public readonly static ulong[] SOUTH;
    public readonly static ulong[] EAST;
    public readonly static ulong[] WEST;

    public readonly static ulong[] NORTH_EAST;
    public readonly static ulong[] NORTH_WEST;
    public readonly static ulong[] SOUTH_EAST;
    public readonly static ulong[] SOUTH_WEST;

    //NORTH, SOUTH, EAST, WEST, NE, NW, SE, SW
    public static readonly int[] DirectionOffsets = new int[] { 8, -8, 1, -1, 9, 7, -7, -9 };

    public static readonly int[][] NumSquaresToEdge;

    static Rays() {

        NORTH = new ulong[64];
        SOUTH = new ulong[64];
        EAST = new ulong[64];
        WEST = new ulong[64];

        NORTH_EAST = new ulong[64];
        NORTH_WEST = new ulong[64];
        SOUTH_EAST = new ulong[64];
        SOUTH_WEST = new ulong[64];

        NumSquaresToEdge = new int[64][];

        for (int squareIndex = 0; squareIndex < 64; squareIndex++) {

            int siX = squareIndex % 8;
            int siY = squareIndex / 8;

            int north = 7 - siY;
            int south = siY;
            int west = siX;
            int east = 7 - siX;

            NumSquaresToEdge[squareIndex] = new int[8];
            NumSquaresToEdge[squareIndex][0] = north;
            NumSquaresToEdge[squareIndex][1] = south;
            NumSquaresToEdge[squareIndex][2] = east;
            NumSquaresToEdge[squareIndex][3] = west;
            NumSquaresToEdge[squareIndex][4] = System.Math.Min(north, east);
            NumSquaresToEdge[squareIndex][5] = System.Math.Min(north, west);
            NumSquaresToEdge[squareIndex][6] = System.Math.Min(south, east);
            NumSquaresToEdge[squareIndex][7] = System.Math.Min(south, west);

            for (int distance = 0; distance < 8; distance++) {

                int nTarget = (squareIndex + DirectionOffsets[0] * (distance + 1));
                if(nTarget >= 0 && nTarget <= 63)
                    NORTH[squareIndex] |= 1ul << nTarget;

                int sTarget = (squareIndex + DirectionOffsets[1] * (distance + 1));
                if (sTarget >= 0 && sTarget <= 63) 
                    SOUTH[squareIndex] |= 1ul << sTarget;

                int eTarget = (squareIndex + DirectionOffsets[2] * (distance + 1));
                if (eTarget >= 0 && eTarget <= 63 && siY == eTarget / 8)
                    EAST[squareIndex] |= 1ul << eTarget;


                int wTarget = (squareIndex + DirectionOffsets[3] * (distance + 1));
                if (wTarget >= 0 && wTarget <= 63 && siY == wTarget / 8)
                    WEST[squareIndex] |= 1ul << wTarget;

                int neTarget = (squareIndex + DirectionOffsets[4] * (distance + 1));
                if (neTarget >= 0 && neTarget <= 63 && neTarget % 8 > siX)
                    NORTH_EAST[squareIndex] |= 1ul << neTarget;

                int nwTarget = (squareIndex + DirectionOffsets[5] * (distance + 1));
                if (nwTarget >= 0 && nwTarget <= 63 && nwTarget % 8 < siX)
                    NORTH_WEST[squareIndex] |= 1ul << nwTarget;

                int seTarget = (squareIndex + DirectionOffsets[6] * (distance + 1));
                if (seTarget >= 0 && seTarget <= 63 && seTarget % 8 > siX)
                    SOUTH_EAST[squareIndex] |= 1ul << seTarget;

                int swTarget = (squareIndex + DirectionOffsets[7] * (distance + 1));
                if (swTarget >= 0 && swTarget <= 63 && swTarget % 8 < siX)
                    SOUTH_WEST[squareIndex] |= 1ul << swTarget;
            }
        }
    }
}
