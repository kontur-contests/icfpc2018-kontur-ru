# icfpc2018-kontur-ru
kontur.ru team @ ICFPC 2018

## Setup
1. Install Visual Studio 2017 version 15.7.4
1. Install .NET Core SDK 2.1.300 (includes .NET Core 2.1 Runtime) from https://github.com/dotnet/core/blob/master/release-notes/download-archives/2.1.0-download.md
1. Install the latest ReSharper / Rider version 2018.1.3

## How to run

To actually solve something (for example, get a trace for the model assembly) you can run tests in `SolveAllTest.cs` or `SolveAndPostResults_USE_ME_TO_POST_SOLUTIONS_FROM_YOUR_COMPUTER.cs`. You can also build and run the `houston-runner` project but be warned that it does not output the traces to the disk but posts them to an Elastic instance (as specified by `elasticUrl`) which would be unavailable to you.

## What is what in the source code

You can find the code for our strategies in the `lib` project. As you can see in the `lib/Strategies` folder we're got pretty lots of them. Let me describe a few I can name just now:

* for assembling: a simple one-volxel-by-one-from-bottom-to-top assembler, a greedy assembler which tries to build the nearest to the bot unbuilt part of the model, a slicer assembler which makes bots to build horizontal slices of the model going bottom to top
* for disassembling: a simple one-volxel-by-one disassembler, an invertor disassembler which takes an assembly trace and reverses it, a disassembler which builds cuboids 3-voxel-thick walls around parts of the model, GVoid-s everything inside those cuboids and then GVoid-s the walls
* for reassembling: our reassemblers are basically the combinations of our assemblers and disassemblers
