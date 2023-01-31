# Reggie

[![AUR package](https://repology.org/badge/version-for-repo/aur/reggie.svg)](https://repology.org/project/reggie/versions)

A regex replace CLI tool, as an alternative to sed with a simpler usage.

Other tools I tried had issues - some didnt support getting group by index in the replace pattern, some didnt allow inserting the original text as part of the replace pattern, etc.,
so I made my own!

This tool uses the powerful and fast regex engine in .NET.

```fish
echo "this is a test of reggie, a regular expression CLI tool" > test.txt

cat test.txt
this is a test of reggie, a regular expression CLI tool

reggie test.txt "reg.*n" "a regex replace" -
this is a test of a regex replace CLI tool
```

## Examples
Patch usrbg for Vizality (based on my previous script [here](https://gist.github.com/yellowsink/f29ed1dc9e1b348d4f2436fa18e95db9))
(this was tested with fish, `\n` may work differently in other shells)
```fish
reggie usrbg.css '\\[data-user-id="(.{17,18})"\\]:not\\(img\\)' '$&,'\n'[vz-user-id="$1"]:not(img)' patched.css
mv patched.css usrbg.css
```

## But I want find (like grep) not replace
Good news! - ripgrep has you covered :) https://github.com/BurntSushi/ripgrep
