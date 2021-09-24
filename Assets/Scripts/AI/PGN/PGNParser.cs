using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class PGNParser : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI log;
    [SerializeField] string outputFilePath = "Assets/Scripts/AI/PGN/Games.txt";
    [SerializeField] List<TextAsset> pgnFiles;

    private void Start() {
        /*foreach(TextAsset pgn in pgnFiles) {
            StartCoroutine(WriteToFile(Parse(pgn.text), pgn.name));
        }*/
    }

    string Parse(string pgnContent) {
        string[] lines = pgnContent.Split('\n');
        bool isReadingPGN = false;
        string currentPgn = "";
        string parsedGames = "";

        foreach(string line in lines) {
            if(line.Contains("[")) {
                if (isReadingPGN) {
                    isReadingPGN = false;
                    parsedGames += currentPgn.Replace("  ", " ").Trim() + '\n';
                    currentPgn = "";
                }
                continue;
            }
            else {
                isReadingPGN = true;
                string formatedLine = Regex.Replace(line, "[0-9]+\\.", "").Trim() + " ";
                currentPgn += formatedLine;
            }
        }
        return parsedGames;
    }

    IEnumerator WriteToFile(string games, string fileName) {
        var writer = new System.IO.StreamWriter(outputFilePath, true);
        writer.Write(games);
        writer.Close();
        log.text += "Parsed " + games.Split('\n').Length + " games from " + fileName + "\n";
        yield return new WaitForEndOfFrame();
    }
}
