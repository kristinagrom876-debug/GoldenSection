using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Globalization;

namespace GoldenSection {
  public class FunctionParser {
    private string formula;
    public FunctionParser(string formula) { this.formula = formula; }

    public double Calculate(double x) {
      string expr = formula.Replace("x", "(" + x.ToString(CultureInfo.InvariantCulture) + ")");

      while (expr.Contains("^")) {
        int index = expr.IndexOf("^");
        int start = index - 1;
        if (expr[start] == ')') {
          int bracketCount = 1;
          while (bracketCount > 0 && start > 0) {
            start--;
            if (expr[start] == ')') bracketCount++;
            else if (expr[start] == '(') bracketCount--;
          }
        } else {
          while (start > 0 && (char.IsDigit(expr[start - 1]) || expr[start - 1] == '.')) start--;
        }

        int end = index + 1;
        while (end < expr.Length && (char.IsDigit(expr[end]) || expr[end] == '.')) end++;

        string baseStr = expr.Substring(start, index - start);
        string expStr = expr.Substring(index + 1, end - index - 1);

        expr = expr.Substring(0, start) + "Pow(" + baseStr + "," + expStr + ")" + expr.Substring(end);
      }

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

  public partial class Form1 : Form {
    private TextBox txtA, txtB, txtE, txtF;
    private CheckBox chkMax, chkRoot;
    private Label lblResult;
    private PictureBox pictureBox1;

    public Form1() {
      this.Text = "Лабораторная работа №1: Метод золотого сечения";
      this.Size = new Size(1300, 850);
      this.StartPosition = FormStartPosition.CenterScreen;
      this.BackColor = Color.FromArgb(240, 248, 255);
      this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

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

      GroupBox graphGroup = new GroupBox() {
        Text = "График функции",
        Location = new Point(320, 40),
        Size = new Size(940, 740),
        Font = new Font("Segoe UI", 10F, FontStyle.Bold)
      };
      this.Controls.Add(graphGroup);

      pictureBox1 = new PictureBox() {
        Location = new Point(10, 25),
        Size = new Size(920, 700),
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = Color.White
      };
      graphGroup.Controls.Add(pictureBox1);
    }

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

    private void Очистить_Click(object sender, EventArgs e) {
      txtA.Text = ""; txtB.Text = ""; txtE.Text = ""; txtF.Text = "";
      lblResult.Text = "Нажмите \"Рассчитать\"";
      chkMax.Checked = false; chkRoot.Checked = false;
      pictureBox1.Image = null;
    }

    private void DrawGraph(double a, double b, double xmin, double ymin, FunctionParser parser) {
      int w = pictureBox1.Width;
      int h = pictureBox1.Height;
      if (w <= 0 || h <= 0) return;

      Bitmap bmp = new Bitmap(w, h);
      Graphics g = Graphics.FromImage(bmp);
      g.Clear(Color.White);
      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

      int marginLeft = 60;
      int marginBottom = 50;
      int marginTop = 30;
      int marginRight = 40;

      int plotW = w - marginLeft - marginRight;
      int plotH = h - marginTop - marginBottom;

      double drawMinX = a;
      double drawMaxX = b;

      double y1 = parser.Calculate(a);
      double y2 = parser.Calculate(b);
      double y3 = parser.Calculate(xmin);

      double yminG = Math.Min(y1, Math.Min(y2, y3));
      double ymaxG = Math.Max(y1, Math.Max(y2, y3));

      if (Math.Abs(ymaxG - yminG) < 0.001) { ymaxG += 1; yminG -= 1; }

      double padY = (ymaxG - yminG) * 0.5;
      yminG -= padY;
      ymaxG += padY;

      Func<double, float> Xpix = x => (float)(marginLeft + (x - drawMinX) / (drawMaxX - drawMinX) * plotW);
      Func<double, float> Ypix = y => (float)(marginTop + plotH - (y - yminG) / (ymaxG - yminG) * plotH);

      float zeroX = (drawMinX <= 0 && drawMaxX >= 0) ? Xpix(0) : marginLeft;
      float zeroY = (yminG <= 0 && ymaxG >= 0) ? Ypix(0) : marginTop + plotH;

      int N = 1000;
      double curveStartX = a - (b - a);
      double curveEndX = b + (b - a);

      Pen curve = new Pen(Color.RoyalBlue, 3.5f);
      PointF? prev = null;
      for (int i = 0; i <= N; i++) {
        double x = curveStartX + (curveEndX - curveStartX) * i / N;
        double y;
        try { y = parser.Calculate(x); } catch { prev = null; continue; }
        PointF p = new PointF(Xpix(x), Ypix(y));

        if (prev.HasValue) g.DrawLine(curve, prev.Value, p);
        prev = p;
      }

      Pen axisPen = new Pen(Color.Black, 2f);

      g.DrawLine(axisPen, 0, zeroY, w, zeroY);
      g.DrawLine(axisPen, w - 10, zeroY, w - 20, zeroY - 5);
      g.DrawLine(axisPen, w - 10, zeroY, w - 20, zeroY + 5);

      g.DrawLine(axisPen, zeroX, 0, zeroX, h);
      g.DrawLine(axisPen, zeroX, 10, zeroX - 5, 20);
      g.DrawLine(axisPen, zeroX, 10, zeroX + 5, 20);

      Font labelFont = new Font("Segoe UI", 10F, FontStyle.Regular);
      int steps = 5;

      for (int i = 0; i <= steps; i++) {
        double val = a + (b - a) * i / steps;
        float px = Xpix(val);
        g.DrawLine(axisPen, px, zeroY, px, zeroY + 6);
        string text = Math.Round(val, 2).ToString().Replace(',', '.');
        float textW = g.MeasureString(text, labelFont).Width;
        g.DrawString(text, labelFont, Brushes.Black, px - textW / 2, zeroY + 8);
      }

      for (int i = 0; i <= steps; i++) {
        double val = yminG + (ymaxG - yminG) * i / steps;
        float py = Ypix(val);
        g.DrawLine(axisPen, zeroX, py, zeroX - 6, py);
        string text = Math.Round(val, 2).ToString().Replace(',', '.');
        float textW = g.MeasureString(text, labelFont).Width;
        g.DrawString(text, labelFont, Brushes.Black, zeroX - textW - 10, py - 8);
      }

      Font axisFont = new Font("Segoe UI", 12F, FontStyle.Bold);
      g.DrawString("X", axisFont, Brushes.Black, w - 30, zeroY - 25);
      g.DrawString("Y", axisFont, Brushes.Black, zeroX + 10, 10);

      float pxMin = Xpix(xmin);
      float pyMin = Ypix(ymin);

      Pen dashPen = new Pen(Color.DimGray, 2f);
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

      g.FillEllipse(Brushes.LightGray, pxMin - 9, pyMin - 9, 18, 18);
      g.FillEllipse(pointBrush, pxMin - 7, pyMin - 7, 14, 14);
      g.DrawEllipse(Pens.Black, pxMin - 7, pyMin - 7, 14, 14);
      g.FillEllipse(Brushes.White, pxMin - 2, pyMin - 2, 4, 4);

      Font textFont = new Font("Segoe UI", 10F, FontStyle.Bold);
      SizeF textSize = g.MeasureString(pointText, textFont);

      g.FillRectangle(Brushes.White, pxMin + 15, pyMin - 20, textSize.Width + 4, textSize.Height + 4);
      g.DrawRectangle(Pens.LightGray, pxMin + 15, pyMin - 20, textSize.Width + 4, textSize.Height + 4);

      g.DrawString(pointText, textFont, pointBrush, pxMin + 17, pyMin - 18);

      pictureBox1.Image = bmp;
    }
  }
}