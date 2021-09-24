using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour {

    public string StartingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public TMPro.TextMeshProUGUI GameStateText;
    public TMPro.TextMeshProUGUI CurrentFenText;

    public PlayerType WhitePlayerType;
    public PlayerType BlackPlayerType;
    Player whitePlayer;
    Player blackPlayer;
    Player playerToMove;

    public Board board;
    [SerializeField] BoardUI boardUI;

    [HideInInspector] public int GameSeed;
    public enum GameState { Playing, Checkmate, Stalemate, Draw, Pause }
    GameState gameState;

    void Start() {
        NewGame(WhitePlayerType, BlackPlayerType);
        boardUI.OnAnimateStart += PauseGame;
        boardUI.OnAnimateEnd += ResumeGame;
    }

    void Update() {
        if (gameState == GameState.Playing)
            playerToMove.Update();
    }

    void PauseGame() {
        gameState = GameState.Pause;
    }

    void ResumeGame(Move move) {
        if(gameState == GameState.Pause) 
            gameState = GameState.Playing;
    }

    public void NewGame(PlayerType _whitePlayerType, PlayerType _blackPlayerType) {
        gameState = GameState.Pause;
        GameSeed = Random.Range(-999999, 999999);

        WhitePlayerType = _whitePlayerType;
        BlackPlayerType = _blackPlayerType;

        GameStateText.text = "";

        board = new Board();
        board.LoadPosition(StartingPosition);
        CurrentFenText.text = StartingPosition;

        whitePlayer = CreatePlayer(Piece.White, WhitePlayerType);
        blackPlayer = CreatePlayer(Piece.Black, BlackPlayerType);

        whitePlayer.OnMoveChosen += OnMoveChosen;
        blackPlayer.OnMoveChosen += OnMoveChosen;

        whitePlayer.OnCheckmate += DeclareCheckmate;
        whitePlayer.OnStalemate += DeclareStalemate;
        blackPlayer.OnCheckmate += DeclareCheckmate;
        blackPlayer.OnStalemate += DeclareStalemate;

        boardUI.ResetSquareColors();
        boardUI.LoadPieceSprites(board);
        playerToMove = board.WhiteToMove ? whitePlayer : blackPlayer;
        playerToMove.NotifyTurnToMove();

        gameState = GameState.Playing;
    }

    void DeclareCheckmate() {
        gameState = GameState.Checkmate;
        string winningColor = board.WhiteToMove ? "black" : "white";
        GameStateText.text = gameState + "\n" + winningColor + " wins";
    }

    void DeclareStalemate() {
        gameState = GameState.Stalemate;
        string winningColor = board.WhiteToMove ? "black" : "white";
        GameStateText.text = gameState + "\n" + winningColor + " wins";
    }

    void DeclareDraw(string reason) {
        gameState = GameState.Draw;
        GameStateText.text = gameState + "\n" + reason;
    }

    Player CreatePlayer(int color, PlayerType type) {
        if (type == PlayerType.Human) {
            return new HumanPlayer(color, type, board);
        }
        else {
            return new AIPlayer(color, type, board);
        }
    }

    public void HumanvsHuman() {
        NewGame(PlayerType.Human, PlayerType.Human);
    }

    public void HumanvsAI() {
        NewGame(PlayerType.Human, PlayerType.AI);
    }

    public void AIvsAI() {
        NewGame(PlayerType.AI, PlayerType.AI);
    }

    void OnMoveChosen(Move move, bool animate) {
        board.MakeMove(move);
        boardUI.OnMoveMade(board, move, animate);
        UpdateCurrentFen();

        if (board.AllPieceLists.InsufficientMaterial()) {
            DeclareDraw("Insufficient material");
        }
        else if(board.FiftyMoveCounter >= 50) {
            DeclareDraw("Fifty-move rule");
        }

        playerToMove = playerToMove == whitePlayer ? blackPlayer : whitePlayer;
        playerToMove.NotifyTurnToMove();
    }

    public void UpdateCurrentFen() {
        CurrentFenText.text = FenUtility.FenFromBoard(board);
    }

    /*public void UnmakeMove() {
        if (movesPlayed.Count == 0) return;
        if (gameState != GameState.Playing) gameState = GameState.Playing;

        Move moveToUnmake = movesPlayed[movesPlayed.Count - 1];
        board.UnmakeMove(moveToUnmake);
        boardUI.ResetSquareColors();
        boardUI.LoadPieceSprites(board);

        if (movesPlayed.Count > 1) boardUI.HighlightMove(movesPlayed[movesPlayed.Count - 2]);

        movesPlayed.RemoveAt(movesPlayed.Count - 1);
        playerToMove = playerToMove == whitePlayer ? blackPlayer : whitePlayer;
        playerToMove.NotifyTurnToMove();
    }*/
}
