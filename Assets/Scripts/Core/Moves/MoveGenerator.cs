using System.Collections.Generic;
using static PrecomputedMoves;
using static BitBoardUtility;
using static Rays;

public class MoveGenerator {

    Board board;
    List<Move> moves;

    int friendlyIndex;
    int enemyIndex;
    int friendlyColor;
    int enemyColor;
    bool whiteToMove;

    public ulong enemyAttacks;
    ulong blockers;

    bool inCheck;
    bool inDoubleCheck;

    int checkerPos;
    ulong captureMask;
    ulong pushMask;
    ulong restrictedMoveMask;

    bool pinExists;
    ulong pinRayMask;
    ulong[] pinRays;

    int friendlyKingPos;
    ulong friendlyKingMask;

    public MoveGenerator() { }

    public List<Move> GenerateLegalMoves(Board _board) {
        Init(_board);
        GenerateKingMoves();

        if (inDoubleCheck) {
            return moves;
        }

        GeneratePawnMoves();
        GenerateKnightMoves();
        GenerateBishopMoves();
        GenerateRookMoves();
        GenerateQueenMoves();

        return moves;
    }

    void Init(Board _board) {
        board = _board;
        moves = new List<Move>();

        friendlyIndex = board.ColorToMoveIndex;
        enemyIndex = 1 - friendlyIndex;
        friendlyColor = board.ColorToMove;
        enemyColor = board.EnemyColor;
        whiteToMove = board.WhiteToMove;

        enemyAttacks = 0;
        blockers = board.Blockers;

        inCheck = false;
        inDoubleCheck = false;
        checkerPos = -1;

        captureMask = 0;
        pushMask = 0;
        restrictedMoveMask = 0xffffffffffffffff;

        pinExists = false;
        pinRayMask = 0;
        pinRays = new ulong[64];

        friendlyKingPos = board.Kings[friendlyIndex][0];
        friendlyKingMask = 1ul << friendlyKingPos;

        InitAttackData();
    }

    void InitAttackData() {
        PieceList enemyPawns = board.Pawns[enemyIndex];
        for (int i = 0; i < enemyPawns.Count; i++) {
            int pawnPos = enemyPawns[i];
            ulong pawnAttacks = PawnAttacksLookup[pawnPos][enemyIndex];
            enemyAttacks |= pawnAttacks;
            if ((pawnAttacks & friendlyKingMask) != 0) {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkerPos = pawnPos;
            }
        }

        PieceList enemyKnights = board.Knights[enemyIndex];
        for (int i = 0; i < enemyKnights.Count; i++) {
            int knightPos = enemyKnights[i];
            ulong knightAttacks = KnightAttacksLookup[knightPos];
            enemyAttacks |= knightAttacks;
            if ((knightAttacks & friendlyKingMask) != 0) {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkerPos = knightPos;
            }
        }

        PieceList enemyBishops = board.Bishops[enemyIndex];
        for (int i = 0; i < enemyBishops.Count; i++) {
            int bishopPos = enemyBishops[i];
            ulong bishopAttacks = GenerateBishopAttacks(bishopPos);
            enemyAttacks |= bishopAttacks;
            if ((bishopAttacks & friendlyKingMask) != 0) {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkerPos = bishopPos;
            }
        }

        PieceList enemyRooks = board.Rooks[enemyIndex];
        for (int i = 0; i < enemyRooks.Count; i++) {
            int rookPos = enemyRooks[i];
            ulong rookAttacks = GenerateRookAttacks(rookPos);
            enemyAttacks |= rookAttacks;
            if ((rookAttacks & friendlyKingMask) != 0) {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkerPos = rookPos;
            }
        }

        PieceList enemyQueens = board.Queens[enemyIndex];
        for (int i = 0; i < enemyQueens.Count; i++) {
            int queenPos = enemyQueens[i];
            ulong queenAttacks = GenerateRookAttacks(queenPos) | GenerateBishopAttacks(queenPos);
            enemyAttacks |= queenAttacks;
            if ((queenAttacks & friendlyKingMask) != 0) {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkerPos = queenPos;
            }
        }

        enemyAttacks |= KingAttacksLookup[board.Kings[enemyIndex][0]];

        if (inCheck) {
            captureMask = 1ul << checkerPos;
            if (Piece.IsSlidingPiece(board.Squares[checkerPos])) {
                pushMask = GeneratePushMask(checkerPos, friendlyKingPos);
            }
            restrictedMoveMask &= captureMask | pushMask;
        }

        if (!inDoubleCheck) {
            GeneratePinRayMask();
        }
    }

