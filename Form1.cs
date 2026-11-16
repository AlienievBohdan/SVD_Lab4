using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using MySql.Data.MySqlClient;

namespace SVD_Lab4
{
    public partial class Form1 : Form
    {
        private static readonly string ConnectionString =
            "Server=localhost;Port=3306;Database=alieniev_db;Uid=svd_lab4;Pwd=" + ReadPassword() + ";" +
            "CharSet=utf8mb4;AllowPublicKeyRetrieval=true;";

        private static string ReadPassword()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "db_password.txt");
            return File.Exists(path) ? File.ReadAllText(path).Trim() : "YOUR_PASSWORD";
        }

        private static readonly string XmlDir = Path.Combine(AppContext.BaseDirectory, "XmlFiles");
        private static readonly string TableXml = Path.Combine(XmlDir, "A_01_table.xml");
        private static readonly string QueryXml = Path.Combine(XmlDir, "query_result.xml");
        private static readonly string FilterXml = Path.Combine(XmlDir, "filter_result.xml");
        private static readonly string ImportXml = Path.Combine(XmlDir, "new_users.xml");

        private const string SelectQuery =
            "SELECT m.id AS user_id, m.name AS user_name, d.id AS phone_id, d.phone " +
            "FROM A_01 m INNER JOIN A_01_Details d ON m.id = d.main_id " +
            "ORDER BY m.id, d.id";

        private Button btnTableToXml, btnQueryToXml, btnSearch, btnFilter, btnSelectTable, btnXmlToDb;
        private TextBox txtSearch, txtFilter, txtXml;
        private DataGridView grid;
        private SplitContainer split;
        private Label lblGrid, lblXml, lblStatus;

        public Form1()
        {
            InitializeComponent();
            Directory.CreateDirectory(XmlDir);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "Лабораторна робота №4 – XML та БД MySQL (Алєнєв Б. А., СН-31)";
            Font = new Font("Segoe UI", 10F);
            ClientSize = new Size(1280, 760);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            Panel top = new Panel { Dock = DockStyle.Top, Height = 140 };
            btnTableToXml = MakeButton(top, "п.2  Таблиця A_01 → XML", 12, 12, 230);
            btnQueryToXml = MakeButton(top, "п.3  SELECT → XML", 252, 12, 180);
            btnSelectTable = MakeButton(top, "SELECT * FROM A_01", 442, 12, 200);
            btnXmlToDb = MakeButton(top, "п.6  XML → таблиця A_01", 652, 12, 230);

            top.Controls.Add(new Label { Text = "Ключ пошуку (user_name):", Left = 12, Top = 62, Width = 200, Height = 24 });
            txtSearch = new TextBox { Left = 212, Top = 58, Width = 260, Text = "Bohdan Alieniev" };
            top.Controls.Add(txtSearch);
            btnSearch = MakeButton(top, "п.4  Пошук у XML", 482, 54, 200);

            top.Controls.Add(new Label { Text = "Умова фільтрації:", Left = 12, Top = 102, Width = 200, Height = 24 });
            txtFilter = new TextBox { Left = 212, Top = 98, Width = 260, Text = "phone LIKE '+38093*'" };
            top.Controls.Add(txtFilter);
            btnFilter = MakeButton(top, "п.5  Фільтрація → XML", 482, 94, 200);

            lblStatus = new Label { Dock = DockStyle.Bottom, Height = 30, Padding = new Padding(8, 4, 0, 0), ForeColor = Color.DarkGreen };

            split = new SplitContainer { Dock = DockStyle.Fill };
            Load += (s, e) => split.SplitterDistance = split.Width * 45 / 100;
            lblGrid = new Label { Dock = DockStyle.Top, Height = 26, Text = "Дані:", Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            grid = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White
            };
            split.Panel1.Controls.Add(grid);
            split.Panel1.Controls.Add(lblGrid);

            lblXml = new Label { Dock = DockStyle.Top, Height = 26, Text = "Вміст XML файлу:", Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            txtXml = new TextBox
            {
                Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both,
                WordWrap = false, Font = new Font("Consolas", 10F), BackColor = Color.White
            };
            split.Panel2.Controls.Add(txtXml);
            split.Panel2.Controls.Add(lblXml);

            Controls.Add(split);
            Controls.Add(lblStatus);
            Controls.Add(top);

            btnTableToXml.Click += btnTableToXml_Click;
            btnQueryToXml.Click += btnQueryToXml_Click;
            btnSearch.Click += btnSearch_Click;
            btnFilter.Click += btnFilter_Click;
            btnSelectTable.Click += btnSelectTable_Click;
            btnXmlToDb.Click += btnXmlToDb_Click;
            ResumeLayout(false);
        }

        private Button MakeButton(Control parent, string text, int left, int top, int width)
        {
            Button b = new Button { Text = text, Left = left, Top = top, Width = width, Height = 34 };
            parent.Controls.Add(b);
            return b;
        }

        private void btnTableToXml_Click(object sender, EventArgs e)
        {
            try
            {
                DataSet ds = new DataSet("Alieniev_db");
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT id, name FROM A_01 ORDER BY id", connection);
                    adapter.Fill(ds, "A_01");
                }
                ds.WriteXml(TableXml);
                ShowResult(ds.Tables[0], "Таблиця A_01 (з БД)", TableXml,
                    $"Таблицю A_01 ({ds.Tables[0].Rows.Count} записів) збережено у файл {Path.GetFileName(TableXml)}");
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void btnQueryToXml_Click(object sender, EventArgs e)
        {
            try
            {
                DataSet ds = new DataSet("QueryResult");
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    MySqlDataAdapter adapter = new MySqlDataAdapter(SelectQuery, connection);
                    adapter.Fill(ds, "UserPhone");
                    connection.Close();
                }
                ds.WriteXml(QueryXml);
                ShowResult(ds.Tables[0], "Результат запиту SELECT (A_01 JOIN A_01_Details)", QueryXml,
                    $"Результат запиту ({ds.Tables[0].Rows.Count} записів) збережено у файл {Path.GetFileName(QueryXml)}");
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(QueryXml)) { MessageBox.Show("Спочатку створіть файл query_result.xml (кнопка п.3)."); return; }
                string key = txtSearch.Text.Trim();

                DataSet ds = new DataSet();
                using (XmlReader xmlFile = XmlReader.Create(QueryXml, new XmlReaderSettings()))
                {
                    ds.ReadXml(xmlFile);
                }
                DataView dv = new DataView(ds.Tables[0]);
                dv.Sort = "user_name";
                DataRowView[] found = dv.FindRows(key);

                DataTable result = ds.Tables[0].Clone();
                foreach (DataRowView row in found) result.ImportRow(row.Row);
                txtXml.Text = ReadXmlText(QueryXml);
                lblXml.Text = "Файл, у якому виконується пошук: " + Path.GetFileName(QueryXml);
                lblGrid.Text = $"Результат пошуку «{key}»";
                BindGrid(result);

                if (found.Length == 0)
                {
                    SetStatus($"Пошук «{key}»: запис не знайдено", Color.DarkRed);
                    MessageBox.Show($"Користувача «{key}» у файлі query_result.xml не знайдено.",
                        "Пошук у XML", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    string phones = "";
                    foreach (DataRowView row in found) phones += Environment.NewLine + "   " + row["phone"];
                    SetStatus($"Пошук «{key}»: знайдено записів – {found.Length}", Color.DarkGreen);
                    MessageBox.Show($"Знайдено: {found[0]["user_name"]} (user_id = {found[0]["user_id"]})" +
                        Environment.NewLine + "Телефони:" + phones,
                        "Пошук у XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(QueryXml)) { MessageBox.Show("Спочатку створіть файл query_result.xml (кнопка п.3)."); return; }
                DataSet ds = new DataSet();
                using (XmlReader xmlFile = XmlReader.Create(QueryXml, new XmlReaderSettings()))
                {
                    ds.ReadXml(xmlFile);
                }
                DataView dv = new DataView(ds.Tables[0], txtFilter.Text, "user_name", DataViewRowState.CurrentRows);

                DataSet result = new DataSet("FilterResult");
                result.Tables.Add(dv.ToTable("UserPhone"));
                result.WriteXml(FilterXml);
                ShowResult(result.Tables[0], $"Результат фільтрації: {txtFilter.Text}", FilterXml,
                    $"Відібрано записів: {result.Tables[0].Rows.Count}; результат записано у файл {Path.GetFileName(FilterXml)}");
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void btnSelectTable_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable table = new DataTable("A_01");
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    new MySqlDataAdapter("SELECT * FROM A_01 ORDER BY id", connection).Fill(table);
                }
                BindGrid(table);
                lblGrid.Text = "SELECT * FROM A_01 ORDER BY id";
                SetStatus($"Таблиця A_01 містить {table.Rows.Count} записів ({DateTime.Now:HH:mm:ss})", Color.DarkGreen);
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void btnXmlToDb_Click(object sender, EventArgs e)
        {
            try
            {
                DataSet ds = new DataSet();
                using (XmlReader xmlFile = XmlReader.Create(ImportXml, new XmlReaderSettings()))
                {
                    ds.ReadXml(xmlFile);
                }
                txtXml.Text = ReadXmlText(ImportXml);
                lblXml.Text = "Вміст XML файлу: " + Path.GetFileName(ImportXml);

                int inserted = 0, skipped = 0;
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                    {
                        string name = ds.Tables[0].Rows[i]["name"].ToString().Trim();

                        MySqlCommand check = new MySqlCommand("SELECT COUNT(*) FROM A_01 WHERE name = @name", connection);
                        check.Parameters.AddWithValue("@name", name);
                        if (Convert.ToInt32(check.ExecuteScalar()) > 0) { skipped++; continue; }

                        MySqlCommand command = new MySqlCommand("INSERT INTO A_01 (name) VALUES (@name)", connection);
                        command.Parameters.AddWithValue("@name", name);
                        inserted += command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                BindGrid(ds.Tables[0]);
                lblGrid.Text = "Записи, прочитані з new_users.xml";
                SetStatus($"З файлу {Path.GetFileName(ImportXml)} додано у таблицю A_01 записів: {inserted}" +
                          (skipped > 0 ? $" (пропущено вже наявних: {skipped})" : ""), Color.DarkGreen);
                MessageBox.Show($"Додано записів у таблицю A_01: {inserted}", "XML → БД",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void ShowResult(DataTable table, string gridTitle, string xmlPath, string status)
        {
            BindGrid(table);
            lblGrid.Text = gridTitle;
            txtXml.Text = ReadXmlText(xmlPath);
            lblXml.Text = "Вміст XML файлу: " + Path.GetFileName(xmlPath);
            SetStatus(status, Color.DarkGreen);
        }

        private void BindGrid(DataTable table)
        {
            grid.DataSource = null;
            grid.Columns.Clear();
            grid.DataSource = table;
        }

        private static string ReadXmlText(string path)
        {
            return File.ReadAllText(path).Replace("\r\n", "\n").Replace("\n", "\r\n");
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.ForeColor = color;
            lblStatus.Text = text;
        }

        private void ShowError(Exception ex)
        {
            SetStatus("Помилка: " + ex.Message, Color.DarkRed);
            MessageBox.Show(ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
