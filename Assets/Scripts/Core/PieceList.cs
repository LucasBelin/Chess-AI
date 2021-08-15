public class PieceList {

	public int[] OccupiedSquares;
	int[] map;
	int numPieces;

	public PieceList(int maxPieceCount = 16) {
		OccupiedSquares = new int[maxPieceCount];
		map = new int[64];
		numPieces = 0;
	}

	public int Count {
		get {
			return numPieces;
		}
	}

	public void AddPieceAtSquare(int square) {
		OccupiedSquares[numPieces] = square;
		map[square] = numPieces;
		numPieces++;
	}

	public void RemovePieceAtSquare(int square) {
		int pieceIndex = map[square];
		OccupiedSquares[pieceIndex] = OccupiedSquares[numPieces - 1];
		map[OccupiedSquares[pieceIndex]] = pieceIndex;
		numPieces--;
	}

	public void MovePiece(int startSquare, int targetSquare) {
		int pieceIndex = map[startSquare];
		OccupiedSquares[pieceIndex] = targetSquare;
		map[targetSquare] = pieceIndex;
	}

	public int this[int index] => OccupiedSquares[index];
}