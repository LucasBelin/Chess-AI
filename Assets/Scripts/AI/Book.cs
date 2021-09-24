using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Book {

    const string gamesFilePath = "Assets/Scripts/AI/PGN/Games.txt";
    List<string> allGames;

    public Book() {
        allGames = new List<string>(File.ReadAllLines(gamesFilePath));
    }

    //Match current game with a game from the book and return the next move.
    public Move GetNextMove(List<Move> legalMoves, string gamePGN) {
        Move bookMove = new Move(0);
        //First move of the game, pick a random game from the book and return the first move
        if(string.IsNullOrEmpty(gamePGN)) {
            string randomGame = allGames[Random.Range(0, allGames.Count)];
            string move = randomGame.Split(' ')[0];
            for (int i = 0; i < legalMoves.Count; i++) {
                if(legalMoves[i].Notation == move) {
                    bookMove = legalMoves[i];
                    //Remove every game that doesnt start with the book move to speed up next search
                    for (int j = allGames.Count - 1; j >= 0; j--) {
                        if (allGames[j].Split(' ')[0] != bookMove.Notation) {
                            allGames.RemoveAt(j);
                        }
                    }
                    break;
                }
            }
        }
        //Find a match for the current game in the book and return the next move
        else {
            for (int i = 0; i < allGames.Count; i++) {
                if(allGames[i].StartsWith(gamePGN)) {
                    string move = allGames[i].Remove(0, gamePGN.Length).Trim().Split(' ')[0];
                    for (int j = 0; j < legalMoves.Count; j++) {
                        if (legalMoves[j].Notation == move) {
                            bookMove = legalMoves[j];
                        }
                    }
                }
            }
        }

        return bookMove;
    }
}
