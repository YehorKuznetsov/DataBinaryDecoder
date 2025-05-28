using System.Diagnostics;
using System.Text;

namespace DC
{
    public partial class Form1 : Form
    {
        private int headerLines;
        private CancellationTokenSource cancellationTokenSource;
        private volatile int filesProcessed;
        private volatile int totalBlocks;
        private volatile int totalNumbers;
        private Stopwatch globalStopwatch = new Stopwatch();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) => CancelOperation();
        private async void button2_Click(object sender, EventArgs e) => await ExecuteOperation();
        private void button3_Click(object sender, EventArgs e) => SelectInputFolder();

        private void CancelOperation()
        {
            cancellationTokenSource?.Cancel();
            ResetUI();
        }

        private async Task ExecuteOperation()
        {
            string inputFolder = textBox1.Text;
            if (!ValidateInputFolder(inputFolder)) return;

            cancellationTokenSource = new CancellationTokenSource();
            filesProcessed = totalBlocks = totalNumbers = 0;
            globalStopwatch.Restart();

            try
            {
                await ProcessFilesAsync(inputFolder, cancellationTokenSource.Token);
                ShowCompletionMessage();
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Операция отменена");
            }
            finally
            {
                globalStopwatch.Stop();
            }
        }

        private bool ValidateInputFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                MessageBox.Show("Папка CD-out не выбрана или не существует", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            var files = Directory.GetFiles(path, "*-CDout.txt");
            if (files.Length == 0)
            {
                MessageBox.Show("В папке нет бинарных файлов для обработки", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private async Task ProcessFilesAsync(string inputFolder, CancellationToken token)
        {
            string outputFolder = checkBox1.Checked ? null : CreateOutputFolder(inputFolder);
            var files = Directory.GetFiles(inputFolder, "*-CDout.txt");

            var tasks = files.Select(f =>
                Task.Run(() => ProcessBinaryFileAsync(f, outputFolder, token), token)
            ).ToArray();

            await Task.WhenAll(tasks);
        }

        private string CreateOutputFolder(string inputFolder)
        {
            string outputFolder = Path.Combine(Path.GetDirectoryName(inputFolder), "DC-out");
            Directory.CreateDirectory(outputFolder);
            return outputFolder;
        }

        private async Task ProcessBinaryFileAsync(string inputPath, string outputFolder, CancellationToken token)
        {
            FileInfo fileInfo = new FileInfo(inputPath);
            long totalBytes = fileInfo.Length;
            long processedBytes = 0;

            using (var reader = new StreamReader(inputPath, Encoding.UTF8))
            {
                string originalName = Path.GetFileNameWithoutExtension(inputPath);
                string cleanedName = originalName.Replace("-CDout", "");
                string outputFileName = $"{cleanedName}-DCout.txt";

                using (var writer = outputFolder == null ? StreamWriter.Null :
                    new StreamWriter(Path.Combine(outputFolder, outputFileName)))
                {
                    var binaryData = new List<string>();
                    while (!reader.EndOfStream && !token.IsCancellationRequested)
                    {
                        string line = await reader.ReadLineAsync();
                        binaryData.Add(line);
                        processedBytes += Encoding.UTF8.GetByteCount(line + Environment.NewLine);
                        UpdateProgress(totalBytes, processedBytes);
                    }

                    if (binaryData.Count == 0) return;

                    string firstLine = binaryData[0];
                    headerLines = 0;
                    for (int i = 0; i < firstLine.Length; i++)
                    {
                        headerLines = (headerLines << 1) | (firstLine[i] == '1' ? 1 : 0);
                    }

                    await writer.WriteLineAsync(headerLines.ToString());

                    var decodedBlocks = BinaryDecoder.DecodeBinaryData(binaryData, headerLines, token);

                    bool isFirstBlock = true;
                    foreach (var block in decodedBlocks)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        if (!isFirstBlock)
                        {
                            await writer.WriteLineAsync();
                        }
                        else
                        {
                            isFirstBlock = false;
                        }

                        bool isFirstNumber = true;
                        foreach (var num in block.data)
                        {
                            if (!isFirstNumber)
                            {
                                await writer.WriteLineAsync();
                            }
                            else
                            {
                                isFirstNumber = false;
                            }
                            await writer.WriteAsync(num.ToString());
                        }

                        Interlocked.Increment(ref totalBlocks);
                        Interlocked.Add(ref totalNumbers, block.data.Count);
                    }
                }
            }
            Interlocked.Increment(ref filesProcessed);
        }

        public class BinaryDecoder
        {
            public static List<(int z, List<int> data)> DecodeBinaryData(List<string> binaryData, int headerCount, CancellationToken token)
            {
                var result = new List<(int z, List<int> data)>();
                if (binaryData.Count < 2) return result;

                try
                {
                    int index = 1;
                    BitStream bitStream = new BitStream(binaryData.Skip(1));

                    while (bitStream.HasMoreBits() && !token.IsCancellationRequested)
                    {
                        int blockType = bitStream.ReadBits(1);
                        int z = bitStream.ReadBits(3);
                        int zeroFlag = bitStream.ReadBits(1);
                        int minVal = 0;

                        if (zeroFlag == 0)
                        {
                            int length = bitStream.ReadBits(4);
                            minVal = bitStream.ReadBits(length);
                        }

                        int maxBits = bitStream.ReadBits(4);
                        var constants = new List<int>();
                        for (int i = 0; i < z; i++)
                        {
                            int num = bitStream.ReadBits(maxBits);
                            int sign = bitStream.ReadBits(1);
                            constants.Add(sign == 1 ? -num : num);
                        }

                        int weightCount = bitStream.ReadBits(4);
                        int maxWeightLength = bitStream.ReadBits(4);

                        var weights = new Dictionary<int, int>();
                        for (int i = 0; i < weightCount; i++)
                        {
                            weights[i] = bitStream.ReadBits(maxWeightLength);
                        }

                        var data = new List<int>();
                        while (bitStream.HasMoreBits())
                        {
                            int weightIndex = 0;
                            while (bitStream.PeekBit() == 0 && weightIndex < weightCount - 1)
                            {
                                bitStream.ReadBits(1);
                                weightIndex++;
                            }

                            if (weightIndex >= weightCount)
                                break;

                            int weight = weights[weightIndex];
                            int num = bitStream.ReadBits(weight);
                            int sign = bitStream.ReadBits(1);
                            data.Add(sign == 1 ? -num : num);
                        }

                        var decompressed = DecompressBlock(constants, data, z, zeroFlag, minVal);
                        result.Add((z, decompressed));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка декодирования: {ex.Message}");
                }

                return result;
            }

            private static List<int> DecompressBlock(List<int> constants, List<int> data, int z, int zeroFlag, int minVal)
            {
                var result = new List<int>();
                result.AddRange(constants);

                if (zeroFlag == 1)
                {
                    foreach (var n in data)
                    {
                        int abs = Math.Abs(n) - 1;
                        result.Add(abs * Math.Sign(n));
                    }
                }
                else
                {
                    int offset = minVal;
                    foreach (var n in data)
                    {
                        int abs = Math.Abs(n) + offset;
                        result.Add(abs * Math.Sign(n));
                    }
                }

                return result;
            }
        }

        public class BitStream
        {
            private readonly List<string> binaryData;
            private int dataIndex = 0;
            private int bitIndex = 0;

            public BitStream(IEnumerable<string> data)
            {
                binaryData = data.ToList();
            }

            public bool HasMoreBits()
            {
                return dataIndex < binaryData.Count && bitIndex < binaryData[dataIndex].Length;
            }

            public int PeekBit()
            {
                if (!HasMoreBits()) return 0;
                return binaryData[dataIndex][bitIndex] == '1' ? 1 : 0;
            }

            public int ReadBits(int count)
            {
                int result = 0;
                for (int i = 0; i < count; i++)
                {
                    if (!HasMoreBits()) break;

                    result = (result << 1) | (binaryData[dataIndex][bitIndex] == '1' ? 1 : 0);
                    bitIndex++;

                    if (bitIndex >= binaryData[dataIndex].Length)
                    {
                        dataIndex++;
                        bitIndex = 0;
                    }
                }
                return result;
            }
        }

        private DateTime lastUiUpdate = DateTime.MinValue;

        private void UpdateProgress(long totalBytes, long processedBytes)
        {
            var now = DateTime.Now;
            if ((now - lastUiUpdate).TotalMilliseconds < 200)
                return;
            lastUiUpdate = now;

            Invoke(new Action(() =>
            {
                int progress = totalBytes > 0 ? (int)((double)processedBytes / totalBytes * 100) : 0;
                progress = Math.Min(progress, progressBar1.Maximum);
                progressBar1.Value = progress;
                label3.Text = $"{progress}%";
                label1.Text = $"Время обработки: {globalStopwatch.Elapsed:mm\\:ss\\.fff}";
            }));
        }

        private void ShowCompletionMessage()
        {
            Invoke(() =>
            {
                progressBar1.Value = 100;
                label3.Text = "100%";
                label1.Text = $"Время: {globalStopwatch.Elapsed:mm\\:ss\\.fff}";
            });

            if (checkBox2.Checked)
            {
                string stats = $"Файлов обработано: {filesProcessed}\nБлоков: {totalBlocks}\nЧисел: {totalNumbers}";
                MessageBox.Show(stats, "Статистика", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Расчёт выполнен!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ResetUI()
        {
            Invoke(new Action(() =>
            {
                progressBar1.Value = 0;
                label3.Text = "0%";
                label1.Text = "Время обработки: 00:00:00";
            }));
        }

        private void SelectInputFolder()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Выберите папку CD-out";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = dialog.SelectedPath;
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) { }
        private void checkBox2_CheckedChanged(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
    }
}