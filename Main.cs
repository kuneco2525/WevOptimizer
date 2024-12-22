using System.Text.RegularExpressions;

namespace WavOptimizer;

internal class Main : Form {
	private const ulong TrimVol = 1;
	private static readonly Regex FILE = new(@"^.+[\\/][^\\/]+\.[Ww][Aa][Vv]$"), DIR = new(@"^.+[\\/]$");
	private static readonly Dictionary<int, ulong> BITMAX = new([new(1, -sbyte.MinValue), new(2, -short.MinValue), new(3, -Int24.MinValue), new(4, -(long)int.MinValue)]);

	private static readonly List<WavFile> FILES = [];
	private static readonly int[] num = [0, 0, 0, 1];
	private static readonly bool[] chk = [false, true, true, true, true];
	private static string txt = "";

	private readonly ListBox ListFiles = new();
	private readonly Button ButtonRemove = new(), ButtonClear = new(), ButtonExecute = new();
	private readonly Label Label1 = new(), Label2 = new(), Label3 = new(), Label4 = new(), Label5 = new(), Label6 = new(), LabelPhase = new(), LabelPath = new();
	private readonly TextBox TextOutput = new(), TextPost = new();
	private readonly ComboBox ComboChannel = new(), ComboBit = new(), ComboBit2 = new();
	private readonly CheckBox CheckIgnoreNoData = new(), CheckRemoveNoData = new(), CheckTrim = new(), CheckTrimBit = new(), CheckSplit = new();

