# DataBinaryDecoder
# DataBinaryProcessor
I'll format the code in the OOP view at my leisure.
## Description

This project consists of two C# WinForms applications:

- **CD (Compressor/Encoder)** - processes text files with numbers, splits them into blocks, encodes the data into binary format with additional headers and saves them as `.bin` and `.txt` (in binary representation).
- **DC (Decompressor/Decoder)** - decodes the saved files (`*-CDout.txt`) back to the original numeric format.

Both applications support multi-threaded processing, real-time interface updates, canceling operations and statistics on completion.

## Main features

- Processing of number blocks with adaptive structure
- Bitwise encoding with weights, constants, null flag, minimum absolute value
- Parallel processing with `BlockingCollection`
- Processing progress messages and statistics
- Support for `.txt` and `.bin` formats
