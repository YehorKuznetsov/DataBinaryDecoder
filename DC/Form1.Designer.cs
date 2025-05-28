namespace DC
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            button2 = new Button();
            textBox1 = new TextBox();
            checkBox1 = new CheckBox();
            checkBox2 = new CheckBox();
            progressBar1 = new ProgressBar();
            label1 = new Label();
            button3 = new Button();
            label3 = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 302);
            button1.Name = "button1";
            button1.Size = new Size(340, 42);
            button1.TabIndex = 0;
            button1.Text = "Отменить";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(354, 302);
            button2.Name = "button2";
            button2.Size = new Size(340, 42);
            button2.TabIndex = 1;
            button2.Text = "Выполнить";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(171, 12);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(523, 42);
            textBox1.TabIndex = 2;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(12, 80);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(165, 24);
            checkBox1.TabIndex = 3;
            checkBox1.Text = "Без папки \"DC-out\"";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Location = new Point(586, 80);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(106, 24);
            checkBox2.TabIndex = 4;
            checkBox2.Text = "Статистика";
            checkBox2.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(12, 254);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(682, 42);
            progressBar1.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 231);
            label1.Name = "label1";
            label1.Size = new Size(194, 20);
            label1.TabIndex = 6;
            label1.Text = "Время обработки: 00.00.00";
            // 
            // button3
            // 
            button3.Location = new Point(12, 12);
            button3.Name = "button3";
            button3.Size = new Size(153, 42);
            button3.TabIndex = 8;
            button3.Text = "Папка \"CD-out\"";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(665, 231);
            label3.Name = "label3";
            label3.Size = new Size(29, 20);
            label3.TabIndex = 9;
            label3.Text = "0%";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(704, 383);
            Controls.Add(label3);
            Controls.Add(button3);
            Controls.Add(label1);
            Controls.Add(progressBar1);
            Controls.Add(checkBox2);
            Controls.Add(checkBox1);
            Controls.Add(textBox1);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "Form1";
            Text = "DC-Распаковка";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private TextBox textBox1;
        private CheckBox checkBox1;
        private CheckBox checkBox2;
        private ProgressBar progressBar1;
        private Label label1;
        private Button button3;
        private Label label3;
    }
}
