using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace TennisApp
{
    public partial class MatchListForm : Form
    {
        private DataGridView dgvMatches = new DataGridView();
        private string connString = "Host=localhost;Port=5432;Username=postgres;Password=qwer;Database=tennisdb";

        public MatchListForm()
        {
            Text = "Lista Meczów";
            Width = 1000;
            Height = 500;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;

            dgvMatches.Dock = DockStyle.Fill;
            dgvMatches.ReadOnly = true;
            dgvMatches.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMatches.AllowUserToAddRows = false;
            dgvMatches.AllowUserToDeleteRows = false;
            dgvMatches.RowHeadersVisible = false;
            dgvMatches.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMatches.DefaultCellStyle.Font = new Font("Segoe UI", 8);
            dgvMatches.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8, FontStyle.Bold);

            Controls.Add(dgvMatches);

            Load += (s, e) => LoadMatches();
            dgvMatches.MouseDown += DgvMatches_MouseDown;

        }

        private void LoadMatches()
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            string query = @"
                SELECT 
                    m.id,
                    t.name AS ""Turniej"",
                    p1.first_name || ' ' || p1.last_name AS ""Zawodnik 1"",
                    p2.first_name || ' ' || p2.last_name AS ""Zawodnik 2"",
                    p3.first_name || ' ' || p3.last_name AS ""Zwycięzca"",
                    m.match_date AS ""Data Meczu"",
                    m.score AS ""Wynik""
                FROM matches m
                JOIN players p1 ON m.player1_id = p1.id
                JOIN players p2 ON m.player2_id = p2.id
                JOIN players p3 ON m.winner_id = p3.id
                JOIN tournaments t ON m.tournament_id = t.id
                ORDER BY m.match_date DESC;
            ";

            var da = new NpgsqlDataAdapter(query, conn);
            var dt = new DataTable();
            da.Fill(dt);
            dgvMatches.DataSource = dt;

            dgvMatches.Columns["id"].Visible = false;
        }

        private void DgvMatches_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hit = dgvMatches.HitTest(e.X, e.Y);
                if (hit.RowIndex >= 0)
                {
                    dgvMatches.ClearSelection();
                    dgvMatches.Rows[hit.RowIndex].Selected = true;

                    var menu = new ContextMenuStrip();
                    menu.Items.Add("✏️ Edytuj mecz", null, (s, ev) => EditMatch());
                    menu.Items.Add("🗑️ Usuń mecz", null, (s, ev) => DeleteMatch());

                    menu.Show(dgvMatches, e.Location);
                }
            }
        }
        private void DeleteMatch()
        {
            var row = dgvMatches.SelectedRows[0];
            int matchId = Convert.ToInt32(row.Cells["id"].Value);

            var result = MessageBox.Show("Czy na pewno chcesz usunąć ten mecz?", "Potwierdzenie", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                using var conn = new NpgsqlConnection(connString);
                conn.Open();
                using var cmd = new NpgsqlCommand("DELETE FROM matches WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", matchId);

                try
                {
                    cmd.ExecuteNonQuery();
                    LoadMatches(); 
                    MessageBox.Show("Mecz został usunięty.", "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd podczas usuwania meczu:\n" + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void EditMatch()
        {
            var row = dgvMatches.SelectedRows[0];
            int matchId = Convert.ToInt32(row.Cells["id"].Value);

            var form = new EditMatchForm(matchId);
            form.FormClosed += (s, e) => LoadMatches();
            form.ShowDialog();
        }

    }
}
