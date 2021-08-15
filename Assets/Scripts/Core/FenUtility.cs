using System.Collections.Generic;

public static class FenUtility {

    private static readonly Dictionary<char, int> pieceTypeFromSymbol = new Dictionary<char, int>() {
        ['k'] = Piece.King,
        ['p'] = Piece.Pawn,
        ['n'] = Piece.Knight,
        ['b'] = Piece.Bishop,
        ['r'] = Piece.Rook,
        ['q'] = Piece.Queen
    };

    public static LoadedPositionInfo PositionFromFen(string fen) {

        LoadedPositionInfo info = new LoadedPositionInfo();
        string[] sections = fen.Split(' ');
        int file = 0;
        int rank = 7;

        foreach (char symbol in sections[0]) {
            if (symbol == '/') {
                file = 0;
                rank--;
            }
            else {
                if (char.IsDigit(symbol)) {
                    file += (int)char.GetNumericValue(symbol);
                }
                else {
                    int pieceColor = char.IsUpper(symbol) ? Piece.White : Piece.Black;
                    int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                    info.squares[rank * 8 + file] = pieceColor | pieceType;
                    file++;
                }
            }
        }

        info.whiteToMove = sections[1] == "w";

        string castlingRights = sections[2];
        info.whiteCastleKingSide = castlingRights.Contains("K");
        info.whiteCastleQueenSide = castlingRights.Contains("Q");
        info.blackCastleKingSide = castlingRights.Contains("k");
        info.blackCastleQueenSide = castlingRights.Contains("q");

        if (sections[3] != "-") info.epFile = Coord.SquareCoordinateFromName(sections[3]).File + 1;

        if (sections.Length > 4) info.fiftyMoveCounter = int.Parse(sections[4]);
        else info.fiftyMoveCounter = 0;
        if (sections.Length > 5) info.plyCount = (int.Parse(sections[5]) * 2) - 2;
        else info.plyCount = 0;

        return info;
    }

    public static string FenFromBoard(Board board) {
        string fen = "";
        for (int rank = 7; rank >= 0; rank--) {
            int numEmptyFiles = 0;
            for (int file = 0; file < 8; file++) {
                int i = rank * 8 + file;
                int piece = board.Squares[i];
                if (piece != 0) {
                    if (numEmptyFiles != 0) {
                        fen += numEmptyFiles;
                        numEmptyFiles = 0;
                    }
                    bool isBlack = Piece.IsColor(piece, Piece.Black);
                    int pieceType = Piece.PieceType(piece);
                    char pieceChar = ' ';
                    switch (pieceType) {
                        case Piece.Rook:
                            pieceChar = 'R';
                            break;
                        case Piece.Knight:
                            pieceChar = 'N';
                            break;
                        case Piece.Bishop:
                            pieceChar = 'B';
                            break;
                        case Piece.Queen:
                            pieceChar = 'Q';
                            break;
                        case Piece.King:
                            pieceChar = 'K';
                            break;
                        case Piece.Pawn:
                            pieceChar = 'P';
                            break;
                    }
                    fen += isBlack ? pieceChar.ToString().ToLower() : pieceChar.ToString();
                }
                else {
                    numEmptyFiles++;
                }

            }
            if (numEmptyFiles != 0) {
                fen += numEmptyFiles;
            }
            if (rank != 0) {
                fen += '/';
            }
        }

        // Side to move
        fen += ' ';
        fen += (board.WhiteToMove) ? 'w' : 'b';

        // Castling
        bool whiteKingside = (board.CurrentGameState & 1) == 1;
        bool whiteQueenside = (board.CurrentGameState >> 1 & 1) == 1;
        bool blackKingside = (board.CurrentGameState >> 2 & 1) == 1;
        bool blackQueenside = (board.CurrentGameState >> 3 & 1) == 1;
        fen += ' ';
        fen += (whiteKingside) ? "K" : "";
        fen += (whiteQueenside) ? "Q" : "";
        fen += (blackKingside) ? "k" : "";
        fen += (blackQueenside) ? "q" : "";
        fen += ((board.CurrentGameState & 15) == 0) ? "-" : "";

        // En-passant
        fen += ' ';
        int epFile = (int)(board.CurrentGameState >> 4) & 15;
        if (epFile == 0) {
            fen += '-';
        }
        else {
            string fileName = Coord.FileNames[epFile - 1].ToString();
            int epRank = board.WhiteToMove ? 6 : 3;
            fen += fileName + epRank;
        }

        // 50 move counter
        fen += ' ';
        fen += board.FiftyMoveCounter;

        // Full-move count
        fen += ' ';
        fen += (board.PlyCount / 2) + 1;

        return fen;
    }

    public class LoadedPositionInfo {
        public int[] squares;
        public bool whiteCastleKingSide;
        public bool whiteCastleQueenSide;
        public bool blackCastleKingSide;
        public bool blackCastleQueenSide;
        public int epFile;
        public bool whiteToMove;
        public int fiftyMoveCounter;
        public int plyCount;

        public LoadedPositionInfo() {
            squares = new int[64];
        }
    }
}