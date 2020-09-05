# RomParser

An advanced script for copying roms

Usually, when you download a pack, you have a tons of roms (different versions, langages, countries, etc). The goal of this
small tool is to just try and copy the one you're the most interested in: The best match depending on your country. It was tweaked 
for my personal usage but just submit bugs or merge request and I'll update it.

## Prerequesites

* Visual Studio 2020

## Usage


```bash
RomParser --bare -c "WORLD, USA" "c:\roms\snes all" "\\PIE\roms\snes" 
```

### Options

```text
-s, summary                   Display a summary for the input and the options used.
-d, dry-run                   Do the operations without actually copying the files
-b, bare                      Display the filename only
-c, countries                 Enumarate countries using coma separated list. The ordering is important. 
                              The available values: {USA, Japan, Europe, World}
Default: USA
-r, recursive                 Search recursively in the different folders
-i, interactive               Ask if you want to overwrite the file
-f, force                     Overwrite if file already exists. Overrides -u -i
-u, update                    Check if file exists and skip it. Overrides -i
-z, uncompress                un-compress the ROM from the archive.
-h, help                      Display this help
```

## Known Issues

* Doesn't support multiple discs.
* Cannot choose using language, only countries.
* When uncompressing, doesn't check if already exists, always overwrite.