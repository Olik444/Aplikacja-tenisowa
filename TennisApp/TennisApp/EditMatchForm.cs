using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace TennisApp
{
    public partial class EditMatchForm : Form
    {
        private readonly int matchId;
        private string connString = "Host=localhost;Port=5432;Username=postgres;Password=qwer;Database=tennisdb";

        private ComboBox cbPlayer1 = new ComboBox();
        private ComboBox cbPlayer2 = new ComboBox();
        private ComboBox cbTournament = new ComboBox();
        private ComboBox cbWinner = new ComboBox();
        private DateTimePicker dpMatchDate = new DateTimePicker();
        private TextBox tbScore = new TextBox();
        private Button btnSave = new Button();

        public EditMatchForm(int matchId)
        {
            this.matchId = matchId;
            InitializeComponent();
            Load += EditMatchForm_Load;
        }

        private void EditMatchForm_Load(object sender, EventArgs e)
        {
            Text = "Edytuj mecz";
            Size = new System.Drawing.Size(400, 400);
            StartPosition = FormStartPosition.CenterParent;

            cbPlayer1.DropDownStyle = cbPlayer2.DropDownStyle = cbTournament.DropDownStyle = cbWinner.DropDownStyle = ComboBoxStyle.DropDownList;
            cbPlayer1.Width = cbPlayer2.Width = cbTournament.Width = cbWinner.Width = 250;
            tbScore.Width = 250;
            dpMatchDate.Width = 250;

            btnSave.Text = "Zapisz zmiany";
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
            panel.Controls.Add(new Label { Text = "Wynik (np. 6:4, 3:6):" });
            panel.Controls.Add(tbScore);
            panel.Controls.Add(btnSave);

            Controls.Add(panel);

            LoadComboData();
            LoadMatchData();
        }

        private void LoadComboData()
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            var daPlayers = new NpgsqlDataAdapter("SELECT id, first_name || ' ' || last_name AS name FROM players ORDER BY last_name", conn);
            var dtPlayers = new DataTable();
            daPlayers.Fill(dtPlayers);

            cbPlayer1.DataSource = dtPlayers.Copy();
            cbPlayer1.DisplayMember = cbPlayer2.DisplayMember = cbWinner.DisplayMember = "name";
            cbPlayer1.ValueMember = cbPlayer2.ValueMember = cbWinner.ValueMember = "id";
            cbPlayer2.DataSource = dtPlayers.Copy();
            cbWinner.DataSource = dtPlayers.Copy();

            var daTournaments = new NpgsqlDataAdapter("SELECT id, name FROM tournaments ORDER BY start_date DESC", conn);
            var dtTournaments = new DataTable();
            daTournaments.Fill(dtTournaments);
            cbTournament.DataSource = dtTournaments;
            cbTournament.DisplayMember = "name";
            cbTournament.ValueMember = "id";
        }

        private void LoadMatchData()
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            var cmd = new NpgsqlCommand("SELECT * FROM matches WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", matchId);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                cbPlayer1.SelectedValue = reader["player1_id"];
                cbPlayer2.SelectedValue = reader["player2_id"];
                cbWinner.SelectedValue = reader["winner_id"];
                cbTournament.SelectedValue = reader["tournament_id"];
                dpMatchDate.Value = Convert.ToDateTime(reader["match_date"]);
                tbScore.Text = reader["score"].ToString();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cbPlayer1.SelectedValue.Equals(cbPlayer2.SelectedValue))
            {
                MessageBox.Show("Zawodnik nie może grać sam ze sobą.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int player1 = (int)cbPlayer1.SelectedValue;
            int player2 = (int)cbPlayer2.SelectedValue;
            int winner = (int)cbWinner.SelectedValue;
            int tournament = (int)cbTournament.SelectedValue;
            DateTime date = dpMatchDate.Value.Date;
            string score = tbScore.Text.Trim();

            if (winner != player1 && winner != player2)
            {
                MessageBox.Show("Zwycięzca musi być jednym z zawodników.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var sets = score.Split(',');
            if (sets.Length == 0 || sets.Length > 5)
            {
                MessageBox.Show("Nieprawidłowa liczba setów (1-5).", "Błąd wyniku", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var set in sets)
            {
                var parts = set.Trim().Split(':');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int a) || !int.TryParse(parts[1], out int b))
                {
                    MessageBox.Show("Nieprawidłowy format setu. Poprawny to np. 6:4", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if ((a < 0 || b < 0) || (a < 6 && b < 6))
                {
                    MessageBox.Show("Nieprawidłowy set: " + set, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                UPDATE matches
                SET player1_id = @p1, player2_id = @p2, winner_id = @win,
                    tournament_id = @tid, match_date = @date, score = @score
                WHERE id = @id
            ", conn);

            cmd.Parameters.AddWithValue("@id", matchId);
            cmd.Parameters.AddWithValue("@p1", player1);
            cmd.Parameters.AddWithValue("@p2", player2);
            cmd.Parameters.AddWithValue("@win", winner);
            cmd.Parameters.AddWithValue("@tid", tournament);
            cmd.Parameters.AddWithValue("@date", date);
            cmd.Parameters.AddWithValue("@score", score);

            try
            {
                cmd.ExecuteNonQuery();
                MessageBox.Show("Mecz zaktualizowany pomyślnie!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd: " + ex.Message);
            }
        }
    }
}
