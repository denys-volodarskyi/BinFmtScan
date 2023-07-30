# Binary Format Scanner

Scans input binary for known file formats.

# Adding new format

- Add new class in the ``Formats`` folder.<br/><br/>
  > Please use or create sub-folder for the right category to keep code clean.
  >
  > For example put **BMP**, **PNG**, **JPEG** formats into ``Images`` folder.

- Inherit from ``IDetect``, set ``ID`` and ``Category``. 

- Implement ``Detect``.<br/>
  You get ``input`` to read/inspect input binary and ``fmt`` to set only if format detected.

- Optionally implement extraction logic.

- Register format detector

# Categories

> Maybe use TAGS instead

- Images
- Sounds
- Executables
- Archives
- File Systems

# Detector

It should find start of the format.

Optionally it can find format size.

# Complex / nested file formats

# Notes

- Should find CRC tables too.
- YARA rules ?

**BinaryInput** abstraction needed for:

- Simple APIs for the format implementer.
- Possible future performance optimizations reading the input.