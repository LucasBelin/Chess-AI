using System.Collections.Generic;
using static Utils;

public class Board {

    public int[] Squares;
    public int PlyCount;
    public int FiftyMoveCounter;

    public bool WhiteToMove;
    public int ColorToMove;
    public int EnemyColor;
    public int ColorToMoveIndex;
    public int EnemyColorIndex;

    public PieceList[] Pawns;
    public PieceList[] Knights;
    public PieceList[] Bishops;
    public PieceList[] Rooks;
    public PieceList[] Queens;
    public PieceList[] Kings;
    public PieceList[] AllPieceLists;

    public ulong Blockers;

    public uint CurrentGameState;
    public Stack<uint> GameStateHistory;
    public List<string> MovesPlayed;

    PieceList GetPieceList(int pieceType, int colorIndex) {
        return AllPieceLists[colorIndex * 8 + pieceType];
    }

    public void CalculateBlockers() {
        Blockers = 0;
        foreach(PieceList list in AllPieceLists) {
            for (int i = 0; i < list.Count; i++) {
                Blockers |= 1ul << list[i];
            }
        }
    }

    public void LoadStartPosition() {
        LoadPosition(StartFen);
    }

    public void LoadPosition(string fen) {
        Initialize();
        var loadedPosition = FenUtility.PositionFromFen(fen);
        for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
            int piece = loadedPosition.squares[squareIndex];
            Squares[squareIndex] = piece;

            if(piece != Piece.None) {
                int pieceType = Piece.PieceType(piece);
                int colorIndex = (Piece.IsColor(piece, Piece.White)) ? WhiteIndex : BlackIndex;
                switch(pieceType) {
                    case Piece.Pawn:
                        Pawns[colorIndex].AddPieceAtSquare(squareIndex);
                        break;
                    case Piece.Knight:
                        Knights[colorIndex].AddPieceAtSquare(squareIndex);
                        break;
                    case Piece.Bishop:
                        Bishops[colorIndex].AddPieceAtSquare(squareIndex);
                        break;
                    case Piece.Rook:
                        Rooks[colorIndex].AddPieceAtSquare(squareIndex);
                        break;
                    case Piece.Queen:
                        Queens[colorIndex].AddPieceAtSquare(squareIndex);
                        break;
                    case Piece.King:
                        Kings[colorIndex].AddPieceAtSquare(squareIndex);
                        break;
                }
            }
        }

        WhiteToMove = loadedPosition.whiteToMove;
        ColorToMove = WhiteToMove ? Piece.White : Piece.Black;
        EnemyColor = WhiteToMove ? Piece.Black : Piece.White;
        ColorToMoveIndex = WhiteToMove ? WhiteIndex : BlackIndex;
        EnemyColorIndex = WhiteToMove ? BlackIndex : WhiteIndex;

        int whiteCastle = (loadedPosition.whiteCastleKingSide ? 1 << 0 : 0) | (loadedPosition.whiteCastleQueenSide ? 1 << 1 : 0);
        int blackCastle = (loadedPosition.blackCastleKingSide ? 1 << 2 : 0) | (loadedPosition.blackCastleQueenSide ? 1 << 3 : 0);
        int epState = loadedPosition.epFile << 4;
        ushort initialGameState = (ushort)(whiteCastle | blackCastle | epState);
        GameStateHistory.Push(initialGameState);
        CurrentGameState = initialGameState;
        FiftyMoveCounter = loadedPosition.fiftyMoveCounter;
        PlyCount = loadedPosition.plyCount;