	private void InitializeComponent() {
		SuspendLayout();
		ListFiles.AllowDrop = true;
		ListFiles.ForeColor = Color.DarkGreen;
		ListFiles.FormattingEnabled = true;
		ListFiles.ItemHeight = 15;
		ListFiles.Location = new Point(12, 12);
		ListFiles.Name = "ListFiles";
		ListFiles.SelectionMode = SelectionMode.MultiExtended;
		ListFiles.Size = new Size(348, 94);
		ListFiles.TabStop = false;
		ListFiles.DragDrop += new DragEventHandler(ListFiles_DragDrop);
		ListFiles.DragEnter += new DragEventHandler(ListFiles_DragEnter);
		ButtonRemove.FlatStyle = FlatStyle.Popup;
		ButtonRemove.Location = new Point(366, 12);
		ButtonRemove.Name = "ButtonRemove";
		ButtonRemove.Size = new Size(54, 23);
		ButtonRemove.TabStop = false;
		ButtonRemove.Text = "除外";
		ButtonRemove.Click += new EventHandler(ButtonRemove_Click);
		ButtonClear.FlatStyle = FlatStyle.Popup;
		ButtonClear.Location = new Point(366, 41);
		ButtonClear.Name = "ButtonClear";
		ButtonClear.Size = new Size(54, 23);
		ButtonClear.TabStop = false;
		ButtonClear.Text = "クリア";
		ButtonClear.Click += new EventHandler(ButtonClear_Click);
		ButtonExecute.FlatStyle = FlatStyle.Popup;
		ButtonExecute.Location = new Point(366, 83);
		ButtonExecute.Name = "ButtonExecute";
		ButtonExecute.Size = new Size(54, 23);
		ButtonExecute.TabIndex = 0;
		ButtonExecute.Text = "実行";
		ButtonExecute.Click += new EventHandler(ButtonExecute_Click);
		Label1.AutoSize = true;
		Label1.Location = new Point(12, 115);
		Label1.Name = "Label1";
		Label1.Size = new Size(137, 15);
		Label1.Text = "出力パス(省略で同じ場所)";
		TextOutput.ImeMode = ImeMode.Disable;
		TextOutput.Location = new Point(155, 112);
		TextOutput.Name = "TextOutput";
		TextOutput.Size = new Size(265, 23);
		TextOutput.TabIndex = 1;
		TextOutput.Text = "";
		TextOutput.TextChanged += new EventHandler(TextOutput_TextChanged);
		Label2.AutoSize = true;
		Label2.Location = new Point(12, 138);
		Label2.Name = "Label2";
		Label2.Size = new Size(100, 15);
		Label2.Text = "フェーズ1 : チャンネル";
		ComboChannel.DropDownStyle = ComboBoxStyle.DropDownList;
		ComboChannel.FlatStyle = FlatStyle.Flat;
		ComboChannel.FormattingEnabled = true;
		ComboChannel.Items.AddRange(["何もしない", "算術平均でモノラルに変換(不可逆)", "チャンネルごとに分割"]);
		ComboChannel.Location = new Point(12, 156);
		ComboChannel.Name = "ComboChannel";
		ComboChannel.Size = new Size(193, 23);
		ComboChannel.TabIndex = 2;
		ComboChannel.SelectedIndexChanged += new EventHandler(ComboChannel_SelectedIndexChanged);
		CheckIgnoreNoData.AutoSize = true;
		CheckIgnoreNoData.Enabled = false;
		CheckIgnoreNoData.Location = new Point(211, 158);
		CheckIgnoreNoData.Name = "CheckIgnoreNoData";
		CheckIgnoreNoData.Size = new Size(129, 19);
		CheckIgnoreNoData.TabIndex = 3;
		CheckIgnoreNoData.Text = "無音チャンネルを無視";
		CheckIgnoreNoData.CheckedChanged += new EventHandler(CheckIgnoreNoData_CheckedChanged);
		Label3.AutoSize = true;
		Label3.Location = new Point(12, 182);
		Label3.Name = "Label3";
		Label3.Size = new Size(102, 15);
		Label3.Text = "フェーズ2 : ビット深度";
		ComboBit.DropDownStyle = ComboBoxStyle.DropDownList;
		ComboBit.FlatStyle = FlatStyle.Flat;
		ComboBit.FormattingEnabled = true;
		ComboBit.Items.AddRange(["何もしない", "8bit以下に制限する", "16bit以下に制限する", "24bit以下に制限する", "32bit整数にする", "64bit整数にする"]);
		ComboBit.Location = new Point(12, 200);
		ComboBit.Name = "ComboBit";
		ComboBit.Size = new Size(126, 23);
		ComboBit.TabIndex = 4;
		ComboBit.SelectedIndexChanged += new EventHandler(ComboBit_SelectedIndexChanged);
		Label4.AutoSize = true;
		Label4.Location = new Point(144, 203);
		Label4.Name = "Label4";
		Label4.Size = new Size(77, 15);
		Label4.Text = "不可逆の場合";
		ComboBit2.DropDownStyle = ComboBoxStyle.DropDownList;
		ComboBit2.Enabled = false;
		ComboBit2.FlatStyle = FlatStyle.Flat;
		ComboBit2.FormattingEnabled = true;
		ComboBit2.Items.AddRange(["何もしない", "音割れ防止(ピーク 0dBフルスケール)", "音割れ防止(ビット倍率)"]);
		ComboBit2.Location = new Point(227, 200);
		ComboBit2.Name = "ComboBit2";
		ComboBit2.Size = new Size(193, 23);
		ComboBit2.TabIndex = 5;
		ComboBit2.SelectedIndexChanged += new EventHandler(ComboBit2_SelectedIndexChanged);
		Label5.AutoSize = true;
		Label5.Location = new Point(12, 226);
		Label5.Name = "Label5";
		Label5.Size = new Size(249, 15);
		Label5.Text = "フェーズ3 : 音質を劣化させずにファイルサイズを抑える";
		CheckRemoveNoData.AutoSize = true;
		CheckRemoveNoData.Checked = true;
		CheckRemoveNoData.Location = new Point(12, 246);
		CheckRemoveNoData.Name = "CheckRemoveNoData";
		CheckRemoveNoData.Size = new Size(129, 19);
		CheckRemoveNoData.TabIndex = 6;
		CheckRemoveNoData.Text = "無音チャンネルを削除";
		CheckRemoveNoData.CheckedChanged += new EventHandler(CheckRemoveNoData_CheckedChanged);
		CheckTrim.AutoSize = true;
		CheckTrim.Checked = true;
		CheckTrim.Location = new Point(147, 246);
		CheckTrim.Name = "CheckTrim";
		CheckTrim.Size = new Size(150, 19);
		CheckTrim.TabIndex = 7;
		CheckTrim.Text = "連続する無音部分を削除";
		CheckTrim.CheckedChanged += new EventHandler(CheckTrim_CheckedChanged);
		Label6.AutoSize = true;
		Label6.Location = new Point(329, 247);
		Label6.Name = "Label6";
		Label6.Size = new Size(62, 15);
		Label6.Text = "秒まで許容";
		TextPost.ImeMode = ImeMode.Disable;
		TextPost.Location = new Point(303, 244);
		TextPost.Name = "TextPost";
		TextPost.Size = new Size(20, 23);
		TextPost.TabIndex = 8;
		TextPost.Text = "1";
		TextPost.TextChanged += new EventHandler(TextPost_TextChanged);
		CheckTrimBit.AutoSize = true;
		CheckTrimBit.Checked = true;
		CheckTrimBit.Location = new Point(12, 273);
		CheckTrimBit.Name = "CheckTrimBit";
		CheckTrimBit.Size = new Size(177, 19);
		CheckTrimBit.TabIndex = 9;
		CheckTrimBit.Text = "必要最低限のビット深度に変更";
		CheckTrimBit.CheckedChanged += new EventHandler(CheckTrimBit_CheckedChanged);
		CheckSplit.AutoSize = true;
		CheckSplit.Checked = true;
		CheckSplit.Location = new Point(195, 273);
		CheckSplit.Name = "CheckSplit";
		CheckSplit.Size = new Size(218, 19);
		CheckSplit.TabIndex = 10;
		CheckSplit.Text = "4GB以上のファイルを分割する(強く推奨)";
		CheckSplit.CheckedChanged += new EventHandler(CheckSplit_CheckedChanged);
		LabelPhase.AutoSize = true;
		LabelPhase.Location = new Point(12, 298);
		LabelPhase.Name = "LabelPhase";
		LabelPhase.Size = new Size(0, 15);
		LabelPath.AutoSize = true;
		LabelPath.Location = new Point(69, 298);
		LabelPath.Name = "LabelPath";
		LabelPath.Size = new Size(0, 15);
		BackColor = Color.Honeydew;
		ClientSize = new Size(432, 322);
		Controls.Add(LabelPath);
		Controls.Add(LabelPhase);
		Controls.Add(CheckSplit);
		Controls.Add(CheckTrimBit);
		Controls.Add(TextPost);
		Controls.Add(Label6);
		Controls.Add(CheckTrim);
		Controls.Add(CheckRemoveNoData);
		Controls.Add(Label5);
		Controls.Add(ComboBit2);
		Controls.Add(Label4);
		Controls.Add(ComboBit);
		Controls.Add(Label3);
		Controls.Add(CheckIgnoreNoData);
		Controls.Add(ComboChannel);
		Controls.Add(Label2);
		Controls.Add(TextOutput);
		Controls.Add(Label1);
		Controls.Add(ButtonExecute);
		Controls.Add(ButtonClear);
		Controls.Add(ButtonRemove);
		Controls.Add(ListFiles);
		ForeColor = Color.DarkGreen;
		FormBorderStyle = FormBorderStyle.FixedSingle;
		Name = "Main";
		Text = "WavOptimizer";
		ResumeLayout(false);
		PerformLayout();
	}

