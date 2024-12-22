namespace WavOptimizer;

internal class WavHeader : ICloneable {
	internal const long UINTMOD = (long)uint.MaxValue + 1;
	private static readonly byte[] RIFF = [82, 73, 70, 70], WAVE = [87, 65, 86, 69], FMT = [102, 109, 116, 32], DATA = [100, 97, 116, 97];
	internal bool NeedUpdate, UseOrigin;

	internal byte[] RiffID, WavID, FmtID, DataID;
	internal uint Size, FmtSize, SampleRate, BytePerSec, DataSize;
	internal ushort Format, Channels, BlockSize, Bit;

	internal WavHeader() {
		RiffID = (byte[])RIFF.Clone();
		Size = 0;
		WavID = (byte[])WAVE.Clone();
		FmtID = (byte[])FMT.Clone();
		FmtSize = 16;
		Format = 1;
		Channels = 2;
		SampleRate = 192000;
		BytePerSec = 768000;
		BlockSize = 4;
		Bit = 16;
		DataID = (byte[])DATA.Clone();
		DataSize = 0;
	}

	internal WavHeader(BinaryReader b) {
		RiffID = b.ReadBytes(4);
		if(NeedUpdate = !RiffID.SequenceEqual(RIFF)) {
			b.BaseStream.Position = 0;
			RiffID = (byte[])RIFF.Clone();
			Size = (uint)(b.BaseStream.Length % UINTMOD);
			WavID = (byte[])WAVE.Clone();
			FmtID = (byte[])FMT.Clone();
			FmtSize = 16;
			Format = 1;
			Channels = 2;
			SampleRate = 192000;
			BytePerSec = 768000;
			BlockSize = 4;
			Bit = 16;
			DataID = (byte[])DATA.Clone();
			DataSize = (uint)((b.BaseStream.Length - 44) % UINTMOD);
			return;
		}
		Size = b.ReadUInt32();
		WavID = b.ReadBytes(4);
		FmtID = b.ReadBytes(4);
		while(!FmtID.SequenceEqual(FMT)) {
			NeedUpdate = true;
			FmtID = b.ReadBytes(4);
		}
		FmtSize = b.ReadUInt32();
		if((Format = b.ReadUInt16()) != 1) {
			NeedUpdate = true;
			Format = 1;
		}
		Channels = b.ReadUInt16();
		SampleRate = b.ReadUInt32();
		BytePerSec = b.ReadUInt32();
		BlockSize = b.ReadUInt16();
		Bit = b.ReadUInt16();
		if(BlockSize != Bit / 8 * Channels) {
			NeedUpdate = true;
			BlockSize = (ushort)(Bit / 8 * Channels);
		}
		if(BytePerSec != BlockSize * SampleRate) {
			NeedUpdate = true;
			BytePerSec = BlockSize * SampleRate;
		}
		if(FmtSize > 16) {
			NeedUpdate = true;
			FmtSize = 16;
			_ = b.ReadBytes((int)FmtSize - 16);
		}
		DataID = b.ReadBytes(4);
		while(!DataID.SequenceEqual(DATA)) {
			NeedUpdate = true;
			DataID = b.ReadBytes(4);
		}
		DataSize = b.ReadUInt32();
		UseOrigin = DataSize > 0 && b.BaseStream.Length < UINTMOD;
	}

	private WavHeader(byte[] riffID, uint size, byte[] wavID, byte[] fmtID, uint fmtSize, ushort format, ushort channels, uint sampleRate, uint bytePerSec, ushort blockSize, ushort bit, byte[] dataID, uint dataSize) {
		RiffID = riffID;
		Size = size;
		WavID = wavID;
		FmtID = fmtID;
		FmtSize = fmtSize;
		Format = format;
		Channels = channels;
		SampleRate = sampleRate;
		BytePerSec = bytePerSec;
		BlockSize = blockSize;
		Bit = bit;
		DataID = dataID;
		DataSize = dataSize;
	}

	internal void WriteHeader(BinaryWriter b) {
		b.BaseStream.Position = 0;
		b.Write(RiffID);
		b.Write(Size);
		b.Write(WavID);
		b.Write(FmtID);
		b.Write(FmtSize);
		b.Write(Format);
		b.Write(Channels);
		b.Write(SampleRate);
		b.Write(BytePerSec);
		b.Write(BlockSize);
		b.Write(Bit);
		b.Write(DataID);
		b.Write(DataSize);
	}

	public object Clone() => new WavHeader((byte[])RiffID.Clone(), Size, (byte[])WavID.Clone(), (byte[])FmtID.Clone(), FmtSize, Format, Channels, SampleRate, BytePerSec, BlockSize, Bit, (byte[])DataID.Clone(), DataSize);
}