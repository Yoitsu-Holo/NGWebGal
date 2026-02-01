using System;
using Xunit;
using NGWebGal.Interpreter.Moe;

namespace NGWebGal.Tests.Unit.Interpreter.Moe;

public class MoeInterpreterTests
{
	[Fact]
	public void MoeInterpreter_Initialize_Success()
	{
		var interpreter = new MoeInterpreter();
		Assert.NotNull(interpreter);
		Assert.NotNull(interpreter.ElfHeader);
		Assert.NotNull(interpreter.Runtime);
	}

	[Fact]
	public void MoeInterpreter_Clear_Success()
	{
		var interpreter = new MoeInterpreter();
		interpreter.Clear();
		Assert.NotNull(interpreter.ElfHeader);
		Assert.NotNull(interpreter.Runtime);
	}

	[Fact]
	public void MoeElfLoader_LoadELF_EmptyContent()
	{
		var interpreter = new MoeInterpreter();
		interpreter.LoadELF("");
		Assert.NotNull(interpreter.ElfHeader);
	}

	[Fact]
	public void MoeElfLoader_LoadELF_FileSection()
	{
		var interpreter = new MoeInterpreter();
		string elfContent = @".file
image1 png /path/to/image.png
.start
main";
		interpreter.LoadELF(elfContent);
		Assert.NotNull(interpreter.ElfHeader);
		Assert.Equal("main", interpreter.ElfHeader.Start);
	}

	[Fact]
	public void MoeElfLoader_LoadELF_InvalidFileEntry_Throws()
	{
		var interpreter = new MoeInterpreter();
		string elfContent = @".file
invalid_entry";
		Assert.Throws<InvalidOperationException>(() => interpreter.LoadELF(elfContent));
	}

	[Fact]
	public void MoeFileType_Enum_Values()
	{
		Assert.Equal(0x1UL, (ulong)MoeFileType.Image_png);
		Assert.Equal(0x2UL, (ulong)MoeFileType.Image_jpg);
		Assert.Equal(0x10UL, (ulong)MoeFileType.Audio_wav);
		Assert.Equal(0x100UL, (ulong)MoeFileType.Text_script);
	}
}