	internal Main() => InitializeComponent();

	private static void AddFiles(string p) {
		Match m = FILE.Match(p);
		if(m.Success) {
			FILES.Add(new(new(p)));
			return;
		}
		if(!DIR.Match(p).Success) { return; }
		foreach(string f in Directory.GetFiles(p)) { AddFiles(f); }
		foreach(string d in Directory.GetDirectories(p)) { AddFiles(d); }
	}
	private void ListFiles_DragDrop(object? sender, DragEventArgs e) {
		string[] files = (string[])(e.Data?.GetData(DataFormats.FileDrop, false) ?? "");
		for(int i = 0; i < files.Length; ++i) { AddFiles(files[i]); }
		for(int i = 0; i < FILES.Count; ++i) { _ = ListFiles.Items.Add(files[i].Split('/', '\\')[^1]); }
	}
	private void ListFiles_DragEnter(object? sender, DragEventArgs e) { if(e.Data?.GetDataPresent(DataFormats.FileDrop) == true) e.Effect = DragDropEffects.Copy; }

	private void ButtonRemove_Click(object? sender, EventArgs e) {
		for(int i = ListFiles.SelectedIndices.Count - 1; i >= 0; --i) {
			FILES.RemoveAt(ListFiles.SelectedIndices[i]);
			ListFiles.Items.RemoveAt(ListFiles.SelectedIndices[i]);
		}
	}

	private void ButtonClear_Click(object? sender, EventArgs e) {
		ListFiles.Items.Clear();
		FILES.Clear();
	}

