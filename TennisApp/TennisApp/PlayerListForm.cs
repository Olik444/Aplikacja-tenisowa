using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace TennisApp
{
    public partial class PlayerListForm : Form
    {
        private DataGridView dgvPlayers = new DataGridView();
        private string connString = "Host=localhost;Port=5432;Username=postgres;Password=qwer;Database=tennisdb";

        public PlayerListForm()
        {
            Text = "Lista Zawodników";
            Width = 900;
            Height = 500;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;

            dgvPlayers.Dock = DockStyle.Fill;
            dgvPlayers.ReadOnly = true;
            dgvPlayers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPlayers.AllowUserToAddRows = false;
            dgvPlayers.AllowUserToDeleteRows = false;
            dgvPlayers.RowHeadersVisible = false;
            dgvPlayers.BackgroundColor = Color.WhiteSmoke;
            dgvPlayers.BorderStyle = BorderStyle.FixedSingle;
            dgvPlayers.GridColor = Color.LightGray;
            dgvPlayers.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvPlayers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvPlayers.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSteelBlue;
            dgvPlayers.EnableHeadersVisualStyles = false;
            dgvPlayers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;


            Controls.Add(dgvPlayers);

            Load += (s, e) => LoadPlayers();
            dgvPlayers.MouseDown += DgvPlayers_MouseDown;
        }

        private void LoadPlayers()
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            string query = @"
               SELECT 
    p.id,
    p.first_name AS ""Imię"",
    p.last_name AS ""Nazwisko"",
    p.birth_date AS ""Data Urodzenia"",
    c.name AS ""Kraj"",
    p.ranking AS ""Ranking"",
    CASE WHEN p.is_active THEN 'Tak' ELSE 'Nie' END AS ""Aktywny""
FROM players p
JOIN countries c ON p.country_id = c.id;

            ";

            using var da = new NpgsqlDataAdapter(query, conn);
            var table = new DataTable();
            da.Fill(table);
            dgvPlayers.DataSource = table;
            foreach (DataGridViewColumn column in dgvPlayers.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Automatic;
            }

            dgvPlayers.Columns["id"].Visible = false;
        }

        private void DgvPlayers_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hit = dgvPlayers.HitTest(e.X, e.Y);
                if (hit.RowIndex >= 0)
                {
                    dgvPlayers.ClearSelection();
                    dgvPlayers.Rows[hit.RowIndex].Selected = true;

                    var menu = new ContextMenuStrip();
                    menu.Items.Add("✏️ Edytuj zawodnika", null, (s, ev) => EditPlayer());
                    menu.Items.Add("🗑️ Usuń zawodnika", null, (s, ev) => DeletePlayer());
                    menu.Show(dgvPlayers, e.Location);
                }
            }
        }

        private void EditPlayer()
        {
            var row = dgvPlayers.SelectedRows[0];
            int playerId = Convert.ToInt32(row.Cells["id"].Value);

            var form = new EditPlayerForm(playerId);
            form.FormClosed += (s, e) => LoadPlayers();
            form.ShowDialog();
        }

        private void DeletePlayer()
        {
            var row = dgvPlayers.SelectedRows[0];
            int playerId = Convert.ToInt32(row.Cells["id"].Value);
            string name = $"{row.Cells["Imię"].Value} {row.Cells["Nazwisko"].Value}";

            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            // Sprawdzenie powiązanych meczów
            using var checkCmd = new NpgsqlCommand(@"
        SELECT COUNT(*) FROM matches 
        WHERE player1_id = @id OR player2_id = @id OR winner_id = @id
    ", conn);
            checkCmd.Parameters.AddWithValue("@id", playerId);
            long matchCount = (long)checkCmd.ExecuteScalar();

            if (matchCount > 0)
            {
                MessageBox.Show($"Nie można usunąć zawodnika {name}, ponieważ uczestniczy on w {matchCount} meczu(-ach).",
                    "Błąd usuwania", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"Czy na pewno chcesz usunąć zawodnika:\n{name}?",
                "Potwierdzenie", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                using var deleteCmd = new NpgsqlCommand("DELETE FROM players WHERE id = @id", conn);
                deleteCmd.Parameters.AddWithValue("@id", playerId);
                deleteCmd.ExecuteNonQuery();

                LoadPlayers();
                MessageBox.Show("Zawodnik usunięty.", "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
