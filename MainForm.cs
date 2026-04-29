using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaPiaoTool
{
    public partial class MainForm : Form
    {
        private readonly List<InvoiceItem> _invoices = new();
        private CancellationTokenSource? _cts;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Default layout
            cmbLayout.SelectedIndex = 1; // 双张
            RefreshList();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "选择PDF发票文件",
                Filter = "PDF文件|*.pdf",
                Multiselect = true
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            var duplicateFiles = new List<string>();

            foreach (var file in dlg.FileNames)
            {
                string fileName = Path.GetFileName(file);

                // Check for duplicate file names
                bool isDuplicate = false;
                foreach (var inv in _invoices)
                {
                    if (string.Equals(inv.FileName, fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (isDuplicate)
                {
                    duplicateFiles.Add(fileName);
                    continue;
                }

                try
                {
                    int pageCount = PdfMergeService.GetPageCount(file);
                    _invoices.Add(new InvoiceItem
                    {
                        FilePath = file,
                        FileName = fileName,
                        PageCount = pageCount
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法读取文件 \"{fileName}\":\n{ex.Message}",
                        "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (duplicateFiles.Count > 0)
            {
                string names = string.Join("\n", duplicateFiles);
                MessageBox.Show($"以下文件已存在，无法重复添加：\n\n{names}",
                    "重复文件提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            RefreshList();
        }

        private void BtnMoveUp_Click(object sender, EventArgs e)
        {
            var selectedIndices = new List<int>();
            foreach (int idx in lstInvoices.SelectedIndices)
                selectedIndices.Add(idx);

            if (selectedIndices.Count == 0 || selectedIndices[0] == 0) return;

            // Sort ascending for processing
            selectedIndices.Sort();

            // Move each selected item up by one
            foreach (int idx in selectedIndices)
            {
                if (idx > 0 && !selectedIndices.Contains(idx - 1))
                {
                    (_invoices[idx - 1], _invoices[idx]) = (_invoices[idx], _invoices[idx - 1]);
                }
            }

            RefreshList();

            // Re-select the moved items
            lstInvoices.SelectedIndices.Clear();
            foreach (int idx in selectedIndices)
            {
                if (idx > 0)
                    lstInvoices.SelectedIndices.Add(idx - 1);
            }
        }

        private void BtnMoveDown_Click(object sender, EventArgs e)
        {
            var selectedIndices = new List<int>();
            foreach (int idx in lstInvoices.SelectedIndices)
                selectedIndices.Add(idx);

            if (selectedIndices.Count == 0 || selectedIndices[selectedIndices.Count - 1] >= _invoices.Count - 1)
                return;

            // Sort descending for processing
            selectedIndices.Sort();
            selectedIndices.Reverse();

            // Move each selected item down by one
            foreach (int idx in selectedIndices)
            {
                if (idx < _invoices.Count - 1 && !selectedIndices.Contains(idx + 1))
                {
                    (_invoices[idx], _invoices[idx + 1]) = (_invoices[idx + 1], _invoices[idx]);
                }
            }

            RefreshList();

            // Re-select the moved items
            lstInvoices.SelectedIndices.Clear();
            selectedIndices.Sort();
            foreach (int idx in selectedIndices)
            {
                if (idx < _invoices.Count - 1)
                    lstInvoices.SelectedIndices.Add(idx + 1);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var selectedIndices = new List<int>();
            foreach (int idx in lstInvoices.SelectedIndices)
                selectedIndices.Add(idx);

            if (selectedIndices.Count == 0) return;

            // Sort descending to remove from end first
            selectedIndices.Sort();
            selectedIndices.Reverse();

            foreach (int idx in selectedIndices)
            {
                _invoices.RemoveAt(idx);
            }

            RefreshList();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            if (_invoices.Count == 0) return;

            var result = MessageBox.Show("确定要清空所有已添加的发票文件吗？",
                "确认重置", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _invoices.Clear();
                progressBar1.Value = 0;
                lblStatus.Text = "";
                RefreshList();
            }
        }

        private async void BtnMerge_Click(object sender, EventArgs e)
        {
            if (_invoices.Count == 0)
            {
                MessageBox.Show("请先添加发票文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string defaultName = $"合并发票-{DateTime.Now:yyyyMMdd}.pdf";
            using var dlg = new SaveFileDialog
            {
                Title = "保存合并后的PDF",
                Filter = "PDF文件|*.pdf",
                DefaultExt = "pdf",
                FileName = defaultName
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            var layout = cmbLayout.SelectedIndex switch
            {
                0 => LayoutMode.Single,
                1 => LayoutMode.Double,
                2 => LayoutMode.Quadruple,
                _ => LayoutMode.Double
            };

            var paths = new List<string>();
            foreach (var inv in _invoices)
                paths.Add(inv.FilePath);

            SetUiEnabled(false);
            progressBar1.Value = 0;
            lblStatus.Text = "正在合并...";

            _cts = new CancellationTokenSource();
            var progress = new Progress<int>(p =>
            {
                progressBar1.Value = Math.Min(p, 100);
                lblStatus.Text = $"正在处理... {p}%";
            });

            try
            {
                await PdfMergeService.MergeAsync(paths, layout, dlg.FileName, progress, _cts.Token);
                progressBar1.Value = 100;
                lblStatus.Text = "合并完成！";
                MessageBox.Show("发票合并完成！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "已取消";
                MessageBox.Show("操作已取消。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "处理失败";
                // Log detailed error to file for debugging
                try
                {
                    string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FaPiaoTool");
                    Directory.CreateDirectory(logDir);
                    string logFile = Path.Combine(logDir, "error.log");
                    File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}\n\n");
                }
                catch { /* ignore logging errors */ }
                MessageBox.Show($"合并过程中发生错误:\n{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                SetUiEnabled(true);
            }
        }

        private void SetUiEnabled(bool enabled)
        {
            btnAdd.Enabled = enabled;
            btnMoveUp.Enabled = enabled;
            btnMoveDown.Enabled = enabled;
            btnDelete.Enabled = enabled;
            btnReset.Enabled = enabled;
            btnMerge.Enabled = enabled;
            cmbLayout.Enabled = enabled;
            lstInvoices.Enabled = enabled;
        }

        private void RefreshList()
        {
            lstInvoices.Items.Clear();
            foreach (var inv in _invoices)
            {
                lstInvoices.Items.Add($"{inv.FileName}  ({inv.PageCount}页)");
            }
        }
    }

    public class InvoiceItem
    {
        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";
        public int PageCount { get; set; }
    }
}