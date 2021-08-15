public static class Utils {

    public const int WhiteIndex = 0;
    public const int BlackIndex = 1;

    public const uint WhiteCastleKingsideMask = 0b1111111111111110;
    public const uint WhiteCastleQueensideMask = 0b1111111111111101;
    public const uint BlackCastleKingsideMask = 0b1111111111111011;
    public const uint BlackCastleQueensideMask = 0b1111111111110111;

    public const uint WhiteCastleMask = WhiteCastleKingsideMask & WhiteCastleQueensideMask;
    public const uint BlackCastleMask = BlackCastleKingsideMask & BlackCastleQueensideMask;

    public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public static bool InsufficientMaterial(this PieceList[] allPieceLists) {
        int whitePawnCount = allPieceLists[WhiteIndex * 8 + Piece.Pawn].Count;
        int whiteKnightCount = allPieceLists[WhiteIndex * 8 + Piece.Knight].Count;
        int whiteBishopCount = allPieceLists[WhiteIndex * 8 + Piece.Bishop].Count;
        int whiteRookCount = allPieceLists[WhiteIndex * 8 + Piece.Rook].Count;
        int whiteQueenCount = allPieceLists[WhiteIndex * 8 + Piece.Queen].Count;

        int blackPawnCount = allPieceLists[BlackIndex * 8 + Piece.Pawn].Count;
        int blackKnightCount = allPieceLists[BlackIndex * 8 + Piece.Knight].Count;
        int blackBishopCount = allPieceLists[BlackIndex * 8 + Piece.Bishop].Count;
        int blackRookCount = allPieceLists[BlackIndex * 8 + Piece.Rook].Count;
        int blackQueenCount = allPieceLists[BlackIndex * 8 + Piece.Queen].Count;

        int whitePieceCount = whitePawnCount + whiteKnightCount + whiteBishopCount + whiteRookCount + whiteQueenCount;
        int blackPieceCount = blackPawnCount + blackKnightCount + blackBishopCount + blackRookCount + blackQueenCount;

        //Only kings on board
        if (whitePieceCount == 0 && blackPieceCount == 0) return true;
        //Only king and one bishop or knight for each side
        else if ((whitePieceCount == 1 && whiteBishopCount == 1 && blackPieceCount == 1 && blackBishopCount == 1) ||
            (whitePieceCount == 1 && whiteKnightCount == 1 && blackPieceCount == 1 && blackKnightCount == 1)) return true;
        //King vs King + Knight
        else if ((whitePieceCount == 0 && blackPieceCount == 1 && blackKnightCount == 1) ||
            blackPieceCount == 0 && whitePieceCount == 1 && whiteKnightCount == 1) return true;

        return false;
    }
}
