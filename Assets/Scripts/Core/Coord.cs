using UnityEngine;

public class Coord {

    public static readonly string FileNames = "abcdefgh";
    public static readonly string RankNames = "12345678";

    public int File;
    public int Rank;

    public Coord(int _file, int _rank) {
        File = _file;
        Rank = _rank;
    }

    public Coord(int _squareIndex) {
        File = _squareIndex % 8;
        Rank = _squareIndex / 8;
    }

    public bool InBounds {
        get {
            return File >= 0 && File <= 7 && Rank >= 0 && Rank <= 7;
        }
    }

    public int SquareIndex {
        get {
            return Rank * 8 + File;
        }
    }

    public bool IsLight {
        get {
            return (File + Rank) % 2 == 0;
        }
    }

    public int CompareTo(Coord other) {
        int same = 0;
        int different = 1;
        if (File == other.File && Rank == other.Rank) return same;
        return different;
    }

    public static string SquareNameFromCoordinate(int fileIndex, int rankIndex) {
        return FileNames[fileIndex] + "" + RankNames[rankIndex];
    }

    public static string SquareNameFromCoordinate(Coord coord) {
        return SquareNameFromCoordinate(coord.File, coord.Rank);
    }

    public static string SquareNameFromMousePos(Vector3 mousePos) {
        return SquareNameFromCoordinate(SquareCoordinateFromMousePos(mousePos));
    }

    public static Coord SquareCoordinateFromName(string name) {
        if (name.Length < 2) return new Coord(-1, -1);
        int fileIndex = FileNames.IndexOf(name[0]);
        int rankIndex = RankNames.IndexOf(name[1]);
        return new Coord(fileIndex, rankIndex);
    }

    public static int SquareIndexFromName(string name) {
        Coord coord = SquareCoordinateFromName(name);
        return coord.Rank * 8 + coord.File;
    }

    public static int SquareIndexFromCoordinate(Coord coord) {
        return SquareIndexFromCoordinate(coord.File, coord.Rank);
    }

    public static int SquareIndexFromCoordinate(int fileIndex, int rankIndex) {
        return SquareIndexFromName(SquareNameFromCoordinate(fileIndex, rankIndex));
    }

    public static Coord SquareCoordinateFromIndex(int index) {
        return new Coord(index % 8, index / 8);
    }

    public static Coord SquareCoordinateFromMousePos(Vector3 mousePos) {
        float boardStartCoord = -4f;
        float boardEndCoord = 3f;

        float fileIndex = Map(Mathf.FloorToInt(mousePos.x), boardStartCoord, boardEndCoord, 0, 7);
        float rankIndex = Map(Mathf.FloorToInt(mousePos.y), boardStartCoord, boardEndCoord, 0, 7);

        return new Coord((int)fileIndex, (int)rankIndex);
    }

    public static int SquareIndexFromMousePos(Vector3 mousePos) {
        return SquareIndexFromCoordinate(SquareCoordinateFromMousePos(mousePos));
    }

    private static float Map(float value, float startSource, float endSource, float startTarget, float endTarget) {
        return (value - startSource) / (endSource - startSource) * (endTarget - startTarget) + startTarget;
    }

    public override string ToString() {
        if (File < 0 || Rank < 0) return "-";
        return File + "" + Rank;
    }
}
