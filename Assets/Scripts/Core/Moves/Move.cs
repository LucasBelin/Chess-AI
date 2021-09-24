public struct Move {

	public struct Flag {
		public const int None = 0;
		public const int EnPassantCapture = 1;
		public const int Castling = 2;
		public const int PromoteToQueen = 3;
		public const int PromoteToKnight = 4;
		public const int PromoteToRook = 5;
		public const int PromoteToBishop = 6;
		public const int PawnTwoForward = 7;
	}

	readonly int moveValue;

	const int startSquareMask = 0b00000000000000111111;
	const int targetSquareMask = 0b00000000111111000000;
	const int flagMask = 0b00001111000000000000;
	const int pieceTypeMask = 0b01110000000000000000;
	const int captureMask = 0b10000000000000000000;

	public Move(ushort value) {
		moveValue = value;
    }

	public Move(int startSquare, int targetSquare, int pieceType) {
		moveValue = startSquare | targetSquare << 6 | pieceType << 16;
	}

	public Move(int startSquare, int targetSquare, int pieceType, bool isCapture, int flag = 0) {
		int captureValue = isCapture ? 1 << 19 : 0;
		moveValue = startSquare | targetSquare << 6 | flag << 12 | pieceType << 16 | captureValue;
	}

	public int StartSquare {
		get {
			return moveValue & startSquareMask;
		}
	}

	public int TargetSquare {
		get {
			return (moveValue & targetSquareMask) >> 6;
		}
	}

	public bool IsPromotion {
		get {
			int flag = MoveFlag;
			return flag == Flag.PromoteToQueen || flag == Flag.PromoteToRook || flag == Flag.PromoteToKnight || flag == Flag.PromoteToBishop;
		}
	}

	public int MoveFlag {
		get {
			return (moveValue & flagMask) >> 12;
		}
	}

	public int PromotionPieceType {
		get {
            return MoveFlag switch {
                Flag.PromoteToRook => Piece.Rook,
                Flag.PromoteToKnight => Piece.Knight,
                Flag.PromoteToBishop => Piece.Bishop,
                Flag.PromoteToQueen => Piece.Queen,
                _ => Piece.None,
            };
        }
	}

	public int Value {
		get {
			return moveValue;
		}
	}

	public int PieceType {
		get {
			return (moveValue & pieceTypeMask) >> 16;
        }
    }

	public bool IsCapture {
		get {
			return ((moveValue & captureMask) >> 19) != 0;
        }
    }

	public string Notation {
		get {
			if(MoveFlag == Flag.Castling) {
				if (StartSquare < TargetSquare) return "O-O";
				else return "O-O-O";
			}
			if(IsCapture) {
				if(PieceType == Piece.Pawn) {
					return Coord.FileNames[StartSquare % 8] + "x" + Coord.SquareNameFromIndex(TargetSquare);
				}
				return SymbolFromPieceType(PieceType) + "x" + Coord.SquareNameFromIndex(TargetSquare);
            }
			return SymbolFromPieceType(PieceType) + Coord.SquareNameFromIndex(TargetSquare);
		}
    }

	private static string SymbolFromPieceType(int pieceType) {
		return pieceType switch {
			Piece.King => "K",
			Piece.Knight => "N",
			Piece.Bishop => "B",
			Piece.Rook => "R",
			Piece.Queen => "Q",
			_ => ""
		};
    }
}