
--| unpoka 1.0.0
--| https://github.com/JKAnderson/unpoka
--| By TKGP https://tkgp.neocities.org/

A command-line utility for unpacking gamedata from FromSoftware's PSP game モンハン日記 ぽかぽかアイルー村,
also known as Monster Hunter Diary, Monhan Nikki, Poka Poka Felyne Village, Poka Poka Airou Village, Poka Poka Airū Mura, etc.
https://en.wikipedia.org/wiki/Monster_Hunter_Diary

Supports all PSP versions including the "G" expansion; does not support the 3DS "DX" version, because it isn't packed to begin with.
This tool is strictly for unpacking the main POKAPOKA archive; doing anything with the individual files after that is left as an exercise for the reader.

If you find this tool useful, you can support me on Ko-fi or Patreon:
https://ko-fi.com/tkgp
https://www.patreon.com/c/TKGP


Usage
-----

You'll need to find your own copy of the game first; mount or extract the ROM however you see fit, and you should wind up with a USRDIR directory containing POKAPOKA.BHD, POKAPOKA.BND, and POKAPOKA.FAT.
If you want to be lazy, you can simply drop unpoka.exe into your USRDIR directory alongside the POKAPOKA files and run it; the default options will work fine for unpacking to the same directory.
If you want to be slightly less lazy, unpoka accepts two options: -i to specify the input directory, and -o to specify the output directory. Both are optional, and will default to the working directory.
You can also specify --help to basically read what I just said again but worded slightly differently.
