# Reggie
[![`GPL-3.0-or-later`](https://img.shields.io/badge/license-GPL--3.0--or--later-blue)](https://github.com/cainy-a/reggie/blob/master/LICENSE.md)
[![AUR package](https://repology.org/badge/version-for-repo/aur/reggie.svg)](https://repology.org/project/reggie/versions)

A new regex replace CLI tool, as an alternative to sed.

Other tools I tried had issues - some didnt support getting group by index in the replace pattern, some didnt allow inserting the original text as part of the replace pattern, etc.

This tool uses the powerful and fast regex engine in .NET.

![Image](https://raw.githubusercontent.com/cainy-a/reggie/master/screenshot.png)

## Examples
Patch usrbg for Vizality (based on my previous script [here](https://gist.github.com/cainy-a/f29ed1dc9e1b348d4f2436fa18e95db9))
(this was tested with fish, `\n` may work differently in other shells)
```fish
reggie usrbg.css '\\[data-user-id="(.{17,18})"\\]:not\\(img\\)' '$&,'\n'[vz-user-id="$1"]:not(img)' patched.css
mv patched.css usrbg.css
```