	private static long BytesToInt(ReadOnlySpan<byte> b) => b.Length switch { 1 => b.ToArray()[0], 2 => BitConverter.ToInt16(b), 3 => new Int24(b).Value, 4 => BitConverter.ToInt32(b), _ => BitConverter.ToInt64(b) };
	private static byte[] IntToBytes(long v, int bytes) {
		byte[] conv = BitConverter.GetBytes(bytes == 3 ? new UInt24(v).Value : v), dest = new byte[bytes];
		for(int i = 0; i < bytes; ++i) { dest[i] = conv[i]; }
		return dest;
	}
	private static decimal GetMult(ushort bit, int method, ulong peak) {
		ushort realbit = peak switch { <= -sbyte.MinValue => 8, <= -short.MinValue => 16, <= -Int24.MinValue => 24, <= -(long)int.MinValue => 32, _ => 64 };
		return bit < realbit ? method switch { 1 => BITMAX[bit] / peak, 2 => bit / realbit, _ => decimal.Zero } : method switch { 0 or 1 or 2 => decimal.One, _ => decimal.Zero };
	}
	private static ulong Abs(long value) => (ulong)(value < 0 ? -value : value);
	private static ulong[] Abs(long[] va) {
		ulong[] r = new ulong[va.Length];
		for(int i = 0; i < va.Length; ++i) { r[i] = Abs(va[i]); }
		return r;
	}

