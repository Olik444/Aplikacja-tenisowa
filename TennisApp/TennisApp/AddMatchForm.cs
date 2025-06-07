using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace TennisApp
{
    public partial class AddMatchForm : Form
    {
        private string connString = "Host=localhost;Port=5432;Username=postgres;Password=qwer;Database=tennisdb";

        public AddMatchForm()
        {
            InitializeComponent();
            Load += AddMatchForm_Load;
        }

        private ComboBox cbPlayer1 = new ComboBox();
        private ComboBox cbPlayer2 = new ComboBox();
        private ComboBox cbTournament = new ComboBox();
        private ComboBox cbWinner = new ComboBox();
        private DateTimePicker dpMatchDate = new DateTimePicker();
        private TextBox tbScore = new TextBox();
        private Button btnSave = new Button();

        private void AddMatchForm_Load(object sender, EventArgs e)
        {
            Text = "Dodaj mecz";
            Size = new System.Drawing.Size(400, 400);
            StartPosition = FormStartPosition.CenterParent;

            cbPlayer1.DropDownStyle = cbPlayer2.DropDownStyle = cbTournament.DropDownStyle = cbWinner.DropDownStyle = ComboBoxStyle.DropDownList;
            cbPlayer1.Width = cbPlayer2.Width = cbTournament.Width = cbWinner.Width = 250;
            tbScore.Width = 250;
            dpMatchDate.Width = 250;

            btnSave.Text = "Zapisz mecz";
            btnSave.Click += BtnSave_Click;

            FlowLayoutPanel panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
            panel.Controls.Add(new Label { Text = "Zawodnik 1:" });
            panel.Controls.Add(cbPlayer1);
            panel.Controls.Add(new Label { Text = "Zawodnik 2:" });
            panel.Controls.Add(cbPlayer2);
            panel.Controls.Add(new Label { Text = "Zwycięzca:" });
            panel.Controls.Add(cbWinner);
            panel.Controls.Add(new Label { Text = "Turniej:" });
            panel.Controls.Add(cbTournament);
            panel.Controls.Add(new Label { Text = "Data meczu:" });
            panel.Controls.Add(dpMatchDate);
            panel.Controls.Add(new Label { Text = "Wynik (np. 6:4, 3:6, 7:5):" });
            panel.Controls.Add(tbScore);
            panel.Controls.Add(btnSave);

            Controls.Add(panel);

            LoadComboData();
        }

        private void LoadComboData()
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            // Zawodnicy
            var daPlayers = new NpgsqlDataAdapter("SELECT id, first_name || ' ' || last_name AS name FROM players ORDER BY last_name", conn);
            var dtPlayers = new DataTable();
            daPlayers.Fill(dtPlayers);
            cbPlayer1.DataSource = dtPlayers.Copy();
            cbPlayer1.DisplayMember = "name";
            cbPlayer1.ValueMember = "id";

            cbPlayer2.DataSource = dtPlayers.Copy();
            cbPlayer2.DisplayMember = "name";
            cbPlayer2.ValueMember = "id";

            cbWinner.DataSource = dtPlayers.Copy();
            cbWinner.DisplayMember = "name";
            cbWinner.ValueMember = "id";

            // Turnieje
            var daTournaments = new NpgsqlDataAdapter("SELECT id, name FROM tournaments ORDER BY start_date DESC", conn);
            var dtTournaments = new DataTable();
            daTournaments.Fill(dtTournaments);
            cbTournament.DataSource = dtTournaments;
            cbTournament.DisplayMember = "name";
            cbTournament.ValueMember = "id";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cbPlayer1.SelectedValue.Equals(cbPlayer2.SelectedValue))
            {
                MessageBox.Show("Zawodnik nie może grać sam ze sobą.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!cbWinner.SelectedValue.Equals(cbPlayer1.SelectedValue) && !cbWinner.SelectedValue.Equals(cbPlayer2.SelectedValue))
            {
                MessageBox.Show("Zwycięzca musi być jednym z zawodników grających w meczu.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            int player1 = (int)cbPlayer1.SelectedValue;
            int player2 = (int)cbPlayer2.SelectedValue;
            int winner = (int)cbWinner.SelectedValue;
            int tournament = (int)cbTournament.SelectedValue;
            DateTime date = dpMatchDate.Value;
            string score = tbScore.Text.Trim();

            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                INSERT INTO matches (tournament_id, player1_id, player2_id, match_date, winner_id, score)
                VALUES (@tid, @p1, @p2, @date, @win, @score)
            ", conn);

            cmd.Parameters.AddWithValue("@tid", tournament);
            cmd.Parameters.AddWithValue("@p1", player1);
            cmd.Parameters.AddWithValue("@p2", player2);
            cmd.Parameters.AddWithValue("@date", date);
            cmd.Parameters.AddWithValue("@win", winner);
            cmd.Parameters.AddWithValue("@score", score);

            try
            {
                // WALIDACJA: Czy zawodnik 1 gra już inny mecz tego dnia?
                var cmdCheck = new NpgsqlCommand(@"
        SELECT COUNT(*) FROM matches
        WHERE match_date = @date
        AND (player1_id = @p1 OR player2_id = @p1 OR player1_id = @p2 OR player2_id = @p2)
    ", conn);

                cmdCheck.Parameters.AddWithValue("@date", date.Date);
                cmdCheck.Parameters.AddWithValue("@p1", player1);
                cmdCheck.Parameters.AddWithValue("@p2", player2);

                var conflictCount = (long)cmdCheck.ExecuteScalar();

                if (conflictCount > 0)
                {
                    MessageBox.Show("Jeden z zawodników gra już mecz tego dnia.", "Konflikt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // WALIDACJA wyniku (np. "6:4, 3:6, 7:5")
                var sets = score.Split(',');
                if (sets.Length == 0 || sets.Length > 5)
                {
                    MessageBox.Show("Nieprawidłowa liczba setów (1-5).", "Błąd wyniku", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                foreach (var set in sets)
                {
                    var trimmed = set.Trim();
                    var parts = trimmed.Split(':');
                    if (parts.Length != 2 || !int.TryParse(parts[0], out int a) || !int.TryParse(parts[1], out int b))
                    {
                        MessageBox.Show("Nieprawidłowy format setów. Użyj np. \"6:4, 3:6\"", "Błąd wyniku", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Nie pozwalamy na niemożliwe wyniki
                    if ((a < 0 || b < 0) || (a < 6 && b < 6))
                    {
                        MessageBox.Show($"Nieprawidłowy set: {trimmed}. Wynik musi być np. 6:4.", "Błąd wyniku", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    
                }

                // Jeśli nie ma konfliktu — dodaj mecz
                cmd.ExecuteNonQuery();
                MessageBox.Show("Mecz dodany pomyślnie!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd: " + ex.Message);
            }

        }
    }
}
