using System;
using System.Drawing;
using System.Windows.Forms;

namespace TennisApp
{
    public partial class MenuForm : Form
    {
        public MenuForm()
        {
            // Konfiguracja formularza
            Text = "Menu Główne";
            Width = 320;
            Height = 400;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;

            // Tytuł
            var lblTitle = new Label
            {
                Text = "TennisApp",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Top = 20,
                Left = 80,
                ForeColor = Color.DarkSlateBlue
            };
            Controls.Add(lblTitle);

            // Przycisk: Dodaj zawodnika
            var btnAddPlayer = new Button
            {
                Text = "Dodaj zawodnika",
                Width = 200,
                Height = 40,
                Left = 50,
                Top = 70,
                BackColor = Color.LightSteelBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnAddPlayer.Click += (s, e) =>
            {
                var form = new MainForm();
                form.ShowDialog();
            };
            Controls.Add(btnAddPlayer);

            // Przycisk: Lista zawodników
            var btnViewPlayers = new Button
            {
                Text = "Lista zawodników",
                Width = 200,
                Height = 40,
                Left = 50,
                Top = 120,
                BackColor = Color.LightSteelBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnViewPlayers.Click += (s, e) =>
            {
                var form = new PlayerListForm();
                form.ShowDialog();
            };
            Controls.Add(btnViewPlayers);
            Button btnToggleTheme = new Button
            {
                Text = "Zmień motyw",
                Width = 200,
                Height = 40,
                Left = 50,
                Top = 270,
                BackColor = Color.LightSteelBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)

            };
            btnToggleTheme.Click += (s, e) => ThemeManager.ToggleTheme(this);
            Controls.Add(btnToggleTheme);

            Load += (s, e) => ThemeManager.ApplyTheme(this);

            var btnAddMatch = new Button { 
                Text = "Dodaj Mecz",
                Width = 200,
                Height = 40,
                Left = 50,
                Top = 170,
                BackColor = Color.LightSteelBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)


            };
            btnAddMatch.Click += (s, e) =>
            {
                var form = new AddMatchForm();
                form.ShowDialog();
            };

            Controls.Add(btnAddMatch);

            var btnListForm = new Button
            {
                Text = "Lista Meczy",
                Width = 200,
                Height = 40,
                Left = 50,
                Top = 220,
                BackColor = Color.LightSteelBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)


            };
            btnListForm.Click += (s, e) =>
            {
                var form = new MatchListForm();
                form.ShowDialog();
            };

            Controls.Add(btnListForm);
        }
    }
}