    void GeneratePinRayMask() {
        int startDirIndex = 0;
        int endDirIndex = 8;

        if (board.Queens[enemyIndex].Count == 0) {
            startDirIndex = (board.Rooks[enemyIndex].Count > 0) ? 0 : 4;
            endDirIndex = (board.Bishops[enemyIndex].Count > 0) ? 8 : 4;
        }

        for (int dir = startDirIndex; dir < endDirIndex; dir++) {
            bool isDiagonal = dir > 3;
            int directionOffset = DirectionOffsets[dir];
            int n = NumSquaresToEdge[friendlyKingPos][dir];
            bool friendlyPieceEncountered = false;
            ulong currentRayMask = 0;
            int friendlyPiecePos = -1;

            for (int i = 0; i < n; i++) {
                int target = friendlyKingPos + directionOffset * (i + 1);
                currentRayMask |= 1ul << target;
                int pieceOnTarget = board.Squares[target];

                if (pieceOnTarget != Piece.None) {
                    if (Piece.IsColor(pieceOnTarget, friendlyColor)) {
                        //First friendly piece on this ray
                        if (!friendlyPieceEncountered) {
                            friendlyPieceEncountered = true;
                            friendlyPiecePos = target;
                        }
                        //Second friendly piece on this ray, pin is not possible
                        else break;
                    }
                    else {
                        int enemyPieceType = Piece.PieceType(pieceOnTarget);
                        if (isDiagonal && Piece.IsBishopOrQueen(enemyPieceType) || !isDiagonal && Piece.IsRookOrQueen(enemyPieceType)) {
                            //This is a pin
                            if (friendlyPieceEncountered) {
                                pinExists = true;
                                pinRayMask |= currentRayMask;
                                pinRays[friendlyPiecePos] = currentRayMask;
                            }
                            break;
                        }
                        else break;
                    }
                }
            }
        }
    }

    ulong GenerateBishopAttacks(int bishopPos) {
        ulong attacks = 0ul;
        ulong blockingPieces = blockers & ~friendlyKingMask;

        attacks |= NORTH_WEST[bishopPos];
        ulong nw = NORTH_WEST[bishopPos] & blockingPieces;
        if (nw != 0) {
            int blockerIndex = BitscanForward(nw);
            attacks &= ~NORTH_WEST[blockerIndex];
        }

        attacks |= NORTH_EAST[bishopPos];
        ulong ne = NORTH_EAST[bishopPos] & blockingPieces;
        if (ne != 0) {
            int blockerIndex = BitscanForward(ne);
            attacks &= ~NORTH_EAST[blockerIndex];
        }

        attacks |= SOUTH_WEST[bishopPos];
        ulong sw = SOUTH_WEST[bishopPos] & blockingPieces;
        if (sw != 0) {
            int blockerIndex = BitscanReverse(sw);
            attacks &= ~SOUTH_WEST[blockerIndex];
        }

        attacks |= SOUTH_EAST[bishopPos];
        ulong se = SOUTH_EAST[bishopPos] & blockingPieces;
        if (se != 0) {
            int blockerIndex = BitscanReverse(se);
            attacks &= ~SOUTH_EAST[blockerIndex];
        }

        return attacks;
    }

    ulong GenerateRookAttacks(int rookPos) {
        ulong attacks = 0ul;
        ulong blockingPieces = blockers & ~friendlyKingMask;

        attacks |= NORTH[rookPos];
        ulong north = NORTH[rookPos] & blockingPieces;
        if (north != 0) {
            int blockerIndex = BitscanForward(north);
            attacks &= ~NORTH[blockerIndex];
        }

        attacks |= SOUTH[rookPos];
        ulong south = SOUTH[rookPos] & blockingPieces;
        if (south != 0) {
            int blockerIndex = BitscanReverse(south);
            attacks &= ~SOUTH[blockerIndex];
        }

        attacks |= WEST[rookPos];
        ulong west = WEST[rookPos] & blockingPieces;
        if (west != 0) {
            int blockerIndex = BitscanReverse(west);
            attacks &= ~WEST[blockerIndex];
        }

        attacks |= EAST[rookPos];
        ulong east = EAST[rookPos] & blockingPieces;
        if (east != 0) {
            int blockerIndex = BitscanForward(east);
            attacks &= ~EAST[blockerIndex];
        }

        return attacks;
    }

