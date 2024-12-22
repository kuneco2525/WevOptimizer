namespace WavOptimizer;

internal struct Int24 {
	internal const int MaxValue = 8388607, MinValue = -8388608;
	private readonly byte _b0, _b1, _b2;

	internal Int24(long value) {
		_b0 = (byte)(value & 0xFF);
		_b1 = (byte)((value >> 8) & 0xFF);
		_b2 = (byte)((value >> 16) & 0xFF);
	}

	internal Int24(ReadOnlySpan<byte> b) {
		_b0 = b.Length > 0 ? b[0] : (byte)0;
		_b1 = b.Length > 1 ? b[1] : (byte)0;
		_b2 = b.Length > 2 ? b[2] : (byte)0;
	}

	internal readonly int Value {
		get {
			int v = _b0 | (_b1 << 8) | (_b2 << 16);
			return v > MaxValue ? v - UInt24.MaxValue - 1 : v;
		}
	}
}