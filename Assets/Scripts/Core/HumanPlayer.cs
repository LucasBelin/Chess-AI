using System.Collections.Generic;
using UnityEngine;

class HumanPlayer : Player {

    public enum InputState { None, PieceSelected, DraggingPiece }

    InputState currentState;
    int selectedSquare;
    Coord selectedSquareCoord;
    BoardUI boardUI;

    public HumanPlayer(int _color, PlayerType _type, Board _board) : base(_color, _type, _board) {
        boardUI = GameObject.FindObjectOfType<BoardUI>();
        moveGen = new MoveGenerator();
    }

    public override void Update() {
        HandleInput();
    }

    public void HandleInput() {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (currentState == InputState.None) {
            HandlePieceSelection(mousePos);
        }
        else if(currentState == InputState.PieceSelected) {
            HandlePieceMovement(mousePos);
        }
        else if(currentState == InputState.DraggingPiece) {
            HandlePieceDrag(mousePos);
        }

        if (Input.GetMouseButtonDown(1)) {
            CancelPieceSelection();
        }
    }

    void HandlePieceSelection(Vector2 mousePos) {
        if(Input.GetMouseButtonDown(0)) {
            Coord clickedSquare = Coord.SquareCoordinateFromMousePos(mousePos);
            if(clickedSquare.InBounds) {
                if (!Piece.IsColor(board.Squares[clickedSquare.SquareIndex], color)) return;
                selectedSquareCoord = clickedSquare;
                selectedSquare = clickedSquare.SquareIndex;
                boardUI.SelectSquare(clickedSquare);
                List<Move> legals = GetLegalMovesForSquare(clickedSquare.SquareIndex);
                boardUI.ShowLegals(legals);
                currentState = InputState.DraggingPiece;
            }
        }
    }

    void HandlePieceMovement(Vector2 mousePos) {
        if (Input.GetMouseButtonDown(0)) {
            Coord clickedSquare = Coord.SquareCoordinateFromMousePos(mousePos);
            if (!clickedSquare.InBounds) return;

            if(clickedSquare.CompareTo(selectedSquareCoord) == 0) {
                currentState = InputState.DraggingPiece;
            }
            else {
                HandlePiecePlacement(mousePos);
            }
        }
    }

    void HandlePieceDrag(Vector2 mousePos) {
        boardUI.DragPiece(selectedSquareCoord, mousePos);
        if (Input.GetMouseButtonUp(0)) {
            HandlePiecePlacement(mousePos);
        }
    }

    void HandlePiecePlacement(Vector2 mousePos) {
        Coord targetSquareCoord = Coord.SquareCoordinateFromMousePos(mousePos);
        if (targetSquareCoord.InBounds) {
            if (targetSquareCoord.SquareIndex == selectedSquare) {
                boardUI.ResetPiecePosition(selectedSquareCoord);
                if (currentState == InputState.DraggingPiece) {
                    currentState = InputState.PieceSelected;
                }
            }
            else {
                if (TryGetLegalMove(targetSquareCoord.SquareIndex, out Move move)) {
                    bool animate = currentState != InputState.DraggingPiece;
                    currentState = InputState.None;
                    ChooseMove(move, animate);
                }
                else {
                    currentState = InputState.PieceSelected;
                    boardUI.ResetPiecePosition(selectedSquareCoord);
                    if (Piece.IsColor(board.Squares[targetSquareCoord.SquareIndex], color)) {
                        boardUI.HideLegals();
                        boardUI.DeselectSquare(selectedSquareCoord);
                        boardUI.SelectSquare(targetSquareCoord);
                        boardUI.ShowLegals(GetLegalMovesForSquare(targetSquareCoord.SquareIndex));
                        selectedSquareCoord = targetSquareCoord;
                        selectedSquare = targetSquareCoord.SquareIndex;
                        currentState = InputState.DraggingPiece;
                    }
                }
            }
        }
        else {
            boardUI.ResetPiecePosition(selectedSquareCoord);
            currentState = InputState.PieceSelected;
        }
    }

    void CancelPieceSelection() {
        if (selectedSquareCoord == null) return;
        boardUI.ResetPiecePosition(selectedSquareCoord);
        boardUI.DeselectSquare(selectedSquareCoord);
        boardUI.HideLegals();
        currentState = InputState.None;
    }

    bool TryGetLegalMove(int targetSquare, out Move move) {
        foreach(Move legal in legalMoves) {
            if(legal.StartSquare == selectedSquare && legal.TargetSquare == targetSquare) {
                move = legal;
                return true;
            }
        }
        move = new Move(0);
        return false;
    }

    List<Move> GetLegalMovesForSquare(int square) {
        List<Move> legals = new List<Move>();
        for (int i = 0; i < legalMoves.Count; i++) {
            if (legalMoves[i].StartSquare == square)
                legals.Add(legalMoves[i]);
        }

        return legals;
    }
}