	private void Fix() {
		foreach(WavFile f in FILES) {
			if(!File.Exists(f.FullName)) { continue; }
			f.Dir = string.IsNullOrWhiteSpace(txt) ? f.Dir : txt.Replace('\\', '/');
			if(!f.Dir.EndsWith('/')) { f.Dir += '/'; }
			if(f.FullName != $"{f.Dir}{f.Name}.wav") { File.Move(f.FullName, f.FullName = $"{f.Dir}{f.Name}.wav"); }
			using(BinaryReader o = new(new FileStream(f.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))) {
				_ = Invoke(new MethodInvoker(() => LabelPath.Text = f.FullName));
				f.Header = new(o);
				f.Peak = new ulong[f.Header.Channels];
				f.Muted = new bool[f.Header.Channels];
				f.TrimEnd = new long[f.Header.Channels];
				f.IsEquals = f.Header.Channels > 1;
				f.DataSize = !f.Header.UseOrigin ? o.BaseStream.Length - o.BaseStream.Position : f.Header.DataSize;
				if(!f.Header.NeedUpdate) { continue; }
				ushort block = (ushort)(f.Header.Bit / 8);
				uint post = (uint)num[3] * f.Header.SampleRate;
				long[] e = new long[f.Header.Channels], v = new long[f.Header.Channels];
				using BinaryWriter r = new(new FileStream($"{f.Dir}{f.Name}_tmp.wav", FileMode.Create, FileAccess.Write));
				f.Header.WriteHeader(r);
				byte[] b = new byte[block];
				for(long i = 0; b.Length > 0; ++i) {
					for(ushort c = 0; c < f.Header.Channels; ++c) {
						b = o.ReadBytes(block);
						if(b.Length < 1) { break; }
						v[c] = BytesToInt(b);
						ulong a = Abs(v[c]);
						if(a > f.Peak[c]) { f.Peak[c] = a; }
						if(a <= TrimVol) { ++e[c]; } else { e[c] = 0; }
						f.TrimEnd[c] = e[c] > post ? i - e[c] + post : i;
						if(e[c] > post) { f.Muted[c] = true; }
						if(c == f.Header.Channels - 1) {
							if(f.IsEquals && v.Distinct().Count() > 1) { f.IsEquals = false; }

							if(Abs(v).Max() <= TrimVol) f.NeedTrim = true;
						}
						r.Write(IntToBytes(v[c], block));
						f.WriteSize += block;
					}
				}
				f.DataSize = f.WriteSize;
				f.Size = f.DataSize + 44;
				f.Header.DataSize = (uint)(f.DataSize % WavHeader.UINTMOD);
				f.Header.Size = (uint)(f.Size % WavHeader.UINTMOD);
				f.Header.WriteHeader(r);
			}
			File.Delete(f.FullName);
			File.Move($"{f.Dir}{f.Name}_tmp.wav", f.FullName);
			File.SetCreationTimeUtc(f.FullName, f.Create);
			File.SetLastWriteTimeUtc(f.FullName, f.LastWrite);
			GC.Collect();
		}
	}
	private void PreRead() {
		foreach(WavFile f in FILES) {
			if(!File.Exists(f.FullName)) { continue; }
			if(f.WriteSize > 0) {
				f.WriteSize = 0;
				continue;
			}
			_ = Invoke(new MethodInvoker(() => LabelPath.Text = f.FullName));
			using(BinaryReader o = new(new FileStream(f.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))) {
				f.Header = new(o);
				ushort block = (ushort)(f.Header.Bit / 8);
				uint post = (uint)num[3] * f.Header.SampleRate;
				long[] e = new long[f.Header.Channels], v = new long[f.Header.Channels];
				byte[] b = new byte[block];
				for(long i = 0; b.Length > 0; ++i) {
					for(ushort c = 0; c < f.Header.Channels; ++c) {
						b = o.ReadBytes(block);
						if(b.Length < 1) { break; }
						v[c] = BytesToInt(b);
						ulong a = Abs(v[c]);
						if(a > f.Peak[c]) f.Peak[c] = a;
						if(a <= TrimVol) { ++e[c]; } else { e[c] = 0; }
						f.TrimEnd[c] = e[c] > post ? i - e[c] + post : i;
						if(e[c] > post) { f.Muted[c] = true; }
						f.WriteSize += block;
					}
					if(b.Length < 1) { break; }
					if(f.IsEquals && v.Distinct().Count() > 1) { f.IsEquals = false; }
					if(Abs(v).Max() <= TrimVol) { f.NeedTrim = true; }
				}
				f.DataSize = f.WriteSize;
				f.WriteSize = 0;
				f.Size = f.DataSize + 44;
			}
			GC.Collect();
		}
	}
	private void ModifyCh() {
		switch(num[0]) {
			case 1:
				foreach(WavFile f in FILES) {
					if(!File.Exists(f.FullName) || f.Header.Channels < 2) { continue; }
					_ = Invoke(new MethodInvoker(() => LabelPath.Text = f.FullName));
					using(BinaryReader o = new(new FileStream(f.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))) {
						WavHeader h = new(o);
						if(f.IsEquals) { for(ushort c = 1; c < f.Peak.Length; ++c) { f.Peak[c] = 0; } }
						if(chk[0]) {
							ushort ch = 0;
							for(ushort c = 0; c < f.Peak.Length; ++c) { if(f.Peak[c] > TrimVol) { ++ch; } }
							f.Header.Channels = (ushort)(ch > 0 ? 1 : 0);
						}
						f.Header.BlockSize /= h.Channels;
						f.Header.BytePerSec /= h.Channels;
						f.Name += "_Mono";
						f.FullName = $"{f.Dir}{f.Name}.wav";
						ulong p = 0;
						using BinaryWriter r = new(new FileStream(f.FullName, FileMode.Create, FileAccess.Write));
						f.Header.WriteHeader(r);
						byte[] b = new byte[f.Header.BlockSize];
						for(long i = 0; b.Length > 0; ++i) {
							decimal a = 0;
							for(ushort c = 0; c < h.Channels; ++c) {
								b = o.ReadBytes(f.Header.BlockSize);
								if(b.Length < 1) { break; }
								decimal d = BytesToInt(b);
								if(!chk[0] || f.Peak[c] > TrimVol) a += d / f.Header.Channels;
							}
							if(b.Length < 1 && a == 0) { break; }
							long v = (long)Math.Round(a, 0);
							if(Abs(v) > p) { p = Abs(v); }
							r.Write(IntToBytes(v, f.Header.BlockSize));
							f.WriteSize += f.Header.BlockSize;
						}
						f.Header.Channels = 1;
						f.Peak = [p];
						f.Muted = [f.NeedTrim];
						f.TrimEnd = [f.TrimEnd.Max()];
						f.DataSize = f.WriteSize;
						f.WriteSize = 0;
						f.Size = f.DataSize + 44;
						f.Header.DataSize = (uint)(f.DataSize % WavHeader.UINTMOD);
						f.Header.Size = (uint)(f.Size % WavHeader.UINTMOD);
						f.Header.WriteHeader(r);
					}
					File.SetCreationTimeUtc(f.FullName, f.Create);
					File.SetLastWriteTimeUtc(f.FullName, f.LastWrite);
					GC.Collect();
				}
				break;
			case 2:
				int fc = FILES.Count;
				for(int i = 0; i < fc; ++i) {
					if(!File.Exists(FILES[i].FullName) || FILES[i].Header.Channels < 2) { continue; }
					_ = Invoke(new MethodInvoker(() => LabelPath.Text = FILES[i].FullName));
					using(BinaryReader o = new(new FileStream(FILES[i].FullName, FileMode.Open, FileAccess.Read, FileShare.Read))) {
						WavHeader h = new(o);
						if(FILES[i].IsEquals) { for(ushort c = 1; c < FILES[i].Peak.Length; ++c) { FILES[i].Peak[c] = 0; } }
						if(chk[0]) {
							ushort ch = 0;
							for(ushort c = 0; c < FILES[i].Peak.Length; ++c) { if(FILES[i].Peak[c] > TrimVol) { ++ch; } }
							FILES[i].Header.Channels = ch;
						}
						FILES[i].Header.BlockSize /= h.Channels;
						FILES[i].Header.BytePerSec /= h.Channels;
						WavFile[] f = new WavFile[FILES[i].Header.Channels];
						BinaryWriter[] r = new BinaryWriter[FILES[i].Header.Channels];
						string n = FILES[i].Name;
						int k = 0;
						for(ushort c = 0; c < h.Channels; ++c) {
							if(chk[0] && FILES[i].Peak[c] <= TrimVol) { continue; }
							if(FILES[i].Name != n) {
								FILES.Add((WavFile)FILES[i].Clone());
								FILES[^1].Name = $"{n}_{c}ch";
								FILES[^1].FullName = $"{FILES[^1].Dir}{FILES[^1].Name}.wav";
								r[k] = new(new FileStream(FILES[^1].FullName, FileMode.Create, FileAccess.Write));
								FILES[^1].Header.WriteHeader(r[k]);
								f[k++] = FILES[^1];
							} else {
								FILES[i].Name = $"{n}_{c}ch";
								FILES[i].FullName = $"{FILES[i].Dir}{FILES[i].Name}.wav";
								r[k] = new(new FileStream(FILES[i].FullName, FileMode.Create, FileAccess.Write));
								FILES[i].Header.WriteHeader(r[k]);
								f[k++] = FILES[i];
							}
						}
						byte[] b = new byte[FILES[i].Header.BlockSize];
						for(long j = 0; b.Length > 0; ++j) {
							k = 0;
							for(ushort c = 0; c < h.Channels; ++c) {
								b = o.ReadBytes(FILES[i].Header.BlockSize);
								if(b.Length < 1) { break; }
								long v = BytesToInt(b);
								if(!chk[0] || FILES[i].Peak[c] > TrimVol) {
									f[k].WriteSize += FILES[i].Header.BlockSize;
									r[k++].Write(IntToBytes(v, FILES[i].Header.BlockSize));
								}
							}
						}
						k = 0;
						for(ushort c = 0; c < h.Channels; ++c) {
							if(chk[0] && f[k].Peak[c] <= TrimVol) { continue; }
							f[k].Peak = [f[k].Peak[c]];
							f[k].TrimEnd = [f[k].TrimEnd[c]];
							f[k].Muted = [f[k].Muted[c]];
							f[k].NeedTrim = f[k].Muted[0];
							++k;
						}
						for(ushort c = 0; c < f.Length; ++c) {
							f[c].Header.Channels = 1;
							f[c].DataSize = FILES[i].WriteSize;
							f[c].WriteSize = 0;
							f[c].Size = FILES[i].DataSize + 44;
							f[c].Header.DataSize = (uint)(FILES[i].DataSize % WavHeader.UINTMOD);
							f[c].Header.Size = (uint)(FILES[i].Size % WavHeader.UINTMOD);
							f[c].Header.WriteHeader(r[c]);
							r[c].Dispose();
							File.SetCreationTimeUtc(f[c].FullName, f[c].Create);
							File.SetLastWriteTimeUtc(f[c].FullName, f[c].LastWrite);
						}
					}
					GC.Collect();
				}
				break;
		}
	}
	private void ModifyBit() {
		foreach(WavFile f in FILES) {
			if(!File.Exists(f.FullName)) { continue; }
			using(BinaryReader o = new(new FileStream(f.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))) {
				_ = Invoke(new MethodInvoker(() => LabelPath.Text = f.FullName));
				WavHeader h = new(o);
				if(num[1] > 0) { f.Header.Bit = num[1] switch { 1 => 8, 2 => 16, 3 => 24, 4 => 32, _ => 64 }; }
				decimal mult = GetMult(f.Header.Bit, num[2], f.Peak.Max());
				if(h.Bit == f.Header.Bit || mult == decimal.Zero) { continue; }
				ushort src = (ushort)(h.Bit / 8), block = (ushort)(f.Header.Bit / 8);
				f.Header.BlockSize = (ushort)(block * f.Header.Channels);
				f.Header.BytePerSec = f.Header.BlockSize * f.Header.SampleRate;
				f.Name += $"_{f.Header.Bit}bit";
				f.FullName = $"{f.Dir}{f.Name}.wav";
				f.Peak = new ulong[f.Header.Channels];
				using BinaryWriter r = new(new FileStream(f.FullName, FileMode.Create, FileAccess.Write));
				f.Header.WriteHeader(r);
				byte[] b = new byte[block];
				for(long j = 0; b.Length > 0; ++j) {
					for(ushort c = 0; c < h.Channels; ++c) {
						b = o.ReadBytes(src);
						if(b.Length < 1) { break; }
						long v = (long)(BytesToInt(b) * mult);
						ulong a = Abs(v);
						if(a > f.Peak[c]) { f.Peak[c] = a; }
						r.Write(IntToBytes(v, block));
						f.WriteSize += block;
					}
				}
				f.DataSize = f.WriteSize;
				f.WriteSize = 0;
				f.Size = f.DataSize + 44;
				f.Header.DataSize = (uint)(f.DataSize % WavHeader.UINTMOD);
				f.Header.Size = (uint)(f.Size % WavHeader.UINTMOD);
				f.Header.WriteHeader(r);
			}
			File.SetCreationTimeUtc(f.FullName, f.Create);
			File.SetLastWriteTimeUtc(f.FullName, f.LastWrite);
			GC.Collect();
		}
	}
	private void Optimize() {
		foreach(WavFile f in FILES) {
			if(!File.Exists(f.FullName)) { continue; }
			using(BinaryReader o = new(new FileStream(f.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))) {
				_ = Invoke(new MethodInvoker(() => LabelPath.Text = f.FullName));
				WavHeader h = new(o);
				if(f.IsEquals) { for(ushort c = 1; c < f.Peak.Length; ++c) { f.Peak[c] = 0; } }
				if(chk[1]) {
					ushort ch = 0;
					for(ushort c = 0; c < f.Peak.Length; ++c) { if(f.Peak[c] > TrimVol) { ++ch; } }
					f.Header.Channels = ch;
				}
				//if(chk[3]) { f.Header.Bit = f.Peak.Max() switch { <= -sbyte.MinValue => 8, <= -short.MinValue => 16, <= -Int24.MinValue => 24, <= -(long)int.MinValue => 32, _ => 64 }; }
				if(chk[3]) { f.Header.Bit = f.Peak.Max() switch { <= -short.MinValue => 16, <= -Int24.MinValue => 24, <= -(long)int.MinValue => 32, _ => 64 }; }
				f.Header.BlockSize = (ushort)(f.Header.Bit / 8 * f.Header.Channels);
				f.Header.BytePerSec = f.Header.BlockSize * f.Header.SampleRate;
				if(h.Channels == f.Header.Channels && h.Bit == f.Header.Bit && !f.NeedTrim) { continue; }
				ushort src = (ushort)(h.Bit / 8), block = (ushort)(f.Header.Bit / 8);
				uint post = (uint)num[3] * f.Header.SampleRate;
				long[] v = new long[h.Channels], e = new long[h.Channels];
				using BinaryWriter r = new(new FileStream($"{f.Dir}{f.Name}_tmp.wav", FileMode.Create, FileAccess.Write));
				f.Header.WriteHeader(r);
				byte[] b = new byte[block];
				for(long i = 0; b.Length > 0; ++i) {
					if(i > f.TrimEnd.Max()) { break; }
					for(ushort c = 0; c < h.Channels; ++c) {
						b = o.ReadBytes(src);
						if(b.Length < 1) { break; }
						v[c] = BytesToInt(b);
						if(Abs(v[c]) <= TrimVol) { ++e[c]; } else { e[c] = 0; }
					}
					if(chk[2] && e.Min() > post) { continue; }
					for(ushort c = 0; c < h.Channels; ++c) {
						if(chk[1] && f.Peak[c] <= TrimVol) { continue; }
						r.Write(IntToBytes(v[c], block));
						f.WriteSize += block;
					}
				}
				f.DataSize = f.WriteSize;
				f.WriteSize = 0;
				f.Size = f.DataSize + 44;
				f.Header.DataSize = (uint)(f.DataSize % WavHeader.UINTMOD);
				f.Header.Size = (uint)(f.Size % WavHeader.UINTMOD);
				f.Header.WriteHeader(r);
			}
			File.Delete(f.FullName);
			File.Move($"{f.Dir}{f.Name}_tmp.wav", f.FullName);
			File.SetCreationTimeUtc(f.FullName, f.Create);
			File.SetLastWriteTimeUtc(f.FullName, f.LastWrite);
			GC.Collect();
		}
	}
	private void Split4GB() {
		foreach(WavFile f in FILES) {
			if(!File.Exists(f.FullName) || f.Size <= uint.MaxValue) { continue; }
			_ = Invoke(new MethodInvoker(() => { LabelPath.Text = f.FullName; }));
			using(BinaryReader o = new(new FileStream(f.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))) {
				WavHeader h = new(o);
				int fc = (int)Math.Ceiling((decimal)f.Size / WavHeader.UINTMOD);
				long l = f.DataSize;
				uint fmax = uint.MaxValue - 44;
				for(int i = 0; l > 0; ++i) {
					uint ds = l < fmax ? (uint)l : fmax;
					long count = ds / h.BlockSize;
					h.DataSize = ds;
					h.Size = ds + 44;
					l -= ds;
					string n = $"{f.Dir}{f.Name}_{i}.wav";
					using(BinaryWriter r = new(new FileStream(n, FileMode.Create, FileAccess.Write))) {
						f.Header.WriteHeader(r);
						byte[] b = new byte[h.BlockSize];
						for(long j = 0; j < count && b.Length > 0; ++j) {
							b = o.ReadBytes(h.BlockSize);
							if(b.Length < 1) { break; }
							r.Write(b);
							f.WriteSize += h.BlockSize;
						}
						f.DataSize = f.WriteSize;
						f.WriteSize = 0;
						f.Size = f.DataSize + 44;
						f.Header.DataSize = (uint)(f.DataSize % WavHeader.UINTMOD);
						f.Header.Size = (uint)(f.Size % WavHeader.UINTMOD);
						f.Header.WriteHeader(r);
					}
					File.SetCreationTimeUtc(n, f.Create);
					File.SetLastWriteTimeUtc(n, f.LastWrite);
				}
			}
			File.Delete(f.FullName);
			GC.Collect();
		}
	}
	private async void ButtonExecute_Click(object? sender, EventArgs e) {
		if(FILES.Count < 1) { return; }
		bool b = false;
		ButtonRemove.Enabled = b;
		ButtonClear.Enabled = b;
		ButtonExecute.Enabled = b;
		TextOutput.Enabled = b;
		ComboChannel.Enabled = b;
		CheckIgnoreNoData.Enabled = b;
		ComboBit.Enabled = b;
		ComboBit2.Enabled = b;
		CheckRemoveNoData.Enabled = b;
		CheckTrim.Enabled = b;
		TextPost.Enabled = b;
		CheckTrimBit.Enabled = b;
		CheckSplit.Enabled = b;
		LabelPhase.Text = "準備中";
		await Task.Run(() => {
			Fix();
			_ = Invoke(new MethodInvoker(() => LabelPhase.Text = "Phase 0"));
			PreRead();
			_ = Invoke(new MethodInvoker(() => LabelPhase.Text = "Phase 1"));
			if(num[0] > 0) ModifyCh();
			_ = Invoke(new MethodInvoker(() => LabelPhase.Text = "Phase 2"));
			if(num[1] > 0) ModifyBit();
			_ = Invoke(new MethodInvoker(() => LabelPhase.Text = "Phase 3"));
			Optimize();
			_ = Invoke(new MethodInvoker(() => LabelPhase.Text = "Finalize"));
			if(chk[4]) Split4GB();
		});
		Close();
	}

