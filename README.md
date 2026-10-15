# scrabble-structure-study

A study of how to structure the domain model of a Scrabble game in C#. The goal is not a playable game (yet) but to explore which classes exist, how they are grouped, and how responsibilities such as scoring and word validation are divided between them.

Most classes are currently empty skeletons with comments describing their intended role. The structure is the deliverable.

## Tech

- .NET 10 (`net10.0`), C# with nullable reference types and implicit usings enabled
- [Newtonsoft.Json](https://www.newtonsoft.com/json) 13.0.4, used to read the word list

```sh
dotnet run
```

The game first asks for the word rules: loose (inflected forms are valid words) or strict (base forms only). It then asks whether to play with an end word: before the game every player secretly suggests a 5-7 letter word from the word list, the letters of all suggestions are pooled, and the word list word that the suggestions explain best *together* (see [Round/EndWord.cs](Round/EndWord.cs)) becomes the end word. A word copied from a single player's suggestion scores zero, so a real mix always wins. Whoever plays it gets a 50-point bonus and the game ends.

`Program.cs` runs a hot-seat game in the console (2-4 players). Moves are checked (single line, no gaps, connected, star covered first, words in the word list) and scored with the standard premium squares and the 50-point bonus for using all 7 tiles.

## Structure

Each domain concept has its own folder and its own namespace, named `<Concept>System` (for example `BoardSystem`, `TileSystem`). A concept that can be scored has a companion `<Concept>Score` class next to it, so scoring logic is kept apart from the entity itself.

| Folder | Namespace | Classes | Intended role |
| --- | --- | --- | --- |
| [Board/](Board) | `BoardSystem` | `Board`, `Square`, `SquareScore` | The grid of squares. `Board` holds a `Square[,]`, the standard 15×15 layout. Each `Square` has a `ScoreType`, an `IsStart` flag (the centre star, also a double word) and an optional `Tile`. |
| [Tile/](Tile) | `TileSystem` | `Tile`, `TileBag`, `TileScore` | A tile (letter and point value) and the bag of tiles left to draw from. |
| [Player/](Player) | `PlayerSystem` | `Player`, `PlayerRack`, `PlayerScore` | A player (name, rack, score). The rack holds at most 7 tiles and is refilled to 7 after each turn. |
| [Word/](Word) | `WordSystem` | `Word`, `WordScore` | The tiles played in a turn, with their positions on the board. |
| [Round/](Round) | `RoundSystem` | `Round`, `Turn`, `RoundScore` | A round of play and the individual turns (moves) within it. |
| [Score/](Score) | `ScoreSystem` | `Score`, `ScoreType` | Shared scoring types. `ScoreType` is an enum: `Normal`, `DoubleLetter`, `TripleLetter`, `DoubleWord`, `TripleWord`. |
| [Dictionary/](Dictionary) | `DictionarySystem` | `Dictionary` | The set of valid words, used to validate a `Word`. |
| [UI/](UI) | `UISystem` | `IGameUI`, `ConsoleUI`, `SquareView`, `TurnAction` | The console front end. It renders a `SquareView[,]` map (multipliers, start square, tiles) and returns a `TurnAction` (place, exchange, pass, quit). It knows nothing about game rules. |
| [wordlists/](wordlists) | n/a | `swedish.json` | Word data: base forms mapped to their inflections. |

### Scoring hierarchy

The comment in [Score/Score.cs](Score/Score.cs) sketches how scores roll up from the smallest unit to the largest:

```
square
tile
  word
    move
      (round)
      player
```

A square's `ScoreType` modifies a tile's value, tile values add up to a word score, a word is played as a move (turn), and moves accumulate into a player's score, optionally grouped per round. The `*Score` classes (`SquareScore`, `TileScore`, `WordScore`, `RoundScore`, `PlayerScore`) are the planned home for each level of this calculation.

### Word list

[wordlists/swedish.json](wordlists/swedish.json) maps a base form to its inflected forms:

```json
{
  "bil": ["bilar", "bilarna", "bilen"]
}
```

The keys are the strict rule set; keys plus values are the loose one (`WordRules` in [Dictionary/Dictionary.cs](Dictionary/Dictionary.cs)). The file holds about 100,000 base forms and 410,000 playable words, generated from SALDO's morphology (CC BY 4.0) by [tools/build_wordlist.py](tools/build_wordlist.py). See [wordlists/README.md](wordlists/README.md) for the filtering and the source.

## Status

| Area | State |
| --- | --- |
| Board, squares, premium layout | Done (standard 15×15) |
| Tiles, bag, racks, players | Done (Swedish distribution) |
| Placement rules, word finding, scoring, dictionary | Done; no challenges, no 6-scoreless-turns end rule |
| Console UI and hot-seat game loop | Done |
| `Round`, `RoundScore`, `PlayerScore` | Empty |
