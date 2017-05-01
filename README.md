# loco16

No longer worked on, however this application successfully decodes the save files of the game [Locomotion](https://en.wikipedia.org/wiki/Chris_Sawyer%27s_Locomotion) by Chris Sawyer and outputs the decoded chunks of a selected save file. It also converts those chunks back into a save file (i.e. after editing) that can be loaded by the game.

The goal of this was to prevent the progress I made in one game from being destroyed because it was so fun building a rail line between a bunch of small towns that I wasn't paying attention to the market and became bankrupt in-game. The application doesn't compress the save file back to original size because it wasn't in the scope of what I wanted to do and the game is okay with reading from uncompressed data (header magic <3). Also the game will compress the file again if you save it.

Existing save-file decoders/editors stopped working after the game was updated and ported to Steam.