        CalculateBlockers();
    }

    public void Initialize() {
        MovesPlayed = new List<string>();

        Squares = new int[64];
        FiftyMoveCounter = 0;
        Blockers = 0;
        GameStateHistory = new Stack<uint>();

        Pawns = new PieceList[] { new PieceList(8), new PieceList(8) };
        Knights = new PieceList[] { new PieceList(10), new PieceList(10) };
        Bishops = new PieceList[] { new PieceList(10), new PieceList(10) };
        Rooks = new PieceList[] { new PieceList(10), new PieceList(10) };
        Queens = new PieceList[] { new PieceList(9), new PieceList(9) };
        Kings = new PieceList[] { new PieceList(1), new PieceList(1) };
        PieceList emptyList = new PieceList(0);

        AllPieceLists = new PieceList[] {
            emptyList,
            Kings[WhiteIndex],
            Pawns[WhiteIndex],
            Knights[WhiteIndex],
            Bishops[WhiteIndex],
            Rooks[WhiteIndex],
            Queens[WhiteIndex],
            emptyList,
            emptyList,
            Kings[BlackIndex],
            Pawns[BlackIndex],
            Knights[BlackIndex],
            Bishops[BlackIndex],
            Rooks[BlackIndex],
            Queens[BlackIndex],
        };
    }

    public void MakeMove(Move move) {
        uint originalCastleState = CurrentGameState & 15;
        uint newCastleState = originalCastleState;
        CurrentGameState = 0;

        int moveFrom = move.StartSquare;
        int moveTo = move.TargetSquare;

        int capturedPieceType = Piece.PieceType(Squares[moveTo]);
        int movePiece = Squares[moveFrom];
        int movePieceType = Piece.PieceType(movePiece);

        int moveFlag = move.MoveFlag;
        bool isPromotion = move.IsPromotion;
        bool isEnPassant = moveFlag == Move.Flag.EnPassantCapture;

        MovesPlayed.Add(move.Notation);

        // Handle captures
        CurrentGameState |= (ushort)(capturedPieceType << 8);
        if (capturedPieceType != 0 && !isEnPassant) {
            GetPieceList(capturedPieceType, EnemyColorIndex).RemovePieceAtSquare(moveTo);
        }

        // Move pieces in piece lists
        if (movePieceType == Piece.King) {
            newCastleState &= WhiteToMove ? WhiteCastleMask : BlackCastleMask;
        }
        GetPieceList(movePieceType, ColorToMoveIndex).MovePiece(moveFrom, moveTo);

        int pieceOnTargetSquare = movePiece;

        // Handle promotion
        if (isPromotion) {
            int promoteType = 0;
            switch (moveFlag) {
                case Move.Flag.PromoteToQueen:
                    promoteType = Piece.Queen;
                    Queens[ColorToMoveIndex].AddPieceAtSquare(moveTo);
                    break;
                case Move.Flag.PromoteToRook:
                    promoteType = Piece.Rook;
                    Rooks[ColorToMoveIndex].AddPieceAtSquare(moveTo);
                    break;
                case Move.Flag.PromoteToBishop:
                    promoteType = Piece.Bishop;
                    Bishops[ColorToMoveIndex].AddPieceAtSquare(moveTo);
                    break;
                case Move.Flag.PromoteToKnight:
                    promoteType = Piece.Knight;
                    Knights[ColorToMoveIndex].AddPieceAtSquare(moveTo);
                    break;

            }
            pieceOnTargetSquare = promoteType | ColorToMove;
            Pawns[ColorToMoveIndex].RemovePieceAtSquare(moveTo);
        }
        else {
            switch (moveFlag) {
                case Move.Flag.EnPassantCapture:
                    int epPawnSquare = moveTo + ((ColorToMove == Piece.White) ? -8 : 8);
                    CurrentGameState |= (ushort)(Squares[epPawnSquare] << 8); // add pawn as capture type
                    Squares[epPawnSquare] = 0; // clear ep capture square
                    Pawns[EnemyColorIndex].RemovePieceAtSquare(epPawnSquare);
                    break;
                case Move.Flag.Castling:
                    bool kingside = moveTo == 6 || moveTo == 62;
                    int castlingRookFromIndex = kingside ? moveTo + 1 : moveTo - 2;
                    int castlingRookToIndex = kingside ? moveTo - 1 : moveTo + 1;

                    Squares[castlingRookFromIndex] = Piece.None;
                    Squares[castlingRookToIndex] = Piece.Rook | ColorToMove;

                    Rooks[ColorToMoveIndex].MovePiece(castlingRookFromIndex, castlingRookToIndex);
                    break;
            }
        }

        Squares[moveTo] = pieceOnTargetSquare;
        Squares[moveFrom] = 0;

        // Pawn has moved two forwards, mark file with en-passant flag
        if (moveFlag == Move.Flag.PawnTwoForward) {
            int file = new Coord(moveFrom).File + 1;
            CurrentGameState |= (ushort)(file << 4);
        }

        // Piece moving to/from rook square removes castling right for that side
        if (originalCastleState != 0) {
            if (moveTo == 7 || moveFrom == 7) {
                newCastleState &= WhiteCastleKingsideMask;
            }
            else if (moveTo == 0 || moveFrom == 0) {
                newCastleState &= WhiteCastleQueensideMask;
            }
            if (moveTo == 63 || moveFrom == 63) {
                newCastleState &= BlackCastleKingsideMask;
            }
            else if (moveTo == 56 || moveFrom == 56) {
                newCastleState &= BlackCastleQueensideMask;
            }
        }

        CurrentGameState |= newCastleState;
        CurrentGameState |= (uint)FiftyMoveCounter << 14;
        GameStateHistory.Push(CurrentGameState);

        // Change side to move
        WhiteToMove = !WhiteToMove;
        ColorToMove = (WhiteToMove) ? Piece.White : Piece.Black;
        EnemyColor = (WhiteToMove) ? Piece.Black : Piece.White;
        ColorToMoveIndex = 1 - ColorToMoveIndex;
        EnemyColorIndex = 1 - EnemyColorIndex;
        PlyCount++;
        FiftyMoveCounter++;

        if (capturedPieceType != 0 || isEnPassant || movePieceType == Piece.Pawn) {
            FiftyMoveCounter = 0;
        }

        CalculateBlockers();
    }

    public void UnmakeMove(Move move) {
        int opponentColourIndex = ColorToMoveIndex;
        bool undoingWhiteMove = EnemyColor == Piece.White;
        ColorToMove = EnemyColor; // side who made the move we are undoing
        EnemyColor = (undoingWhiteMove) ? Piece.Black : Piece.White;
        ColorToMoveIndex = 1 - ColorToMoveIndex;
        EnemyColorIndex = 1 - ColorToMoveIndex;
        WhiteToMove = !WhiteToMove;

        int capturedPieceType = ((int)CurrentGameState >> 8) & 63;
        int capturedPiece = (capturedPieceType == 0) ? 0 : capturedPieceType | EnemyColor;

        int movedFrom = move.StartSquare;
        int movedTo = move.TargetSquare;
        int moveFlags = move.MoveFlag;
        bool isEnPassant = moveFlags == Move.Flag.EnPassantCapture;
        bool isPromotion = move.IsPromotion;

        int toSquarePieceType = Piece.PieceType(Squares[movedTo]);
        int movedPieceType = (isPromotion) ? Piece.Pawn : toSquarePieceType;

        // ignore ep captures, handled later
        if (capturedPieceType != 0 && !isEnPassant) {
            GetPieceList(capturedPieceType, opponentColourIndex).AddPieceAtSquare(movedTo);
        }

        // Update king index
        if (movedPieceType == Piece.King) {
            Kings[ColorToMoveIndex].MovePiece(movedTo, movedFrom);
        }
        else if (!isPromotion) {
            GetPieceList(movedPieceType, ColorToMoveIndex).MovePiece(movedTo, movedFrom);
        }

        // put back moved piece
        Squares[movedFrom] = movedPieceType | ColorToMove; // note that if move was a pawn promotion, this will put the promoted piece back instead of the pawn. Handled in special move switch
        Squares[movedTo] = capturedPiece; // will be 0 if no piece was captured

        if (isPromotion) {
            Pawns[ColorToMoveIndex].AddPieceAtSquare(movedFrom);
            switch (moveFlags) {
                case Move.Flag.PromoteToQueen:
                    Queens[ColorToMoveIndex].RemovePieceAtSquare(movedTo);
                    break;
                case Move.Flag.PromoteToKnight:
                    Knights[ColorToMoveIndex].RemovePieceAtSquare(movedTo);
                    break;
                case Move.Flag.PromoteToRook:
                    Rooks[ColorToMoveIndex].RemovePieceAtSquare(movedTo);
                    break;
                case Move.Flag.PromoteToBishop:
                    Bishops[ColorToMoveIndex].RemovePieceAtSquare(movedTo);
                    break;
            }
        }
        else if (isEnPassant) { // ep cature: put captured pawn back on right square
            int epIndex = movedTo + ((ColorToMove == Piece.White) ? -8 : 8);
            Squares[movedTo] = 0;
            Squares[epIndex] = capturedPiece;
            Pawns[opponentColourIndex].AddPieceAtSquare(epIndex);
        }
        else if (moveFlags == Move.Flag.Castling) { // castles: move rook back to starting square
            bool kingside = movedTo == 6 || movedTo == 62;
            int castlingRookFromIndex = (kingside) ? movedTo + 1 : movedTo - 2;
            int castlingRookToIndex = (kingside) ? movedTo - 1 : movedTo + 1;

            Squares[castlingRookToIndex] = 0;
            Squares[castlingRookFromIndex] = Piece.Rook | ColorToMove;

            Rooks[ColorToMoveIndex].MovePiece(castlingRookToIndex, castlingRookFromIndex);
        }

        GameStateHistory.Pop(); // removes current state from history
        CurrentGameState = GameStateHistory.Peek(); // sets current state to previous state in history

        FiftyMoveCounter = (int)(CurrentGameState & 4294950912) >> 14;
        PlyCount--;

        CalculateBlockers();
    }

    public string GamePGN {
        get {
            return string.Join(" ", MovesPlayed);
        }
    }
}

