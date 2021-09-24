using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardUI : MonoBehaviour {

	public event System.Action OnAnimateStart;
	public event System.Action<Move> OnAnimateEnd;

	[SerializeField] Toggle showLegalsToggle;
    [SerializeField] BoardTheme boardTheme;
	[SerializeField] PieceTheme pieceTheme;
	[SerializeField] GameObject legalHintUnoccupied;
	[SerializeField] GameObject legalHintOccupied;

    MeshRenderer[,] squareRenderers;
    SpriteRenderer[,] squarePieceRenderers;

    const float pieceDepth = -0.1f;
    const float pieceDragDepth = -0.2f;

	GameMaster manager;
	bool showLegalMoves;
	List<GameObject> legalGOs;

    private void Awake() {
        CreateBoardUI();
		legalGOs = new List<GameObject>();
		showLegalsToggle.onValueChanged.AddListener(UpdateShowLegals);
		showLegalMoves = showLegalsToggle.isOn;
		manager = FindObjectOfType<GameMaster>();
	}

	private void CreateBoardUI() {
		Shader squareShader = Shader.Find("Unlit/Color");
		squareRenderers = new MeshRenderer[8, 8];
		squarePieceRenderers = new SpriteRenderer[8, 8];

		for (int rank = 0; rank < 8; rank++) {
			for (int file = 0; file < 8; file++) {
				// Create square
				Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
				square.SetParent(transform);
				square.name = rank * 8 + file + "";
				square.position = PositionFromCoord(file, rank);

				Material squareMaterial = new Material(squareShader);
				squareRenderers[file, rank] = square.gameObject.GetComponent<MeshRenderer>();
				squareRenderers[file, rank].material = squareMaterial;
				squareRenderers[file, rank].material.color = boardTheme.SquareColor(new Coord(file, rank), ColorType.NORMAL);

				// Create piece sprite renderer for current square
				SpriteRenderer pieceRenderer = new GameObject("Piece").AddComponent<SpriteRenderer>();
				pieceRenderer.transform.SetParent(square);
				pieceRenderer.transform.position = PositionFromCoord(file, rank, pieceDepth);
				pieceRenderer.transform.localScale = Vector3.one * 100 / (2000 / 13f);
				squarePieceRenderers[file, rank] = pieceRenderer;
			}
		}
	}

	void UpdateShowLegals(bool isOn) {
		showLegalMoves = isOn;
		if(!isOn) HideLegals();
    }

	public void OnMoveMade(Board board, Move move, bool animate) {
		if (animate) {
			HideLegals();
			StartCoroutine(AnimateMove(move, board));
		}
		else {
			LoadPieceSprites(board);
			HighlightMove(move);
			HideLegals();
        }
	}

	IEnumerator AnimateMove(Move move, Board board) {
		OnAnimateStart?.Invoke();
		int seed = manager.GameSeed;
		float t = 0;
		const float moveAnimDuration = 0.15f;
		Coord startCoord = new Coord(move.StartSquare);
		Coord targetCoord = new Coord(move.TargetSquare);
		Transform pieceT = squarePieceRenderers[startCoord.File, startCoord.Rank].transform;
		Vector3 startPos = PositionFromCoord(startCoord.File, startCoord.Rank);
		Vector3 targetPos = PositionFromCoord(targetCoord.File, targetCoord.Rank);

		while (t <= 1) {
			yield return null;
			t += Time.deltaTime * 1 / moveAnimDuration;
			pieceT.position = Vector3.Lerp(startPos, targetPos, t);
		}
		pieceT.position = startPos;

		//Prevents loading the wrong board when a new game starts in the middle of the animation
		if(seed == manager.GameSeed) {
			LoadPieceSprites(board);
			HighlightMove(move);
			OnAnimateEnd?.Invoke(move);
		}
	}

	private Vector3 PositionFromCoord(int file, int rank, float depth = 0) {
		return new Vector3(-3.5f + file, -3.5f + rank, depth);
	}

	public void LoadPieceSprites(Board board) {
		for (int rank = 0; rank < 8; rank++) {
			for (int file = 0; file < 8; file++) {
				int piece = board.Squares[Coord.SquareIndexFromCoordinate(file, rank)];
				squarePieceRenderers[file, rank].sprite = pieceTheme.GetPieceSprite(piece);
				ResetPiecePosition(new Coord(file, rank));
			}
		}
	}

	public void ResetSquareColors() {
        for (int rank = 0; rank < 8; rank++) {
            for (int file = 0; file < 8; file++) {
				squareRenderers[file, rank].material.color = boardTheme.SquareColor(new Coord(file, rank), ColorType.NORMAL);
            }
        }
    }

	public void SelectSquare(Coord square) {
		squareRenderers[square.File, square.Rank].material.color = boardTheme.SquareColor(square, ColorType.SELECTED);
    }

	public void DeselectSquare(Coord square) {
		squareRenderers[square.File, square.Rank].material.color = boardTheme.SquareColor(square, ColorType.NORMAL);
	}

	public void ShowLegals(List<Move> moves) {
		if (!showLegalMoves || moves.Count == 0) return;

		HashSet<int> targets = new HashSet<int>();
        for (int i = 0; i < moves.Count; i++) {
			targets.Add(moves[i].TargetSquare);
		}

		foreach(int target in targets) {
			Coord targetCoord = new Coord(target);
			Vector3 pos = PositionFromCoord(targetCoord.File, targetCoord.Rank);
			GameObject prefab = squarePieceRenderers[targetCoord.File, targetCoord.Rank].sprite == null ? legalHintUnoccupied : legalHintOccupied;
			GameObject hint = Instantiate(prefab, pos, Quaternion.identity);
			hint.GetComponent<SpriteRenderer>().color = boardTheme.SquareColor(targetCoord, ColorType.LEGAL);
			legalGOs.Add(hint);
		}
	}

	public void HideLegals() {
		if (!showLegalMoves && legalGOs.Count == 0) return;
		foreach(GameObject hint in legalGOs) {
			Destroy(hint);
        }
		legalGOs = new List<GameObject>();
    }

	public void DragPiece(Coord piece, Vector2 mousePos) {
		squarePieceRenderers[piece.File, piece.Rank].transform.position = new Vector3(mousePos.x, mousePos.y, pieceDragDepth);
	}

	public void ResetPiecePosition(Coord square) {
		Vector3 pos = PositionFromCoord(square.File, square.Rank, pieceDepth);
		squarePieceRenderers[square.File, square.Rank].transform.position = pos;
	}

	public void HighlightMove(Move move) {
		ResetSquareColors();
		SelectSquare(new Coord(move.StartSquare));
		SelectSquare(new Coord(move.TargetSquare));
	}
}