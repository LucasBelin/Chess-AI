using System.Collections.Generic;

public static class BitBoardUtility {

    private const ulong DeBruijnSequence = 0x37E84A99DAE458F;

    private static readonly int[] MultiplyDeBruijnBitPosition = {
        0, 1, 17, 2, 18, 50, 3, 57,
        47, 19, 22, 51, 29, 4, 33, 58,
        15, 48, 20, 27, 25, 23, 52, 41,
        54, 30, 38, 5, 43, 34, 59, 8,
        63, 16, 49, 56, 46, 21, 28, 32,
        14, 26, 24, 40, 53, 37, 42, 7,
        62, 55, 45, 31, 13, 39, 36, 6,
        61, 44, 12, 35, 60, 11, 10, 9,
    };

    private static readonly int[] ms1bTable;

    static BitBoardUtility() {
        ms1bTable = InitMS1BTable();
    }

    private static int[] InitMS1BTable() {
        int[] table = new int[256];
        for (int i = 0; i < 256; i++) {
            table[i] = i > 127 ? 7 :
                       i > 63 ? 6 :
                       i > 31 ? 5 :
                       i > 15 ? 4 :
                       i > 7 ? 3 :
                       i > 3 ? 2 :
                       i > 1 ? 1 : 0;
        }
        return table;
    }

    public static byte PopCount(ulong bb) {
        ulong result = bb - ((bb >> 1) & 0x5555555555555555UL);
        result = (result & 0x3333333333333333UL) + ((result >> 2) & 0x3333333333333333UL);
        return (byte)(unchecked(((result + (result >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
    }

    public static int[] GetSetBits(ulong bb) {
        List<int> positions = new List<int>();
        int position = 0;
        while (bb != 0) {
            if ((bb & 1) != 0) {
                positions.Add(position);
            }
            position++;
            bb >>= 1;
        }
        return positions.ToArray();
    }

    public static int BitscanForward(ulong bb) {
        return MultiplyDeBruijnBitPosition[((ulong)((long)bb & -(long)bb) * DeBruijnSequence) >> 58];
    }

    public static int BitscanReverse(ulong bb) {
        int result = 0;
        if (bb > 0xFFFFFFFF) {
            bb >>= 32;
            result = 32;
        }
        if (bb > 0xFFFF) {
            bb >>= 16;
            result += 16;
        }
        if (bb > 0xFF) {
            bb >>= 8;
            result += 8;
        }
        return result + ms1bTable[bb];
    }
}