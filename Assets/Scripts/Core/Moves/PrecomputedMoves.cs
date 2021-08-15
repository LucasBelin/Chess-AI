using System.Collections.Generic;

public static class PrecomputedMoves {

    static readonly int[] knightOffsets = new int[] {
        -17, -15, -6, 10, 17, 15, 6, -10
    };

    static readonly int[] kingOffsets = new int[] {
        7, 8, 9, 1, -7, -8, -9, -1
    };

    static readonly int[][] pawnAttackOffsets = new int[2][] {
        new int[] { 7, 9 },
        new int[] { -7, -9 }
    };

    static readonly int[][] pawnForwardOffsets = new int[2][] {
        new int[] { 8 },
        new int[] { -8 }
    };

    public static ulong[] KnightAttacksLookup;
    public static int[][] KnightMoves;

    public static ulong[] KingAttacksLookup;
    public static int[][] KingMoves;

    //color index, square index, array of moves
    public static int[][][] PawnCaptures;
    public static int[][][] PawnForward;
    public static ulong[][] PawnAttacksLookup;

    static PrecomputedMoves() {

        KnightAttacksLookup = new ulong[64];
        KnightMoves = new int[64][];

        KingAttacksLookup = new ulong[64];
        KingMoves = new int[64][];

        PawnAttacksLookup = new ulong[64][];
        PawnCaptures = new int[64][][];
        PawnForward = new int[64][][];

        for (int squareIndex = 0; squareIndex < 64; squareIndex++) {

            int siX = squareIndex % 8;
            int siY = squareIndex / 8;

            PawnAttacksLookup[squareIndex] = new ulong[2];
            PawnCaptures[squareIndex] = new int[2][];
            PawnForward[squareIndex] = new int[2][];

            List<int> knightMovesList = new List<int>();
            foreach(int offset in knightOffsets) {
                int target = squareIndex + offset;
                if(target >= 0 && target <= 63) {
                    int maxDist = Max(Abs(siX - target % 8), Abs(siY - target / 8));
                    if(maxDist == 2) {
                        KnightAttacksLookup[squareIndex] |= 1ul << target;
                        knightMovesList.Add(target);
                    }
                }
            }
            KnightMoves[squareIndex] = knightMovesList.ToArray();

            List<int> kingMovesList = new List<int>();
            foreach (int offset in kingOffsets) {
                int target = squareIndex + offset;
                if (target >= 0 && target <= 63) {
                    int maxDist = Max(Abs(siX - target % 8), Abs(siY - target / 8));
                    if (maxDist == 1) {
                        KingAttacksLookup[squareIndex] |= 1ul << target;
                        kingMovesList.Add(target);
                    }
                }
            }
            KingMoves[squareIndex] = kingMovesList.ToArray();

            for (int colorIndex = 0; colorIndex < 2; colorIndex++) {
                List<int> pawnDiagonals = new List<int>();
                foreach (int offset in pawnAttackOffsets[colorIndex]) {
                    int target = squareIndex + offset;
                    if (target >= 0 && target <= 63) {
                        int maxDist = Max(Abs(siX - target % 8), Abs(siY - target / 8));
                        if (maxDist == 1) {
                            PawnAttacksLookup[squareIndex][colorIndex] |= 1ul << target;
                            pawnDiagonals.Add(target);
                        }
                    }
                }
                PawnCaptures[squareIndex][colorIndex] = pawnDiagonals.ToArray();
            }

            for (int colorIndex = 0; colorIndex < 2; colorIndex++) {
                List<int> pawnMoves = new List<int>();
                foreach (int offset in pawnForwardOffsets[colorIndex]) {
                    int target = squareIndex + offset;
                    if (target >= 0 && target <= 63) {
                        pawnMoves.Add(target);
                    }
                }
                PawnForward[squareIndex][colorIndex] = pawnMoves.ToArray();
            }
        }
    }

    static int Max(int val1, int val2) {
        return System.Math.Max(val1, val2);
    }

    static int Abs(int value) {
        return System.Math.Abs(value);
    }
}
