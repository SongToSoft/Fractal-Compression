[![Build Status](https://travis-ci.org/SongToSoft/Fractal-Compression.svg?branch=master)](https://travis-ci.org/SongToSoft/Fractal-Compression)

# Fractal-Compression
Fractal Image Compression Algorithm

# Example
Size of compression files depends on range block sizes.

Original size 4705kb:
![oriinal](https://github.com/TakingAway/Fractal-Compression/blob/master/NewFractalCompression/NewFractalCompression/Example/messi.bmp)

Block size - 128px; Compressed  file - size 1kb:
![oriinal](https://github.com/TakingAway/Fractal-Compression/blob/master/NewFractalCompression/NewFractalCompression/Example/128.bmp)

Block size - 64px; Compressed  file - size 4kb:
![oriinal](https://github.com/TakingAway/Fractal-Compression/blob/master/NewFractalCompression/NewFractalCompression/Example/64.bmp)

Block size - 32px; size Compressed  file - 13kb:
![oriinal](https://github.com/TakingAway/Fractal-Compression/blob/master/NewFractalCompression/NewFractalCompression/Example/32.bmp)

Block size - 16px; Compressed  file - 50kb:
![oriinal](https://github.com/TakingAway/Fractal-Compression/blob/master/NewFractalCompression/NewFractalCompression/Example/16.bmp)

Block size - 8px; Compressed  file - 197kb:
![oriinal](https://github.com/TakingAway/Fractal-Compression/blob/master/NewFractalCompression/NewFractalCompression/Example/8.bmp)

Block size - 4px; Compressed  file - 785kb:
![oriinal](https://github.com/TakingAway/Fractal-Compression/blob/master/NewFractalCompression/NewFractalCompression/Example/4.bmp)

Block size - 2px; Compressed  file - 3137kb:
![oriinal](https://github.com/TakingAway/Fractal-Compression/blob/master/NewFractalCompression/NewFractalCompression/Example/2.bmp)

In code exist method compress image with quad tree, that method more powerful than classic block system, but decompress methond not write correct.
Quad file can compress file in range [maxValue, 2px] on any part of image.
![quad](https://github.com/TakingAway/Fractal-Compression/blob/master/NewFractalCompression/NewFractalCompression/Example/Quad%20file.bmp)
