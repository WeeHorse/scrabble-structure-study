# scrabble-structure-study

A study of how to structure the domain model of a Scrabble game in C#. The goal is not a playable game (yet) but to explore which classes exist, how they are grouped, and how responsibilities such as scoring and word validation are divided between them.

Most classes are currently empty skeletons with comments describing their intended role. The structure is the deliverable.

## Tech

- .NET 10 (`net10.0`), C# with nullable reference types and implicit usings enabled
- [Newtonsoft.Json](https://www.newtonsoft.com/json) 13.0.4, used to read the word list

```sh
dotnet run
```

`Program.cs` is currently only a "Hello, World!" placeholder.

## Structure

Each domain concept has its own folder and its own namespace, named `<Concept>System` (for example `BoardSystem`, `TileSystem`). A concept that can be scored has a companion `<Concept>Score` class next to it, so scoring logic is kept apart from the entity itself.

| Folder | Namespace | Classes | Intended role |
| --- | --- | --- | --- |
| [Board/](Board) | `BoardSystem` | `Board`, `Square`, `SquareScore` | The grid of squares. `Board` holds a `List<List<Square>>`, currently a 3×3 prototype with double-letter and triple-word squares. Each `Square` has a `ScoreType`. |
| [Tile/](Tile) | `TileSystem` | `Tile`, `TileBag`, `TileScore` | A tile (letter and point value) and the bag of tiles left to draw from. |
| [Player/](Player) | `PlayerSystem` | `Player`, `PlayerRack`, `PlayerScore` | A player (name, rack, score). The rack holds at most 7 tiles and is refilled to 7 after each turn. |
| [Word/](Word) | `WordSystem` | `Word`, `WordScore` | The tiles played in a turn, with their positions on the board. |
| [Round/](Round) | `RoundSystem` | `Round`, `Turn`, `RoundScore` | A round of play and the individual turns (moves) within it. |
| [Score/](Score) | `ScoreSystem` | `Score`, `ScoreType` | Shared scoring types. `ScoreType` is an enum: `Normal`, `DoubleLetter`, `TripleLetter`, `DoubleWord`, `TripleWord`. |
| [Dictionary/](Dictionary) | `DictionarySystem` | `Dictionary` | The set of valid words, used to validate a `Word`. |
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
  "cykel": ["cykeln", "cyklar", "cyklarna", "cykels"],
  "bil": ["bilen", "bilar", "bilarna", "bils"]
}
```

`Dictionary` deserializes this shape into a `Dictionary<string, List<string>>`. The loader is not finished: `ReadWordList` currently parses a hard-coded JSON string rather than the file, and its result is discarded by the constructor.

## Status

| Area | State |
| --- | --- |
| `ScoreType` enum | Done |
| `Board` / `Square` | Prototype (3×3 grid, `Square` stores its `ScoreType`) |
| `Dictionary` | Partial (parses an inline sample, does not read `swedish.json`) |
| `Tile`, `TileBag`, `Player`, `PlayerRack`, `Word`, `Round`, `Turn` | Empty, with intent in comments |
| All `*Score` classes, `Score` | Empty |
| `Program.cs` | Placeholder |
