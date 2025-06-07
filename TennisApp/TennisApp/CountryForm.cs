using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace TennisApp
{
    public partial class CountryForm : Form
    {
        private DataGridView dgvCountries = new DataGridView();
        private TextBox txtCountryName = new TextBox();
        private Button btnAddCountry = new Button();
        private Button btnDeleteCountry = new Button();

        private string connString = "Host=localhost;Port=5432;Username=postgres;Password=qwer;Database=tennisdb";

        public CountryForm()
        {
            Text = "Zarządzanie Krajami";
            Width = 500;
            Height = 400;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;

            dgvCountries.Dock = DockStyle.Top;
            dgvCountries.Height = 250;
            dgvCountries.ReadOnly = true;
            dgvCountries.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCountries.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            txtCountryName.Top = 270;
            txtCountryName.Left = 20;
            txtCountryName.Width = 200;

            btnAddCountry.Text = "Dodaj kraj";
            btnAddCountry.Top = 270;
            btnAddCountry.Left = 240;
            btnAddCountry.Click += (s, e) => AddCountry();

            btnDeleteCountry.Text = "Usuń wybrany kraj";
            btnDeleteCountry.Top = 310;
            btnDeleteCountry.Left = 20;
            btnDeleteCountry.Width = 200;
            btnDeleteCountry.Click += (s, e) => DeleteSelectedCountry();

            Controls.Add(dgvCountries);
            Controls.Add(txtCountryName);
            Controls.Add(btnAddCountry);
            Controls.Add(btnDeleteCountry);


            Load += (s, e) => LoadCountries();
        }

        private void LoadCountries()
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            using var da = new NpgsqlDataAdapter("SELECT id, name AS \"Nazwa\" FROM countries ORDER BY name", conn);
            var table = new DataTable();
            da.Fill(table);
            dgvCountries.DataSource = table;
            dgvCountries.Columns["id"].Visible = false;
        }

        private void AddCountry()
        {
            string name = txtCountryName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Podaj nazwę kraju.");
                return;
            }

            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            using var cmd = new NpgsqlCommand("INSERT INTO countries (name) VALUES (@n)", conn);
            cmd.Parameters.AddWithValue("@n", name);
            cmd.ExecuteNonQuery();

            MessageBox.Show("Kraj dodany.");
            txtCountryName.Clear();
            LoadCountries();
        }

        private void DeleteSelectedCountry()
        {
            if (dgvCountries.SelectedRows.Count == 0)
            {
                MessageBox.Show("Wybierz kraj do usunięcia.");
                return;
            }

            var row = dgvCountries.SelectedRows[0];
            int countryId = (int)row.Cells["id"].Value;
            string name = row.Cells["Nazwa"].Value.ToString();

            var confirm = MessageBox.Show($"Czy na pewno chcesz usunąć kraj:\n{name}?", "Potwierdzenie", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
                return;

            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            // Sprawdź, czy jakiś zawodnik używa tego kraju
            using var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM players WHERE country_id = @id", conn);
            checkCmd.Parameters.AddWithValue("@id", countryId);
            var count = (long)checkCmd.ExecuteScalar();
            if (count > 0)
            {
                MessageBox.Show("Nie można usunąć kraju, który jest przypisany do zawodników.");
                return;
            }

            using var delCmd = new NpgsqlCommand("DELETE FROM countries WHERE id = @id", conn);
            delCmd.Parameters.AddWithValue("@id", countryId);
            delCmd.ExecuteNonQuery();

            MessageBox.Show("Kraj usunięty.");
            LoadCountries();
        }
    }
}