    ulong GeneratePushMask(int from, int to) {
        int fx = from % 8, fy = from / 8;
        int tx = to % 8, ty = to / 8;
        //North
        if (fx == tx && fy < ty) {
            return NORTH[from] & ~NORTH[to];
        }
        //South
        else if (fx == tx && fy > ty) {
            return SOUTH[from] & ~SOUTH[to];
        }
        //East
        else if (fx < tx && fy == ty) {
            return EAST[from] & ~EAST[to];
        }
        //West
        else if (fx > tx && fy == ty) {
            return WEST[from] & ~WEST[to];
        }
        //North east
        else if (fx < tx && fy < ty) {
            return NORTH_EAST[from] & ~NORTH_EAST[to];
        }
        //North west
        else if (fx > tx && fy < ty) {
            return NORTH_WEST[from] & ~NORTH_WEST[to];
        }
        //South east
        else if (fx < tx && fy > ty) {
            return SOUTH_EAST[from] & ~SOUTH_EAST[to];
        }
        //South west
        else if (fx > tx && fy > ty) {
            return SOUTH_WEST[from] & ~SOUTH_WEST[to];
        }
        return 0;
    }

    bool InCheckAfterEnPassant(int startSquare, int targetSquare, int epCapturedPawnSquare) {
        // Update board to reflect en-passant capture
        board.Squares[targetSquare] = board.Squares[startSquare];
        board.Squares[startSquare] = Piece.None;
        board.Squares[epCapturedPawnSquare] = Piece.None;

        bool inCheckAfterEpCapture = false;
        if (KingAttackedAfterEPCapture()) {
            inCheckAfterEpCapture = true;
        }

        // Undo change to board
        board.Squares[targetSquare] = Piece.None;
        board.Squares[startSquare] = Piece.Pawn | friendlyColor;
        board.Squares[epCapturedPawnSquare] = Piece.Pawn | enemyColor;

        return inCheckAfterEpCapture;
    }

    bool KingAttackedAfterEPCapture() {
        bool isAttacked = false;
        //Check north, south, east, west rays for an enemy rook or queen
        for (int dir = 0; dir < 4; dir++) {
            int n = NumSquaresToEdge[friendlyKingPos][dir];
            for (int i = 0; i < n; i++) {
                int targetSquare = friendlyKingPos + DirectionOffsets[dir] * (i + 1);
                int piece = board.Squares[targetSquare];
                if (piece != Piece.None) {
                    if (Piece.IsColor(piece, enemyColor)) {
                        if (Piece.IsRookOrQueen(Piece.PieceType(piece))) {
                            isAttacked = true;
                        }
                        else break;
                    }
                    else break;
                }
            }
        }

        return isAttacked;
    }

    void GenerateKingMoves() {
        int[] targets = KingMoves[friendlyKingPos];
        for (int i = 0; i < targets.Length; i++) {
            if (((enemyAttacks >> targets[i]) & 1) == 0) {
                int piece = board.Squares[targets[i]];
                if (piece != Piece.None) {
                    if (Piece.IsColor(piece, enemyColor)) {
                        moves.Add(new Move(friendlyKingPos, targets[i], Piece.King, true));
                    }
                }
                else {
                    moves.Add(new Move(friendlyKingPos, targets[i], Piece.King));
                }
            }
        }

        if (!inCheck) {
            if (HasKingsideCastleRight) {
                int[] castlingSquares = new int[] { friendlyKingPos + 1, friendlyKingPos + 2 };
                ulong travelSquaresMask = (1ul << castlingSquares[0]) | (1ul << castlingSquares[1]);
                if ((travelSquaresMask & enemyAttacks) == 0) {
                    if (board.Squares[castlingSquares[0]] == Piece.None &&
                        board.Squares[castlingSquares[1]] == Piece.None) {
                        moves.Add(new Move(friendlyKingPos, castlingSquares[1], Piece.King, false, Move.Flag.Castling));
                    }
                }
            }
            if (HasQueensideCastleRight) {
                int[] castlingSquares = new int[] { friendlyKingPos - 1, friendlyKingPos - 2, friendlyKingPos - 3 };
                ulong travelSquaresMask = (1ul << castlingSquares[0]) | (1ul << castlingSquares[1]);
                if ((travelSquaresMask & enemyAttacks) == 0) {
                    if (board.Squares[castlingSquares[0]] == Piece.None &&
                        board.Squares[castlingSquares[1]] == Piece.None &&
                        board.Squares[castlingSquares[2]] == Piece.None) {
                        moves.Add(new Move(friendlyKingPos, castlingSquares[1], Piece.King, false, Move.Flag.Castling));
                    }
                }
            }
        }
    }

