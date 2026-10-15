# Word lists

`swedish.json` maps a base form to its inflected forms:

```json
{ "bil": ["bilar", "bilarna", "bilen"] }
```

The keys are the strict rule set; keys plus values are the loose one (see `WordRules` in [Dictionary/Dictionary.cs](../Dictionary/Dictionary.cs)).

The file is generated from SALDO's morphology by [tools/build_wordlist.py](../tools/build_wordlist.py). It keeps single lowercase words of 2-15 letters (a-z, å, ä, ö) and leaves out proper nouns, abbreviations, multi-word expressions, compound-formation stems and genitive (-s) forms.

## Source and licence

Språkbanken Text (2017). *SALDO's morphology*. University of Gothenburg. <https://doi.org/10.23695/agcm-ny22>, licensed [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/). The word list is adapted from it (filtered and reformatted).
