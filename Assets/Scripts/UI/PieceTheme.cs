using UnityEngine;

[CreateAssetMenu(menuName = "Theme/Pieces")]
public class PieceTheme : ScriptableObject {
    public PieceSprites WhitePieces;
    public PieceSprites BlackPieces;

    public Sprite GetPieceSprite(int piece) {
        PieceSprites pieceSprites = Piece.Color(piece) == Piece.White ? WhitePieces : BlackPieces;
        return Piece.PieceType(piece) switch {
            Piece.Pawn => pieceSprites.pawn,
            Piece.Rook => pieceSprites.rook,
            Piece.Knight => pieceSprites.knight,
            Piece.Bishop => pieceSprites.bishop,
            Piece.Queen => pieceSprites.queen,
            Piece.King => pieceSprites.king,
            _ => null,
        };
    }

    [System.Serializable]
    public class PieceSprites {
        public Sprite king;
        public Sprite pawn;
        public Sprite knight;
        public Sprite bishop;
        public Sprite rook;
        public Sprite queen;
    }
}
