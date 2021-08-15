using System.Collections.Generic;

public enum PlayerType { Human, AI };

public abstract class Player {

    public event System.Action<Move, bool> OnMoveChosen;
    public event System.Action OnCheckmate;
    public event System.Action OnStalemate;

    protected int color;
    protected PlayerType type;
    protected Board board;
    protected MoveGenerator moveGen;
    protected List<Move> legalMoves;
    protected bool inCheck;

    public Player(int _color, PlayerType _type, Board _board) {
        color = _color;
        type = _type;
        board = _board;
        moveGen = new MoveGenerator();
        legalMoves = new List<Move>();
        inCheck = moveGen.MovingColorInCheck;
    }

    public abstract void Update();

    public void NotifyTurnToMove() {
        legalMoves = moveGen.GenerateLegalMoves(board);
        inCheck = moveGen.MovingColorInCheck;
        if (legalMoves.Count == 0) {
            if(inCheck) {
                OnCheckmate?.Invoke();
            }
            else {
                OnStalemate?.Invoke();
            }
        }
    }

    protected void ChooseMove(Move move, bool animate) {
        OnMoveChosen?.Invoke(move, animate);
    }
}