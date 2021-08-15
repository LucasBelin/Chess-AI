
public static class Piece {

    public const int None = 0;
    public const int King = 1;
    public const int Pawn = 2;
    public const int Knight = 3;
    public const int Bishop = 4;
    public const int Rook = 5;
    public const int Queen = 6;

    public const int White = 8;
    public const int Black = 16;

    const int typeMask = 0b00111;
    const int colorMask = 0b11000;

    public static int Color(int piece) {
        return piece & colorMask;
    }

    public static bool IsColor(int piece, int color) {
        return (piece & colorMask) == color;
    }

    public static int PieceType(int piece) {
        return piece & typeMask;
    }

    public static bool IsSlidingPiece(int piece) {
        return PieceType(piece) == Rook || PieceType(piece) == Queen || PieceType(piece) == Bishop;
    }

    public static bool IsBishopOrQueen(int pieceType) {
        return pieceType == Bishop || pieceType == Queen;
    }

    public static bool IsRookOrQueen(int pieceType) {
        return pieceType == Rook || pieceType == Queen;
    }
}
