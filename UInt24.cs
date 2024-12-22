namespace WavOptimizer;

internal struct UInt24 {
	internal const int MaxValue = 16777215, MinValue = 0;
	private readonly byte _b0, _b1, _b2;

	internal UInt24(long value) {
		_b0 = (byte)(value & 0xFF);
		_b1 = (byte)((value >> 8) & 0xFF);
		_b2 = (byte)((value >> 16) & 0xFF);
	}

	internal UInt24(ReadOnlySpan<byte> b) {
		_b0 = b.Length > 0 ? b[0] : (byte)0;
		_b1 = b.Length > 1 ? b[1] : (byte)0;
		_b2 = b.Length > 2 ? b[2] : (byte)0;
	}

	internal readonly int Value => _b0 | (_b1 << 8) | (_b2 << 16);
}