    void GeneratePawnMoves() {
        PieceList pawns = board.Pawns[friendlyIndex];
        int forwardOffset = whiteToMove ? 8 : -8;
        int startRank = whiteToMove ? 1 : 6;
        int finalRankBeforePromotion = whiteToMove ? 6 : 1;

        int enPassantFile = ((int)(board.CurrentGameState >> 4) & 15) - 1;
        int enPassantSquare = -1;
        if (enPassantFile != -1) {
            enPassantSquare = 8 * (whiteToMove ? 5 : 2) + enPassantFile;
        }

        for (int i = 0; i < pawns.Count; i++) {
            int pawnPos = pawns[i];
            int rank = pawnPos / 8;
            bool oneStepFromPromotion = rank == finalRankBeforePromotion;
            bool isPinned = IsPinned(pawnPos);

            //Forward moves
            for (int j = 0; j < PawnForward[pawnPos][friendlyIndex].Length; j++) {
                int target = PawnForward[pawnPos][friendlyIndex][j];
                //Is pinned and is not moving along the pin ray
                if (isPinned) {
                    if (((pinRays[pawnPos] >> target) & 1) == 0)
                        break;
                }
                //1 forward
                if (board.Squares[target] == Piece.None) {
                    //Not in check or move blocks the check
                    if (!inCheck || ((restrictedMoveMask >> target) & 1) > 0) {
                        if (oneStepFromPromotion) GeneratePromotionMoves(pawnPos, target);
                        else moves.Add(new Move(pawnPos, target, Piece.Pawn));

                        //2 forward
                        if (rank == startRank) {
                            int twoForwardTarget = target + forwardOffset;
                            if (board.Squares[twoForwardTarget] == Piece.None) {
                                if (!inCheck || ((restrictedMoveMask >> twoForwardTarget) & 1) > 0) {
                                    moves.Add(new Move(pawnPos, twoForwardTarget, Piece.Pawn, false, Move.Flag.PawnTwoForward));
                                }
                            }
                        }
                    }
                    else if(inCheck) {
                        //2 forward
                        if (rank == startRank) {
                            int twoForwardTarget = target + forwardOffset;
                            if (board.Squares[twoForwardTarget] == Piece.None) {
                                if (!inCheck || ((restrictedMoveMask >> twoForwardTarget) & 1) > 0) {
                                    moves.Add(new Move(pawnPos, twoForwardTarget, Piece.Pawn, false, Move.Flag.PawnTwoForward));
                                }
                            }
                        }
                    }
                }
            }

            //Capture moves
            for (int j = 0; j < PawnCaptures[pawnPos][friendlyIndex].Length; j++) {
                int target = PawnCaptures[pawnPos][friendlyIndex][j];
                //Is pinned and is not moving along the pin ray
                if (isPinned) {
                    if (((pinRays[pawnPos] >> target) & 1) == 0)
                        continue;
                }

                int piece = board.Squares[target];
                if (piece != Piece.None) {
                    if (Piece.IsColor(piece, enemyColor)) {
                        //Not in check or move removes the check
                        if (!inCheck || ((restrictedMoveMask >> target) & 1) > 0) {
                            if (oneStepFromPromotion) GeneratePromotionMoves(pawnPos, target);
                            else moves.Add(new Move(pawnPos, target, Piece.Pawn, true));
                        }
                    }
                }
                //En passant
                else {
                    if (enPassantFile > -1) {
                        int epCapturedPawnSquare = target - forwardOffset;
                        ulong pawnRestrictedMoveMask = restrictedMoveMask;
                        if (checkerPos == epCapturedPawnSquare) pawnRestrictedMoveMask |= 1ul << enPassantSquare;

                        if (target == enPassantSquare) {
                            if (!inCheck || ((pawnRestrictedMoveMask >> target) & 1) > 0) {
                                if (!InCheckAfterEnPassant(pawnPos, target, epCapturedPawnSquare))
                                    moves.Add(new Move(pawnPos, target, Piece.Pawn, true, Move.Flag.EnPassantCapture));
                            }
                        }
                    }
                }
            }
        }
    }

