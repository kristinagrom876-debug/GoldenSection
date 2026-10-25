using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Globalization;

namespace GoldenSection {
  // ==========================================
  // КЛАСС 1: Парсер формул
  // ==========================================
  public class FunctionParser {
    private string formula;
    public FunctionParser(string formula) { this.formula = formula; }

    public double Calculate(double x) {
      string expr = formula.Replace("x", "(" + x.ToString(CultureInfo.InvariantCulture) + ")");
      expr = expr.Replace("^", "*");
      object res = new DataTable().Compute(expr, null);
      return Convert.ToDouble(res);
    }

    public double GetTargetValue(double x, bool isMax, bool isRoot) {
      double y = Calculate(x);
      if (isRoot) y = Math.Abs(y);
      if (isMax) y = -y;
      return y;
    }
  }

  // ==========================================
  // КЛАСС 2: Метод золотого сечения
  // ==========================================
  public class GoldenSectionMethod {
    public double Find(double a, double b, double eps, Func<double, double> targetFunction) {
      double phi = (1 + Math.Sqrt(5)) / 2;
      double x1 = b - (b - a) / phi;
      double x2 = a + (b - a) / phi;
      double f1 = targetFunction(x1);
      double f2 = targetFunction(x2);

      while (Math.Abs(b - a) > eps) {
        if (f1 < f2) {
          b = x2; x2 = x1; f2 = f1;
          x1 = b - (b - a) / phi;
          f1 = targetFunction(x1);
        } else {
          a = x1; x1 = x2; f1 = f2;
          x2 = a + (b - a) / phi;
          f2 = targetFunction(x2);
        }
      }
      return (a + b) / 2;
    }
  }

  // ==========================================
  // КЛАСС 3: Сама форма
  // ==========================================
  public partial class Form1 : Form {
    private TextBox txtA, txtB, txtE, txtF;
    private CheckBox chkMax, chkRoot;
    private Label lblResult;
    private PictureBox pictureBox1;

    public Form1() {
      this.Text = "Лабораторная работа №1: Метод золотого сечения";
      this.Size = new Size(900, 650);
      this.StartPosition = FormStartPosition.CenterScreen;
      this.BackColor = Color.FromArgb(240, 248, 255);
      this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

      // --- МЕНЮ ---
      MenuStrip menu = new MenuStrip();
      ToolStripMenuItem fileMenu = new ToolStripMenuItem("Файл");
      ToolStripMenuItem calcItem = new ToolStripMenuItem("Рассчитать");
      ToolStripMenuItem clearItem = new ToolStripMenuItem("Очистить");
      ToolStripMenuItem exitItem = new ToolStripMenuItem("Выход");

      calcItem.Click += Рассчитать_Click;
      clearItem.Click += Очистить_Click;
      exitItem.Click += (s, e) => this.Close();

      fileMenu.DropDownItems.AddRange(new ToolStripItem[] { calcItem, clearItem, exitItem });
      menu.Items.Add(fileMenu);
      this.Controls.Add(menu);
      this.MainMenuStrip = menu;

      // --- БЛОК ВВОДА ДАННЫХ ---
      Panel inputPanel = new Panel() {
        Location = new Point(20, 40),
        Size = new Size(280, 350),
        BackColor = Color.White,
        BorderStyle = BorderStyle.FixedSingle
      };
      this.Controls.Add(inputPanel);

      Label lblTitleInput = new Label() {
        Text = "Значения",
        Font = new Font("Segoe UI", 12F, FontStyle.Bold),
        Location = new Point(10, 10),
        AutoSize = true,
        ForeColor = Color.DarkBlue
      };
      inputPanel.Controls.Add(lblTitleInput);

      inputPanel.Controls.Add(new Label() { Text = "Начало интервала (a):", Location = new Point(10, 50), AutoSize = true });
      txtA = new TextBox() { Location = new Point(10, 75), Width = 250, Text = "0" };
      inputPanel.Controls.Add(txtA);

      inputPanel.Controls.Add(new Label() { Text = "Конец интервала (b):", Location = new Point(10, 110), AutoSize = true });
      txtB = new TextBox() { Location = new Point(10, 135), Width = 250, Text = "5" };
      inputPanel.Controls.Add(txtB);

      inputPanel.Controls.Add(new Label() { Text = "Точность (e):", Location = new Point(10, 170), AutoSize = true });
      txtE = new TextBox() { Location = new Point(10, 195), Width = 250, Text = "0.001" };
      inputPanel.Controls.Add(txtE);

      inputPanel.Controls.Add(new Label() { Text = "Функция f(x):", Location = new Point(10, 230), AutoSize = true });
      txtF = new TextBox() { Location = new Point(10, 255), Width = 250, Text = "x*x - 4*x + 3" };
      inputPanel.Controls.Add(txtF);

      chkMax = new CheckBox() { Text = "Искать максимум", Location = new Point(10, 295), AutoSize = true };
      chkRoot = new CheckBox() { Text = "Искать корень (|f(x)|)", Location = new Point(10, 320), AutoSize = true };
      inputPanel.Controls.Add(chkMax);
      inputPanel.Controls.Add(chkRoot);

      chkMax.CheckedChanged += (s, ev) => { if (chkMax.Checked) chkRoot.Checked = false; };
      chkRoot.CheckedChanged += (s, ev) => { if (chkRoot.Checked) chkMax.Checked = false; };

      // --- БЛОК РЕЗУЛЬТАТА ---
      GroupBox resultGroup = new GroupBox() {
        Text = "Результат",
        Location = new Point(20, 410),
        Size = new Size(280, 120),
        Font = new Font("Segoe UI", 10F, FontStyle.Bold)
      };
      this.Controls.Add(resultGroup);

      lblResult = new Label() {
        Text = "Нажмите \"Рассчитать\"",
        Location = new Point(10, 25),
        AutoSize = true,
        Font = new Font("Segoe UI", 11F, FontStyle.Regular),
        ForeColor = Color.DarkGreen
      };
      resultGroup.Controls.Add(lblResult);

      // --- ГРАФИК ---
      GroupBox graphGroup = new GroupBox() {
        Text = "График функции",
        Location = new Point(320, 40),
        Size = new Size(540, 540),
        Font = new Font("Segoe UI", 10F, FontStyle.Bold)
      };
      this.Controls.Add(graphGroup);

      pictureBox1 = new PictureBox() {
        Location = new Point(10, 25),
        Size = new Size(520, 500),
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = Color.White
      };
      graphGroup.Controls.Add(pictureBox1);
    }

