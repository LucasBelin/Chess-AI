using System.Collections.Generic;

public class AIPlayer : Player {

    const int negativeInfinity = -999999;
    const int positiveInfinity = 999999;

    bool inSearch;
    Book book;

    const int maxBookMoves = 10;
    int bookMovesMade;
    bool makeBookMove;

    public AIPlayer(int _color, PlayerType _type, Board _board) : base(_color, _type, _board) {
        inSearch = false;
        book = new Book();
        bookMovesMade = 0;
        makeBookMove = true;
    }

    void ChooseRandomMove() {
        inSearch = true;

        Move selectedMove = legalMoves[UnityEngine.Random.Range(0, legalMoves.Count)];
        inSearch = false;
        ChooseMove(selectedMove, true);
    }

    void ChooseBestMove() {
        if(makeBookMove && bookMovesMade < maxBookMoves) {
            ChooseBookMove();
        }
        else {
            EvaluateBestMove();
        }
    }

    void EvaluateBestMove() {
        Evaluation eval = new Evaluation(board);
        eval.Search(4, negativeInfinity, positiveInfinity);
        List<int> evaluations = eval.Evaluations;

        //Find index of best evaluation
        int index = 0;
        int maxEval = evaluations[0];
        for (int i = 1; i < evaluations.Count; i++) {
            if(evaluations[i] > maxEval) {
                maxEval = evaluations[i];
                index = i;
            }
        }
        ChooseMove(legalMoves[index], true);
    }

    void ChooseBookMove() {
        inSearch = true;
        Move move = book.GetNextMove(legalMoves, board.GamePGN);
        inSearch = false;

        if(move.Value == 0) {
            makeBookMove = false;
            EvaluateBestMove();
            return;
        }
        bookMovesMade++;
        ChooseMove(move, true);
    }

    public override void Update() {
        if(!inSearch) {
            ChooseBestMove();
        }
    }
}
