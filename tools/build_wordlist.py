#!/usr/bin/env python3
"""Builds wordlists/swedish.json from SALDO's morphology (saldom.xml).

    python3 tools/build_wordlist.py path/to/saldom.xml wordlists/swedish.json

Source: Språkbanken Text, SALDO's morphology (2017), CC BY 4.0,
https://spraakbanken.gu.se/en/resources/saldo (DOI 10.23695/agcm-ny22).

Output: {"lemma": ["inflected form", ...]}. The key is the base form (strict
rules); the key plus its list gives every playable form (loose rules).
"""
import json
import re
import sys
import xml.etree.ElementTree as ET

MAX_LENGTH = 15  # the board is 15 squares wide

# Proper nouns, abbreviations, numerals, multi-word expressions and bound
# morphemes (prefixes/suffixes) are not playable words.
SKIP_POS = {
    "pm", "pmm", "pma", "nna", "aba", "ava", "ppa", "kna", "vba", "nl", "nlm",
    "nnm", "vbm", "abm", "avm", "inm", "ppm", "pnm", "snm", "knm",
    "sxc", "mxc", "avh", "nnh", "abh",
}

# Compound-formation stems (c, cm, ci, sms) are not standalone words, and
# genitive forms (-s) are not playable in Swedish Scrabble.
SKIP_MSD = {"c", "cm", "ci", "sms"}

PLAYABLE = re.compile(rf"^[a-zåäö]{{2,{MAX_LENGTH}}}$")


def main(source, target):
    words = {}
    for _, element in ET.iterparse(source):
        if element.tag != "LexicalEntry":
            continue

        lemma = element.find("Lemma/FormRepresentation")
        features = {f.get("att"): f.get("val") for f in lemma.findall("feat")}
        base = features["writtenForm"]

        if features["partOfSpeech"] not in SKIP_POS and PLAYABLE.match(base):
            forms = words.setdefault(base, set())
            for form in element.findall("WordForm"):
                msd = form.find('feat[@att="msd"]').get("val")
                written = form.find('feat[@att="writtenForm"]').get("val")
                if (msd not in SKIP_MSD and not msd.endswith(" gen")
                        and written != base and PLAYABLE.match(written)):
                    forms.add(written)

        element.clear()

    with open(target, "w", encoding="utf-8") as out:
        out.write("{\n")
        entries = [
            f"  {json.dumps(base, ensure_ascii=False)}: "
            f"{json.dumps(sorted(forms), ensure_ascii=False)}"
            for base, forms in sorted(words.items())
        ]
        out.write(",\n".join(entries))
        out.write("\n}\n")

    total = len(words) + sum(len(f) for f in words.values())
    print(f"{len(words)} base forms, {total} playable words")


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
