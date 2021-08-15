using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Perft : MonoBehaviour
{
    Board board;
    MoveGenerator gen;

    [SerializeField] TMPro.TextMeshProUGUI Log;
    [SerializeField] InputField FenInput;
    [SerializeField] InputField DepthInput;

    public void Start() {
        gen = new MoveGenerator();
        board = new Board();
    }

    public void RunSuite(TextAsset json) {
        PerftData tests = JsonUtility.FromJson<PerftData>(json.text);
        StartCoroutine(RunTests(tests));
    }

    public void RunCustom() {
        string fen = FenInput.text;
        int depth = int.Parse(DepthInput.text);
        Log.text = "Running from " + fen + ", depth " + depth + "...";
        board.LoadPosition(fen);
        var stw = new System.Diagnostics.Stopwatch();
        stw.Start();
        int numNodesFound = Search(depth);
        stw.Stop();
        Log.text += "\n" + numNodesFound + " nodes generated in: " + stw.ElapsedMilliseconds + "ms";
    }

    IEnumerator RunTests(PerftData data) {
        Log.text = "Running tests...";
        int i = 1;
        foreach(PerftTest test in data.tests) {
            board.LoadPosition(test.fen);
            var stw = new System.Diagnostics.Stopwatch();
            stw.Start();
            int numNodesFound = Search(test.depth);
            stw.Stop();
            string success = numNodesFound == test.nodes ? " Passed. " : " Failure. ";
            Log.text += "\nTest: " + i + "/" + data.tests.Length + ":" + success + numNodesFound + " nodes generated in: " + stw.ElapsedMilliseconds + "ms";
            i++;
            yield return new WaitForEndOfFrame();
        }
    }

    int Search(int depth) {
        var moves = gen.GenerateLegalMoves(board);
        if(depth == 1) {
            return moves.Count;
        }

        int numNodes = 0;

        for (int i = 0; i < moves.Count; i++) {
            board.MakeMove(moves[i]);
            int numNodesFromThisPos = Search(depth - 1);
            numNodes += numNodesFromThisPos;
            board.UnmakeMove(moves[i]);
        }
        return numNodes;
    }

    [System.Serializable]
    struct PerftTest {
        public int depth;
        public int nodes;
        public string fen;

        public PerftTest(int depth, int nodes, string fen) {
            this.depth = depth;
            this.nodes = nodes;
            this.fen = fen;
        }
    }

    [System.Serializable]
    struct PerftData {
        public PerftTest[] tests;

        public PerftData(PerftTest[] tests) {
            this.tests = tests;
        }
    }
}
