using UnityEngine;

public enum ColorType { NORMAL, LEGAL, SELECTED }

[CreateAssetMenu(menuName = "Theme/Board")]
public class BoardTheme : ScriptableObject {

    public SquareColors LightSquares;
    public SquareColors DarkSquares;

    public Color SquareColor(Coord coord, ColorType type) {
        switch (coord.IsLight) {
            case true:
                return type switch {
                    ColorType.NORMAL => LightSquares.normal,
                    ColorType.LEGAL => LightSquares.legal,
                    ColorType.SELECTED => LightSquares.selected,
                    _ => LightSquares.normal,
                };
            case false:
                return type switch {
                    ColorType.NORMAL => DarkSquares.normal,
                    ColorType.LEGAL => DarkSquares.legal,
                    ColorType.SELECTED => DarkSquares.selected,
                    _ => DarkSquares.normal,
                };
        }
    }

    [System.Serializable]
    public struct SquareColors {
        public Color normal;
        public Color legal;
        public Color selected;
    }
}
