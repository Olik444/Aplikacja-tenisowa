using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Npgsql;

namespace TennisApp
{
    public class EditPlayerForm : Form
    {
        private TextBox txtFirstName = new TextBox { Left = 120, Top = 20, Width = 200 };
        private TextBox txtLastName = new TextBox { Left = 120, Top = 60, Width = 200 };
        private DateTimePicker dtpBirthDate = new DateTimePicker { Left = 120, Top = 100, Width = 200 };
        private ComboBox cmbCountry = new ComboBox { Left = 120, Top = 140, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        private NumericUpDown nudRanking = new NumericUpDown { Left = 120, Top = 180, Width = 200, Minimum = 1, Maximum = 1000 };
        private CheckBox chkIsActive = new CheckBox { Left = 120, Top = 220, Text = "Aktywny" };
        private Button btnSave = new Button { Left = 120, Top = 260, Text = "Zapisz zmiany", Width = 200 };

        private readonly int playerId;
        private readonly string connString = "Host=localhost;Port=5432;Username=postgres;Password=qwer;Database=tennisdb";

        public EditPlayerForm(int playerId)
        {
            this.playerId = playerId;
            Text = "Edytuj Zawodnika";
            Width = 400;
            Height = 370;

            Controls.AddRange(new Control[] {
                new Label { Text = "Imię:", Left = 20, Top = 20 },
                txtFirstName,
                new Label { Text = "Nazwisko:", Left = 20, Top = 60 },
                txtLastName,
                new Label { Text = "Data urodzenia:", Left = 20, Top = 100 },
                dtpBirthDate,
                new Label { Text = "Kraj:", Left = 20, Top = 140 },
                cmbCountry,
                new Label { Text = "Ranking:", Left = 20, Top = 180 },
                nudRanking,
                chkIsActive,
                btnSave
            });

            Load += (s, e) => LoadPlayerData();
            btnSave.Click += (s, e) => SaveChanges();
        }

        private void LoadPlayerData()
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            // Załaduj kraje
            var countries = new List<Country>();
            using (var cmd = new NpgsqlCommand("SELECT id, name FROM countries", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    countries.Add(new Country { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                }
            }
            cmbCountry.DataSource = countries;

            // Załaduj dane zawodnika
            using var cmd2 = new NpgsqlCommand("SELECT first_name, last_name, birth_date, country_id, ranking, is_active FROM players WHERE id = @id", conn);
            cmd2.Parameters.AddWithValue("@id", playerId);
            using var reader2 = cmd2.ExecuteReader();
            if (reader2.Read())
            {
                txtFirstName.Text = reader2.GetString(0);
                txtLastName.Text = reader2.GetString(1);
                dtpBirthDate.Value = reader2.GetDateTime(2);
                cmbCountry.SelectedIndex = countries.FindIndex(c => c.Id == reader2.GetInt32(3));
                nudRanking.Value = reader2.GetInt32(4);
                chkIsActive.Checked = reader2.GetBoolean(5);
            }
        }

        private void SaveChanges()
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"UPDATE players 
                SET first_name = @f, last_name = @l, birth_date = @b, country_id = @c, ranking = @r, is_active = @a 
                WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("@f", txtFirstName.Text);
            cmd.Parameters.AddWithValue("@l", txtLastName.Text);
            cmd.Parameters.AddWithValue("@b", dtpBirthDate.Value.Date);
            cmd.Parameters.AddWithValue("@c", ((Country)cmbCountry.SelectedItem).Id);
            cmd.Parameters.AddWithValue("@r", (int)nudRanking.Value);
            cmd.Parameters.AddWithValue("@a", chkIsActive.Checked);
            cmd.Parameters.AddWithValue("@id", playerId);

            cmd.ExecuteNonQuery();
            MessageBox.Show("Zapisano zmiany.");
            Close();
        }
    }
}
