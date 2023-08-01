# Binary Format Scanner

Scans input binary for known file formats.

> Currently it's only **PNG**.

# Command line options

```
-x: extract
```

# Adding new format

Add new class in ``Formats\<category>``.
> For example **PNG** is under ``Images`` category.

Implement ``IDetector`` interface to examine the given ``BinarySource`` stream.

If some data found return either ``IFoundPosition`` when you don't know the size or ``IFoundRange`` when you know data start and end.

If data position range is known it can be extracted with ``-x`` option.