    // --- ОБРАБОТЧИК КНОПКИ РАССЧИТАТЬ ---
    private void Рассчитать_Click(object sender, EventArgs e) {
      string aText = txtA.Text.Replace(',', '.');
      string bText = txtB.Text.Replace(',', '.');
      string eText = txtE.Text.Replace(',', '.');

      if (!double.TryParse(aText, NumberStyles.Any, CultureInfo.InvariantCulture, out double a) ||
          !double.TryParse(bText, NumberStyles.Any, CultureInfo.InvariantCulture, out double b) ||
          !double.TryParse(eText, NumberStyles.Any, CultureInfo.InvariantCulture, out double eps)) {
        MessageBox.Show("Пожалуйста, введите корректные числа в поля a, b и e.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      if (a >= b) { MessageBox.Show("Начало интервала (a) должно быть меньше конца (b)!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
      if (eps <= 0) { MessageBox.Show("Точность (e) должна быть больше нуля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
      if (string.IsNullOrWhiteSpace(txtF.Text)) { MessageBox.Show("Пожалуйста, введите формулу функции!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

      FunctionParser parser = new FunctionParser(txtF.Text);
      GoldenSectionMethod method = new GoldenSectionMethod();

      double xmin;
      try {
        xmin = method.Find(a, b, eps, x => parser.GetTargetValue(x, chkMax.Checked, chkRoot.Checked));
      } catch (Exception ex) {
        MessageBox.Show("Ошибка в формуле: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      double ymin = parser.Calculate(xmin);

      string resultText = $"Точка: x* = {xmin:F5}\nЗначение: f(x*) = {ymin:F5}".Replace(',', '.');
      if (chkMax.Checked) resultText += "\n(Найден максимум)";
      if (chkRoot.Checked) resultText += "\n(Найден корень)";

      lblResult.Text = resultText;

      DrawGraph(a, b, xmin, ymin, parser);
    }

    // --- ОЧИСТКА ---
    private void Очистить_Click(object sender, EventArgs e) {
      txtA.Text = ""; txtB.Text = ""; txtE.Text = ""; txtF.Text = "";
      lblResult.Text = "Нажмите \"Рассчитать\"";
      chkMax.Checked = false; chkRoot.Checked = false;
      pictureBox1.Image = null;
    }

    // --- ОТРИСОВКА ГРАФИКА ---
    private void DrawGraph(double a, double b, double xmin, double ymin, FunctionParser parser) {
      int w = pictureBox1.Width;
      int h = pictureBox1.Height;
      if (w <= 0 || h <= 0) return;

      Bitmap bmp = new Bitmap(w, h);
      Graphics g = Graphics.FromImage(bmp);
      g.Clear(Color.White);
      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

      int marginLeft = 40;
      int marginBottom = 30;
      int marginTop = 20;
      int marginRight = 20;

      int plotW = w - marginLeft - marginRight;
      int plotH = h - marginTop - marginBottom;

      // --- НАСТРОЙКА ПРИБЛИЖЕНИЯ ---
      // Делаем отступ по бокам всего 30% от ширины интервала [a, b].
      // График будет красиво уходить за края, но точка будет крупной.
      double marginX = (b - a) * 0.3;
      double drawMinX = a - marginX;
      double drawMaxX = b + marginX;

      // Ищем минимум и максимум Y (на этом новом, приближенном интервале)
      double yminG = double.MaxValue, ymaxG = double.MinValue;
      int N = 500;
      for (int i = 0; i <= N; i++) {
        double x = drawMinX + (drawMaxX - drawMinX) * i / N;
        double y;
        try { y = parser.Calculate(x); } catch { continue; }
        if (y < yminG) yminG = y;
        if (y > ymaxG) ymaxG = y;
      }
      if (yminG == ymaxG) { ymaxG += 1; yminG -= 1; }

      // Немного отступаем по Y, чтобы график не прилипал к верхнему/нижнему краю
      double padY = (ymaxG - yminG) * 0.1;
      yminG -= padY;
      ymaxG += padY;

      // Функции перевода координат
      Func<double, float> Xpix = x => (float)(marginLeft + (x - drawMinX) / (drawMaxX - drawMinX) * plotW);
      Func<double, float> Ypix = y => (float)(marginTop + plotH - (y - yminG) / (ymaxG - yminG) * plotH);

      float zeroX = (drawMinX <= 0 && drawMaxX >= 0) ? Xpix(0) : marginLeft;
      float zeroY = (yminG <= 0 && ymaxG >= 0) ? Ypix(0) : marginTop + plotH;

      // Рисуем оси
      Pen axisPen = new Pen(Color.Black, 1.5f);
      g.DrawLine(axisPen, marginLeft, zeroY, w - marginRight, zeroY);
      g.DrawLine(axisPen, w - marginRight, zeroY, w - marginRight - 8, zeroY - 4);
      g.DrawLine(axisPen, w - marginRight, zeroY, w - marginRight - 8, zeroY + 4);
      g.DrawString("X", new Font("Segoe UI", 10F, FontStyle.Bold), Brushes.Black, w - marginRight - 10, zeroY + 5);

      g.DrawLine(axisPen, zeroX, marginTop + plotH, zeroX, marginTop);
      g.DrawLine(axisPen, zeroX, marginTop, zeroX - 4, marginTop + 8);
      g.DrawLine(axisPen, zeroX, marginTop, zeroX + 4, marginTop + 8);
      g.DrawString("Y", new Font("Segoe UI", 10F, FontStyle.Bold), Brushes.Black, zeroX + 5, marginTop - 5);

      // Деления по X (строго на интервале [a, b])
      Font labelFont = new Font("Segoe UI", 8F, FontStyle.Regular);
      int steps = 5;
      for (int i = 0; i <= steps; i++) {
        double val = a + (b - a) * i / steps;
        float px = Xpix(val);
        g.DrawLine(axisPen, px, zeroY, px, zeroY + 5);
        string text = Math.Round(val, 2).ToString().Replace(',', '.');
        float textW = g.MeasureString(text, labelFont).Width;
        g.DrawString(text, labelFont, Brushes.Black, px - textW / 2, zeroY + 7);
      }

      // Деления по Y
      for (int i = 0; i <= steps; i++) {
        double val = yminG + (ymaxG - yminG) * i / steps;
        float py = Ypix(val);
        g.DrawLine(axisPen, zeroX, py, zeroX - 5, py);
        string text = Math.Round(val, 2).ToString().Replace(',', '.');
        float textW = g.MeasureString(text, labelFont).Width;
        g.DrawString(text, labelFont, Brushes.Black, zeroX - textW - 7, py - 7);
      }

      // Рисуем кривую
      Pen curve = new Pen(Color.RoyalBlue, 2.5f);
      PointF? prev = null;
      for (int i = 0; i <= N; i++) {
        double x = drawMinX + (drawMaxX - drawMinX) * i / N;
        double y;
        try { y = parser.Calculate(x); } catch { prev = null; continue; }
        PointF p = new PointF(Xpix(x), Ypix(y));

        // Ограничиваем отрисовку
        if (p.X >= marginLeft && p.X <= w - marginRight && p.Y >= marginTop && p.Y <= marginTop + plotH) {
          if (prev.HasValue) g.DrawLine(curve, prev.Value, p);
          prev = p;
        } else {
          prev = null;
        }
      }

      // Точка экстремума
      float pxMin = Xpix(xmin);
      float pyMin = Ypix(ymin);

      Pen dashPen = new Pen(Color.Gray, 1.5f);
      dashPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
      g.DrawLine(dashPen, pxMin, zeroY, pxMin, pyMin);
      g.DrawLine(dashPen, zeroX, pyMin, pxMin, pyMin);

      Brush pointBrush = Brushes.Crimson;
      string pointText = "x* (min)";

      if (chkMax.Checked) {
        pointBrush = Brushes.DarkOrange;
        pointText = "x* (max)";
      } else if (chkRoot.Checked) {
        pointBrush = Brushes.ForestGreen;
        pointText = "x* (корень)";
      }

      g.FillEllipse(pointBrush, pxMin - 6, pyMin - 6, 12, 12);
      g.DrawEllipse(Pens.Black, pxMin - 6, pyMin - 6, 12, 12);
      g.DrawString(pointText, new Font("Segoe UI", 10F, FontStyle.Bold), pointBrush, pxMin + 10, pyMin - 25);

      pictureBox1.Image = bmp;
    }
  }
}