    void GeneratePromotionMoves(int from, int to) {
        moves.Add(new Move(from, to, Piece.Pawn, false, Move.Flag.PromoteToQueen));
        moves.Add(new Move(from, to, Piece.Pawn, false, Move.Flag.PromoteToKnight));
        moves.Add(new Move(from, to, Piece.Pawn, false, Move.Flag.PromoteToBishop));
        moves.Add(new Move(from, to, Piece.Pawn, false, Move.Flag.PromoteToRook));
    }

    void GenerateKnightMoves() {
        PieceList knights = board.Knights[friendlyIndex];
        for (int i = 0; i < knights.Count; i++) {
            int knightPos = knights[i];
            if (IsPinned(knightPos)) {
                continue;
            }

            int[] targets = KnightMoves[knightPos];
            for (int j = 0; j < targets.Length; j++) {
                int piece = board.Squares[targets[j]];
                if (piece != Piece.None) {
                    if (Piece.IsColor(piece, enemyColor)) {
                        if (!inCheck || ((restrictedMoveMask >> targets[j]) & 1) != 0) {
                            moves.Add(new Move(knightPos, targets[j], Piece.Knight, true));
                        }
                    }
                }
                else {
                    if (!inCheck || ((restrictedMoveMask >> targets[j]) & 1) != 0) {
                        moves.Add(new Move(knightPos, targets[j], Piece.Knight));
                    }
                }
            }
        }
    }

    void GenerateBishopMoves() {
        PieceList bishops = board.Bishops[friendlyIndex];
        for (int i = 0; i < bishops.Count; i++) {
            int bishopPos = bishops[i];
            GenerateSlidingMoves(bishopPos, Piece.Bishop, 4, 8);
        }
    }

    void GenerateRookMoves() {
        PieceList rooks = board.Rooks[friendlyIndex];
        for (int i = 0; i < rooks.Count; i++) {
            int rookPos = rooks[i];
            GenerateSlidingMoves(rookPos, Piece.Rook, 0, 4);
        }
    }

    void GenerateQueenMoves() {
        PieceList queens = board.Queens[friendlyIndex];
        for (int i = 0; i < queens.Count; i++) {
            int queenPos = queens[i];
            GenerateSlidingMoves(queenPos, Piece.Queen, 0, 8);
        }
    }

    void GenerateSlidingMoves(int piecePos, int pieceType, int startDir, int endDir) {
        bool isPinned = IsPinned(piecePos);
        if (inCheck && isPinned) {
            return;
        }

        for (int dir = startDir; dir < endDir; dir++) {
            int n = NumSquaresToEdge[piecePos][dir];
            for (int j = 0; j < n; j++) {
                int target = piecePos + DirectionOffsets[dir] * (j + 1);
                int piece = board.Squares[target];

                if (isPinned) {
                    if (((pinRays[piecePos] >> target) & 1) == 0)
                        break;
                }
                
                if (piece != Piece.None) {
                    if (Piece.IsColor(piece, friendlyColor)) {
                        break;
                    }
                    else {
                        if (!inCheck || ((restrictedMoveMask >> target) & 1) != 0)
                            moves.Add(new Move(piecePos, target, pieceType, true));
                        break;
                    }
                }
                else {
                    if (!inCheck || ((restrictedMoveMask >> target) & 1) != 0) {
                        moves.Add(new Move(piecePos, target, pieceType));
                    }
                }
            }
        }
    }

    bool IsPinned(int square) {
        return pinExists && ((pinRayMask >> square) & 1) != 0;
    }

    public bool MovingColorInCheck {
        get {
            return inCheck;
        }
    }

    bool HasKingsideCastleRight {
        get {
            int mask = board.WhiteToMove ? 1 : 4;
            return (board.CurrentGameState & mask) != 0;
        }
    }

    bool HasQueensideCastleRight {
        get {
            int mask = board.WhiteToMove ? 2 : 8;
            return (board.CurrentGameState & mask) != 0;
        }
    }
}