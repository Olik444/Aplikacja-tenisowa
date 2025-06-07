using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace TennisApp
{
    public class Player
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public int CountryId { get; set; }
        public int Ranking { get; set; }
        public bool IsActive { get; set; }
    }

    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public override string ToString() => Name;
    }

    public partial class MainForm : Form
    {
        private TextBox txtFirstName = new TextBox();
        private TextBox txtLastName = new TextBox();
        private DateTimePicker dtpBirthDate = new DateTimePicker();
        private ComboBox cmbCountry = new ComboBox();
        private NumericUpDown nudRanking = new NumericUpDown();
        private CheckBox chkIsActive = new CheckBox();
        private Button btnAdd = new Button();


        private string connString = "Host=localhost;Port=5432;Username=postgres;Password=qwer;Database=tennisdb";
        private bool HasDigits(string text)
        {
            foreach (char c in text)
            {
                if (char.IsDigit(c))
                    return true;
            }
            return false;
        }
        private bool isDarkMode = false;
        private Button btnToggleTheme = new Button();

        private bool RankingExists(int ranking)
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM players WHERE ranking = @r", conn);
            cmd.Parameters.AddWithValue("@r", ranking);
            long count = (long)cmd.ExecuteScalar();
            return count > 0;
        }

        public MainForm()
        {
            Text = "Dodaj Zawodnika";
            Width = 400;
            Height = 520;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            btnToggleTheme.Text = "Zmień motyw";
            btnToggleTheme.Width = 200;
            btnToggleTheme.Height = 40;
            btnToggleTheme.Left = 100;
            btnToggleTheme.Top = 390;
            btnToggleTheme.BackColor = Color.LightGray;
            btnToggleTheme.FlatStyle = FlatStyle.Flat;
            btnToggleTheme.Font = new Font("Segoe UI", 10);
            btnToggleTheme.Click += (s, e) => ToggleTheme();
            Controls.Add(btnToggleTheme);

            Label lblTitle = new Label
            {
                Text = "Formularz Zawodnika",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(110, 10),
                ForeColor = Color.DarkSlateBlue
            };

            Controls.Add(lblTitle);

            AddLabeledControl("Imię:", txtFirstName, 50);
            AddLabeledControl("Nazwisko:", txtLastName, 90);
            AddLabeledControl("Data urodzenia:", dtpBirthDate, 130);
            AddLabeledControl("Kraj:", cmbCountry, 170);
            AddLabeledControl("Ranking:", nudRanking, 210);

            cmbCountry.DropDownStyle = ComboBoxStyle.DropDownList;
            nudRanking.Minimum = 1;
            nudRanking.Maximum = 1000;

            chkIsActive.Text = "Aktywny";
            chkIsActive.Left = 120;
            chkIsActive.Top = 250;
            Controls.Add(chkIsActive);



            btnAdd.Text = "Dodaj zawodnika";
            btnAdd.Width = 200;
            btnAdd.Height = 40;
            btnAdd.Left = 100;
            btnAdd.Top = 290;
            btnAdd.BackColor = Color.LightSteelBlue;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Font = new Font("Segoe UI", 10);
            btnAdd.Click += (s, e) => AddPlayer();
            Controls.Add(btnAdd);
            var btnManageCountries = new Button
            {
                Text = "Zarządzaj krajami",
                Width = 200,
                Height = 40,
                Left = 100,
                Top = 340,
                BackColor = Color.LightGreen,
                Font = new Font("Segoe UI", 10)
            };

            btnManageCountries.Click += (s, e) =>
            {
                var countryForm = new CountryForm();
                countryForm.ShowDialog();
                LoadCountries(); // Odśwież listę krajów po zamknięciu formularza
            };

            Controls.Add(btnManageCountries);

            Load += (s, e) => LoadCountries();
        }

        private void AddLabeledControl(string labelText, Control control, int top)
        {
            var label = new Label
            {
                Text = labelText,
                Left = 20,
                Top = top + 5,
                Width = 100,
                Font = new Font("Segoe UI", 9)
            };

            control.Left = 120;
            control.Top = top;
            control.Width = 200;

            Controls.Add(label);
            Controls.Add(control);
        }

        private void LoadCountries()
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT id, name FROM countries", conn);
            using var reader = cmd.ExecuteReader();
            var countries = new List<Country>();
            while (reader.Read())
            {
                countries.Add(new Country { Id = reader.GetInt32(0), Name = reader.GetString(1) });
            }
            cmbCountry.DataSource = countries;
        }

        private void AddPlayer()
        {
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();

            // 1. Walidacja pustych pól
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Imię i nazwisko są wymagane.");
                return;
            }

            // 2. Walidacja: brak cyfr w imieniu/nazwisku
            if (HasDigits(firstName) || HasDigits(lastName))
            {
                MessageBox.Show("Imię i nazwisko nie mogą zawierać cyfr.");
                return;
            }

            int ranking = (int)nudRanking.Value;

            // 3. Sprawdzenie czy ranking już istnieje
            if (RankingExists(ranking))
            {
                MessageBox.Show($"Zawodnik z rankingiem {ranking} już istnieje.\nWybierz inny ranking.");
                return;
            }

            // 4. Utwórz zawodnika
            var player = new Player
            {
                FirstName = firstName,
                LastName = lastName,
                BirthDate = dtpBirthDate.Value.Date,
                CountryId = ((Country)cmbCountry.SelectedItem).Id,
                Ranking = ranking,
                IsActive = chkIsActive.Checked
            };

            // 5. Dodanie zawodnika do bazy
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            using var cmd = new NpgsqlCommand("INSERT INTO players (first_name, last_name, birth_date, country_id, ranking, is_active) VALUES (@f, @l, @b, @c, @r, @a)", conn);
            cmd.Parameters.AddWithValue("@f", player.FirstName);
            cmd.Parameters.AddWithValue("@l", player.LastName);
            cmd.Parameters.AddWithValue("@b", player.BirthDate);
            cmd.Parameters.AddWithValue("@c", player.CountryId);
            cmd.Parameters.AddWithValue("@r", player.Ranking);
            cmd.Parameters.AddWithValue("@a", player.IsActive);
            cmd.ExecuteNonQuery();

            MessageBox.Show("Zawodnik dodany pomyślnie.");
        }
        private void ToggleTheme()
        {
            isDarkMode = !isDarkMode;

            BackColor = isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;

            foreach (Control ctrl in Controls)
            {
                if (ctrl is Label)
                {
                    ctrl.ForeColor = isDarkMode ? Color.White : Color.Black;
                }
                else if (ctrl is TextBox or ComboBox or NumericUpDown or DateTimePicker)
                {
                    ctrl.BackColor = isDarkMode ? Color.FromArgb(45, 45, 45) : Color.White;
                    ctrl.ForeColor = isDarkMode ? Color.White : Color.Black;
                }
                else if (ctrl is Button btn)
                {
                    btn.BackColor = isDarkMode ? Color.DimGray : Color.LightSteelBlue;
                    btn.ForeColor = isDarkMode ? Color.White : Color.Black;
                }
                else if (ctrl is CheckBox cb)
                {
                    cb.ForeColor = isDarkMode ? Color.White : Color.Black;
                }
            }
        }


        private void ClearForm()
        {
            txtFirstName.Clear();
            txtLastName.Clear();
            dtpBirthDate.Value = DateTime.Today;
            cmbCountry.SelectedIndex = 0;
            nudRanking.Value = 1;
            chkIsActive.Checked = false;
        }
    }
}
