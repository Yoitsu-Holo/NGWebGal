using System;
using System.Collections.Generic;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// ELF (Executable and Linkable Format) loader for Moe scripts.
/// Loads program structure including files, data, functions, and forms.
/// </summary>
public partial class MoeInterpreter
{
	/// <summary>
	/// Loads ELF format script containing file mappings, data, and entry point.
	/// </summary>
	public void LoadELF(string moeElfContent)
	{
		static void LineSpaceFormatter(ref string rawString)
		{
			string[] ss = rawString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			rawString = string.Join(" ", ss);
		}

		List<string> elfLines = new(moeElfContent.Split('\n', StringSplitOptions.RemoveEmptyEntries));
		MoeElfSegment elfFlag = MoeElfSegment.Void;

		for (int lineCount = 0; lineCount < elfLines.Count; lineCount++)
		{
			string line = elfLines[lineCount].Trim();

			// Filter empty lines
			if (string.IsNullOrEmpty(line))
				continue;

			// Header Flag
			if (line[0] == '.')
			{
				elfFlag = line switch
				{
					".file" => MoeElfSegment.File,
					".data" => MoeElfSegment.Data,
					".start" => MoeElfSegment.Start,
					_ => MoeElfSegment.Void,
				};
				continue;
			}

			if (elfFlag == MoeElfSegment.Void)
				continue;

			LineSpaceFormatter(ref line);
			List<string> parts = new(line.Split(' ', StringSplitOptions.RemoveEmptyEntries));

			if (elfFlag == MoeElfSegment.File)
			{
				if (parts.Count != 3)
					throw new InvalidOperationException($"Invalid file entry format: {line}");

				MoeFile moeFile = new()
				{
					Name = parts[0],
					Type = parts[1] switch
					{
						"png" => MoeFileType.Image_png,
						"jpg" => MoeFileType.Image_jpg,
						"bmp" => MoeFileType.Image_bmp,

						"wav" => MoeFileType.Audio_wav,
						"mp3" => MoeFileType.Audio_mp3,
						"flac" => MoeFileType.Audio_flac,
						"ogg" => MoeFileType.Audio_ogg,

						"script" => MoeFileType.Text_script,
						"opera" => MoeFileType.Text_opera,
						"form" => MoeFileType.Text_form,

						"font" => MoeFileType.Bin_font,
						"bin" => MoeFileType.Bin_bin,

						_ => MoeFileType.Void,
					},
					URL = parts[2],
				};

				if ((moeFile.Type & MoeFileType.Image) != 0)
					_elfHeader.ImageFiles[moeFile.Name] = moeFile;
				else if ((moeFile.Type & MoeFileType.Audio) != 0)
					_elfHeader.AudioFiles[moeFile.Name] = moeFile;
				else if ((moeFile.Type & MoeFileType.Text) != 0)
					_elfHeader.TextFiles[moeFile.Name] = moeFile;
				else if ((moeFile.Type & MoeFileType.Bin) != 0)
					_elfHeader.BinFiles[moeFile.Name] = moeFile;

				continue;
			}

			if (elfFlag == MoeElfSegment.Data)
			{
				// Parse variable definitions from data section
				// Format: var type name = value;
				continue;
			}

			if (elfFlag == MoeElfSegment.Start)
			{
				if (parts.Count != 1)
					throw new InvalidOperationException("Invalid start entry format");
				_elfHeader.Start = parts[0];
			}
		}
	}
}

/// <summary>
/// ELF segment types
/// </summary>
public enum MoeElfSegment
{
	Void,
	File,
	Data,
	Start,
}
