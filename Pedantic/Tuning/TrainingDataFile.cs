using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

using Pedantic.Chess;
using Pedantic.Utilities;

namespace Pedantic.Tuning
{
    public class TrainingDataFile : IDisposable
    {
        public const int BUFFER_LENGTH = 4096;
        public const int MAX_FIELDS = 7;
        public const int FIELD_HASH = 0;
        public const int FIELD_PLY = 1;
        public const int FIELD_GAME_PLY = 2;
        public const int FIELD_FEN = 3;
        public const int FIELD_HAS_CASTLED = 4;
        public const int FIELD_EVAL = 5;
        public const int FIELD_RESULT = 6;

        public struct WdlCounts
        {
            public int TotalWins;
            public int TotalDraws;
            public int TotalLosses;

            public void UpdateCounters(PosRecord pos)
            {
                switch (pos.Result)
                {
                    case PosRecord.WDL_WIN:
                        TotalWins++;
                        break;

                    case PosRecord.WDL_DRAW:
                        TotalDraws++;
                        break;

                    case PosRecord.WDL_LOSS:
                        TotalLosses++;
                        break;
                }
            }

            public bool CanAddDraw
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    int maxDraws = Math.Max(TotalWins, TotalLosses) + 1;
                    //maxDraws += maxDraws / 8;
                    return TotalDraws < maxDraws;
                }
            }
        }

        private readonly string dataPath;
        private readonly StreamReader sr;
        private bool disposedValue;
        private readonly Random random;
        private readonly char[] buffer = new char[BUFFER_LENGTH];

        public TrainingDataFile(string path, Encoding encoding) 
        { 
            if (!System.IO.File.Exists(path))
            {
                throw new FileNotFoundException("Training data file not found.", path);
            }

            dataPath = path;
            sr = new StreamReader(path, encoding, false, BUFFER_LENGTH);
            disposedValue = false;

#if DEBUG
            random = new Random(1);
#else
            random = new Random();
#endif
        }

        public TrainingDataFile(string path) : this(path, Encoding.UTF8)
        { }

        ~TrainingDataFile()
        {
            if (!disposedValue)
            {
                sr.Close();
            }
        }

        // load all position in the file
        public List<PosRecord> LoadFile()
        {
            int lineCount = LineCount();

            if (lineCount <= 1)
            {
                throw new Exception($"Training data file is empty.");
            }

            Console.WriteLine($"Examining data file: \"{dataPath}\"");
            List<PosRecord> records = new(--lineCount); // do not count header
            int currLine = 0;
            Stopwatch clock = new();
            clock.Start();
            long currMs = clock.ElapsedMilliseconds;

            try
            {
                int len;
                ValueStringBuilder sb = new(stackalloc char[256]);
                Span<char> block = buffer;
                bool skipHeader = true;

                do
                {
                    len = sr.ReadBlock(block);
                    for (int n = 0; n < len; n++)
                    {
                        char ch = block[n];
                        if (ch == '\r')
                        {
                            continue;
                        }

                        if (ch == '\n')
                        {
                            if (!skipHeader)
                            {
                                ++currLine;
                                int count = records.Count;
                                ReadOnlySpan<char> lineSpan = sb.AsSpan();
                                if (count == AddPosRecord(records, lineSpan))
                                {
                                    Console.WriteLine($"Unrecognized format found in line {currLine}: {lineSpan[..16]}...");
                                }
                            }
                            sb.Clear();
                            skipHeader = false;

                            if (clock.ElapsedMilliseconds - currMs > 2000)
                            {
                                Console.Write($"Loading {records.Count} of {lineCount} ({records.Count * 100 / lineCount}%)...\r");
                                currMs = clock.ElapsedMilliseconds;
                            }
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                    }
                } while (len == BUFFER_LENGTH);

                clock.Stop();
                Console.WriteLine($"Loading {records.Count} of {records.Count} (100%)...");
                PrintStatistics(records);
                sr.BaseStream.Seek(0, SeekOrigin.Begin);
                return records;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected exception occurred at line {currLine}: {ex.Message}", ex);
            }
        }

        // load a subset of the data file specified by 'sampleSize' and optionally save subset
        // to its own file
        public List<PosRecord> LoadSample(int sampleSize, bool save)
        {
            // must have at least 1 randomly generated sample
            if (sampleSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleSize));
            }

            int lineCount = LineCount() - 1; // total positions in file
            if (sampleSize >= lineCount)
            {
                Console.WriteLine("Specified sample size is larger than file. Entire file will be returned.");
                return LoadFile();
            }

            StreamWriter? sw = save ? new StreamWriter(OutputName(), false, new UTF8Encoding(false), BUFFER_LENGTH) : null;
            int currLine = 0;

            try
            {
                int[] selections = SampleSelections(sampleSize + sampleSize / 4, lineCount);
                List<PosRecord> records = new(sampleSize);
                ValueStringBuilder sb = new(stackalloc char[256]);
                Stopwatch clock = new();
                WdlCounts wdl = new();
                Span<char> block = buffer;

                int sel = 0;
                clock.Start();
                long currMs = clock.ElapsedMilliseconds;
                int len;
                bool skipHeader = true;

                do
                {
                    len = sr.Read(block);
                    for (int n = 0; n < len; n++)
                    {
                        char ch = block[n];

                        if (ch == '\r')
                        {
                            continue;
                        }

                        if (ch == '\n')
                        {
                            if (!skipHeader)
                            {
                                if (currLine++ == selections[sel])
                                {
                                    ReadOnlySpan<char> line = sb.AsSpan();
                                    AddPosRecord(records, line, ref wdl, sw);
                                    sel++;
                                }
                            }
                            skipHeader = false;
                            sb.Clear();

                            if (clock.ElapsedMilliseconds - currMs > 2000)
                            {
                                Console.Write($"Loading {records.Count} of {sampleSize} ({(records.Count * 100) / sampleSize}%)...\r");
                                currMs = clock.ElapsedMilliseconds;
                            }
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                    }

                } while (len == BUFFER_LENGTH && records.Count < sampleSize);

                clock.Stop();
                int pctLoaded = (records.Count * 100) / sampleSize;
                Console.WriteLine($"Loading {records.Count} of {sampleSize} ({pctLoaded}%)...");
                PrintStatistics(records);
                sr.BaseStream.Seek(0, SeekOrigin.Begin);
                return records;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected exception occurred at line {currLine}: {ex.Message}", ex);
            }
            finally
            {
                sw?.Close();
            }
        }

        public int LineCount()
        {
            int lineCount = 0, len;
            Span<char> block = buffer;
            do
            {
                len = sr.ReadBlock(block);

                if (len > 0)
                {
                    lineCount += block.Count('\n');
                }
            } while (len > 0);
            sr.BaseStream.Seek(0, SeekOrigin.Begin);
            return lineCount;
        }

        private static int AddPosRecord(List<PosRecord> records, ReadOnlySpan<char> lineSpan)
        {
            StringSplitOptions options = StringSplitOptions.TrimEntries;
            Span<Range> splitRanges = stackalloc Range[MAX_FIELDS];
            int splitCount = lineSpan.Split(splitRanges, ',', options);

            if (splitCount != MAX_FIELDS)
            {
                return records.Count;
            }

            if (!int.TryParse(lineSpan[splitRanges[FIELD_PLY]], out int ply))
            {
                return records.Count;
            }

            if (!int.TryParse(lineSpan[splitRanges[FIELD_GAME_PLY]], out int gamePly))
            {
                return records.Count;
            }

            ReadOnlySpan<char> fen = lineSpan[splitRanges[FIELD_FEN]];
            if (!Notation.IsValidFen(fen))
            {
                return records.Count;
            }

            if (!short.TryParse(lineSpan[splitRanges[FIELD_EVAL]], out short eval))
            {
                return records.Count;
            }

            if (!float.TryParse(lineSpan[splitRanges[FIELD_RESULT]], out float result))
            {
                return records.Count;
            }

            records.Add(new PosRecord(ply, gamePly, fen, eval, result));
            return records.Count;
        }

        public static int AddPosRecord(List<PosRecord> records, ReadOnlySpan<char> line, ref WdlCounts wdl, StreamWriter? sw = null)
        {
            StringSplitOptions options = StringSplitOptions.TrimEntries;
            Span<Range> splitRanges = stackalloc Range[MAX_FIELDS];
            int splitCount = line.Split(splitRanges, ',', options);

            if (splitCount != MAX_FIELDS)
            {
                return records.Count;
            }

            if (!int.TryParse(line[splitRanges[FIELD_PLY]], out int ply))
            {
                return records.Count;
            }

            if (!int.TryParse(line[splitRanges[FIELD_GAME_PLY]], out int gamePly))
            {
                return records.Count;
            }

            ReadOnlySpan<char> fen = line[splitRanges[FIELD_FEN]];
            if (!Notation.IsValidFen(fen))
            {
                return records.Count;
            }

            if (!short.TryParse(line[splitRanges[FIELD_EVAL]], out short eval))
            {
                return records.Count;
            }

            if (!float.TryParse(line[splitRanges[FIELD_RESULT]], out float result))
            {
                return records.Count;
            }

            PosRecord pos = new(ply, gamePly, fen, eval, result);
            if (pos.Result != PosRecord.WDL_DRAW || wdl.CanAddDraw)
            {
                wdl.UpdateCounters(pos);
                records.Add(pos);
                sw?.WriteLine(line);
            }
            return records.Count;
        }

        public static string OutputName()
        {
            return $"Pedantic_Sample_{APP_VERSION}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        }

        public int[] SampleSelections(int size, int dataLen)
        {
            size = Math.Clamp(size, 0, dataLen);
            int i = dataLen - 1;
            int[] pop = new int[dataLen];
            while (i >= 0) { pop[i] = i--; }

            int[] selections = new int[size];

            for (int n = 0; n < size; n++)
            {
                int m = random.Next(0, dataLen);
                selections[n] = pop[m];
                pop[m] = pop[--dataLen];
            }

            Array.Sort(selections);
            return selections;
        }

        public static void PrintStatistics(IEnumerable<PosRecord> positions)
        {
            int totalWins = 0, totalDraws = 0, totalLosses = 0, totalPositions = 0;
            foreach (var pos in positions)
            {
                totalPositions++;
                switch (pos.Result)
                {
                    case PosRecord.WDL_WIN:
                        totalWins++;
                        break;

                    case PosRecord.WDL_DRAW:
                        totalDraws++;
                        break;

                    case PosRecord.WDL_LOSS:
                        totalLosses++;
                        break;
                }
            }

            double minWDL = Math.Min(totalWins, Math.Min(totalDraws, totalLosses));
            double fpWins = totalWins / minWDL;
            double fpDraws = totalDraws / minWDL;
            double fpLosses = totalLosses / minWDL;

            Console.WriteLine($"\nWDL Ratio: {totalWins:#,#} : {totalDraws:#,#} : {totalLosses:#,#} ({fpWins:F3} : {fpDraws:F3} : {fpLosses:F3})\n");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    sr.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
