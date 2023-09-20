# Binary Format Scanner

Scans input binary for known file formats.

> Currently it's only **PNG**.

# Command line options

```
-x: extract
```

# Adding new format

Add new class in ``Formats\<category>`` folder.
> For example **PNG** is under ``Images`` category.

Implement ``IDetector`` interface to examine the given ``BinarySource`` stream.

```csharp
void Detect(BinarySource src, ref object? res);
```
If some data found, you should set result value to
- either ``IFoundPosition`` when you don't know the size
- or ``IFoundRange`` when you know data start and size

If data position range is known it can be extracted with ``-x`` option.

# Compilation

When you build the project, [source generator](SrcGen/FormatListSourceGenerator.cs) will create ``FormatList``
and your format will be registered automatically.