	private void TextOutput_TextChanged(object? sender, EventArgs e) => txt = TextOutput.Text;

	private void ComboChannel_SelectedIndexChanged(object? sender, EventArgs e) {
		num[0] = ComboChannel.SelectedIndex;
		bool b = num[0] > 0;
		CheckIgnoreNoData.Enabled = b;
		CheckIgnoreNoData.Checked = b;
	}
	private void CheckIgnoreNoData_CheckedChanged(object? sender, EventArgs e) => chk[0] = CheckIgnoreNoData.Checked;

	private void ComboBit_SelectedIndexChanged(object? sender, EventArgs e) {
		num[1] = ComboBit.SelectedIndex;
		ComboBit2.Enabled = num[1] > 0;
		CheckTrimBit.Enabled = num[1] < 3;
		if(num[1] > 2) { CheckTrimBit.Checked = false; }
	}
	private void ComboBit2_SelectedIndexChanged(object? sender, EventArgs e) => num[2] = ComboBit2.SelectedIndex;

	private void CheckRemoveNoData_CheckedChanged(object? sender, EventArgs e) => chk[1] = CheckRemoveNoData.Checked;

	private void CheckTrim_CheckedChanged(object? sender, EventArgs e) => chk[2] = CheckTrim.Checked;
	private void TextPost_TextChanged(object? sender, EventArgs e) {
		if(!int.TryParse(TextPost.Text, out num[3])) { num[3] = 0; }
		TextPost.Text = num[3].ToString();
	}

	private void CheckTrimBit_CheckedChanged(object? sender, EventArgs e) => chk[3] = CheckTrimBit.Checked;

	private void CheckSplit_CheckedChanged(object? sender, EventArgs e) => chk[4] = CheckSplit.Checked;
}