using static Utils;
using System.Collections.Generic;

public class Evaluation
{
    const int pawnValue = 10;
    const int knightValue = 30;
    const int bishopValue = 30;
    const int rookValue = 50;
    const int queenValue = 90;

    const float trueEndgameMaterialValue = 250;
    const float pieceStructureWeight = 0.3f;

    const int negativeInfinity = -999999;

    Board board;
    MoveGenerator moveGen;
    public List<int> Evaluations;

    public Evaluation(Board _board) {
        board = _board;
        moveGen = new MoveGenerator();
        Evaluations = new List<int>();
    }

    int Evaluate(ulong enemyAttackMap) {
        int whiteEval = CountMaterial(WhiteIndex);
        int blackEval = CountMaterial(BlackIndex);

        float endgameCoeff = CalculateEndgameCoeff(whiteEval, blackEval);

        whiteEval += (int)(pieceStructureWeight * EvaluatePieceStructure(WhiteIndex, endgameCoeff, false, enemyAttackMap));
        blackEval += (int)(pieceStructureWeight * EvaluatePieceStructure(BlackIndex, endgameCoeff, true, enemyAttackMap));

        int evaluation = whiteEval - blackEval;
        return evaluation;
    }

    public void Search(int depth, int alpha, int beta) {
        List<Move> moves = moveGen.GenerateLegalMoves(board);
        if(moves.Count == 0) {
            return;
        }

        for (int i = 0; i < moves.Count; i++) {
            board.MakeMove(moves[i]);
            if(depth >= 0)
                Evaluations.Add(Minimax(depth - 1, -beta, -alpha));
            board.UnmakeMove(moves[i]);
        }
    }

    int Minimax(int depth, int alpha, int beta) {
        List<Move> moves = moveGen.GenerateLegalMoves(board);
        if (depth == 0) {
            return Evaluate(moveGen.enemyAttacks);
        }

        if (moves.Count == 0) {
            if (moveGen.MovingColorInCheck) {
                return negativeInfinity;
            }
            return 0;
        }

        for (int i = 0; i < moves.Count; i++) {
            board.MakeMove(moves[i]);
            int evaluation = -Minimax(depth - 1, -beta, -alpha);
            board.UnmakeMove(moves[i]);
            if (evaluation >= beta) {
                return beta;
            }
            alpha = System.Math.Max(alpha, evaluation);
        }

        return alpha;
    }

    float CalculateEndgameCoeff(int whiteMaterialCount, int blackMaterialCount) {
        return System.Math.Min(1, trueEndgameMaterialValue / (whiteMaterialCount + blackMaterialCount));
    }

    int EvaluatePieceStructure(int colorIndex, float endgameCoeff, bool isBlack, ulong enemyAttackMap) {
        int eval = 0;

        PieceList pawns = board.Pawns[colorIndex];
        for (int i = 0; i < pawns.Count; i++) {
            int pos = pawns[i];
            eval += PositionEvaluator.EvaluatePiecePosition(Piece.Pawn, pos, endgameCoeff, isBlack, enemyAttackMap);
        }

        PieceList knights = board.Knights[colorIndex];
        for (int i = 0; i < knights.Count; i++) {
            int pos = knights[i];
            eval += PositionEvaluator.EvaluatePiecePosition(Piece.Knight, pos, endgameCoeff, isBlack, enemyAttackMap);
        }

        PieceList bishops = board.Bishops[colorIndex];
        for (int i = 0; i < bishops.Count; i++) {
            int pos = bishops[i];
            eval += PositionEvaluator.EvaluatePiecePosition(Piece.Bishop, pos, endgameCoeff, isBlack, enemyAttackMap);
        }

        PieceList rooks = board.Rooks[colorIndex];
        for (int i = 0; i < rooks.Count; i++) {
            int pos = rooks[i];
            eval += PositionEvaluator.EvaluatePiecePosition(Piece.Rook, pos, endgameCoeff, isBlack, enemyAttackMap);
        }

        PieceList queens = board.Queens[colorIndex];
        for (int i = 0; i < queens.Count; i++) {
            int pos = queens[i];
            eval += PositionEvaluator.EvaluatePiecePosition(Piece.Queen, pos, endgameCoeff, isBlack, enemyAttackMap);
        }

        return eval;
    }

    int CountMaterial(int colorIndex) {
        int material = 0;
        material += board.Pawns[colorIndex].Count * pawnValue;
        material += board.Knights[colorIndex].Count * knightValue;
        material += board.Bishops[colorIndex].Count * bishopValue;
        material += board.Rooks[colorIndex].Count * rookValue;
        material += board.Queens[colorIndex].Count * queenValue;
        return material;
    }
}
