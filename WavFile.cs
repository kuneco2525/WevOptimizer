namespace WavOptimizer;

internal class WavFile : ICloneable {
	internal readonly DateTime Create, LastWrite;
	internal WavHeader Header;
	internal string Dir, Name, FullName;
	internal long Size, DataSize, WriteSize;
	internal bool IsEquals, NeedTrim;
	internal ulong[] Peak;
	internal bool[] Muted;
	internal long[] TrimEnd;

	internal WavFile(FileInfo i) {
		Create = i.CreationTimeUtc;
		LastWrite = i.LastWriteTimeUtc;
		Header = new();
		Dir = (i.DirectoryName?.Replace('\\', '/') ?? "") + '/';
		Name = i.Name.Replace(i.Extension, "");
		FullName = i.FullName.Replace('\\', '/');
		Size = i.Length;
		Peak = [];
		Muted = [];
		TrimEnd = [];
	}

	private WavFile(DateTime create, DateTime lastWrite, WavHeader header, string dir, string name, string fullName, long size, long dataSize, long writeSize, bool isEquals, bool needTrim, ulong[] peak, bool[] muted, long[] trimEnd) {
		Create = create;
		LastWrite = lastWrite;
		Header = header;
		Dir = dir;
		Name = name;
		FullName = fullName;
		Size = size;
		DataSize = dataSize;
		WriteSize = writeSize;
		IsEquals = isEquals;
		NeedTrim = needTrim;
		Peak = peak;
		Muted = muted;
		TrimEnd = trimEnd;
	}

	public object Clone() => new WavFile(Create, LastWrite, (WavHeader)Header.Clone(), Dir, Name, FullName, Size, DataSize, WriteSize, IsEquals, NeedTrim, (ulong[])Peak.Clone(), (bool[])Muted.Clone(), (long[])TrimEnd.Clone());
}