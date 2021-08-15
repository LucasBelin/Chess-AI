using System.Collections.Generic;

public class AIPlayer : Player {

    bool inSearch;

    public AIPlayer(int _color, PlayerType _type, Board _board) : base(_color, _type, _board) {
        inSearch = false;
    }

    void ChooseRandomMove() {
        inSearch = true;

        Move selectedMove = legalMoves[UnityEngine.Random.Range(0, legalMoves.Count)];
        inSearch = false;
        ChooseMove(selectedMove, true);
    }

    public override void Update() {
        if(!inSearch) {
            ChooseRandomMove();
        }
    }
}
