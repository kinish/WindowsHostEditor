using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace hostsEditor
{
    public partial class Form1 : Form
    {
        private ListBox listBox1 = new ListBox();
        private const string hostsFilePath = @"C:\Windows\System32\drivers\etc\hosts";
        private List<string> hostsLines = new List<string>();

        public Form1()
        {
            InitializeComponent();
            SetupUI();
            LoadHostsFile();
        }

        private void SetupUI()
        {
            this.Size = new Size(500, 300);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Text = "hostsEditor";

            listBox1.SetBounds(10, 10, 400, 200);
            listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            listBox1.DrawItem += (s, e) => DrawListBoxItem(s, e);
            Controls.AddRange(new Control[] { listBox1, CreateButton("+", 420, 10, AddButton_Click),
                                              CreateButton("-", 420, 50, RemoveButton_Click),
                                              CreateButton("#", 420, 90, CommentButton_Click) });
        }

        private Button CreateButton(string text, int x, int y, EventHandler clickEvent)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(50, 30)
            };
            button.Click += clickEvent;
            return button;
        }


        private void LoadHostsFile()
        {
            if (File.Exists(hostsFilePath))
            {
                hostsLines = File.ReadAllLines(hostsFilePath).ToList();
                UpdateListBox();
            }
            else MessageBox.Show("Hosts file not found.");
        }

        private void SaveHostsFile()
        {
            try { File.WriteAllLines(hostsFilePath, hostsLines); }
            catch (Exception ex) { MessageBox.Show("Error writing file: " + ex.Message); }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            using (var form = new Form
            {
                Text = "Add in hosts",
                ClientSize = new Size(400, 120),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var ipBox = new TextBox { Left = 120, Top = 20, Width = 250, Text = "127.0.0.1" };
                var addressBox = new TextBox { Left = 120, Top = 50, Width = 250 };
                form.Controls.AddRange(new Control[] { new Label { Left = 10, Top = 20, Text = "IP:" }, ipBox,
                                                       new Label { Left = 10, Top = 50, Text = "Site:" }, addressBox,
                                                       new Button { Text = "Ok", Left = 150, Width = 100, Top = 80, DialogResult = DialogResult.OK } });
                form.AcceptButton = form.Controls[4] as Button;

                if (form.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(ipBox.Text + " " + addressBox.Text))
                {
                    hostsLines.Add(ipBox.Text + " " + addressBox.Text);
                    UpdateListBox();
                    SaveHostsFile();
                }
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                hostsLines.RemoveAt(listBox1.SelectedIndex);
                UpdateListBox();
                SaveHostsFile();
            }
        }

        private void CommentButton_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                var index = listBox1.SelectedIndex;
                var topIndex = listBox1.TopIndex; // Запоминаем текущую позицию прокрутки

                var line = hostsLines[index];
                hostsLines[index] = line.StartsWith("#") ? line.Substring(1) : "#" + line;

                UpdateListBox();
                SaveHostsFile();

                listBox1.TopIndex = topIndex; // Восстанавливаем позицию прокрутки
                listBox1.SelectedIndex = index; // Восстанавливаем выбранную строку
            }
        }


        private void DrawListBoxItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0 && e.Index < listBox1.Items.Count)
            {
                e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds);
                e.DrawFocusRectangle();
            }
        }

        private void UpdateListBox()
        {
            listBox1.Items.Clear();
            listBox1.Items.AddRange(hostsLines.ToArray());
        }
    }
}
