using MySql.Data.MySqlClient;

namespace Autók
{
    public partial class Form1 : Form
    {
        public MySqlConnectionStringBuilder builder;
        public MySqlCommand cmd;
        public MySqlConnection conn;

        public int ÖsszesSor = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form_onLoad(object sender, EventArgs e)
        {
            builder = new MySqlConnectionStringBuilder();
            builder.Server = "localhost";
            builder.UserID = "root";
            builder.Password = "root";
            builder.Database = "autok";
            conn = new MySqlConnection(builder.ConnectionString);
            try
            {
                conn.Open();
                cmd = conn.CreateCommand();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + "A program leáll", "HIBA", MessageBoxButtons.OK);
                Environment.Exit(0);
            }
            finally { conn.Close(); }

            cmd.CommandText = "SELECT COUNT(*) FROM autok";
            conn.Open();
            int count = Int32.Parse(cmd.ExecuteScalar().ToString());
            ÖsszesSor = count;
            for (int i = 1; i <= count; i++)
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM autok WHERE id = @id";
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = i;
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    AutóLista.Items.Add($"{reader["id"]}:{reader["gyarto"]}");
                }
                reader.Close();
                cmd.Cancel();
            }
            conn.Close();
        }

        private void AutóLista_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AutóLista.SelectedIndex == -1)
            {
                return;
            }
            conn.Open();
            cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM autok WHERE id = @id";
            string[] sorok = this.AutóLista.Items[AutóLista.SelectedIndex].ToString().Split(":");
            IdTextbox.Text = sorok[0];
            cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = sorok[0];
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                GyártóTextbox.Text = reader["gyarto"].ToString();
                ÉvjáratTextbox.Text = reader["evjarat"].ToString();
                SzínTextbox.Text = reader["szin"].ToString();
                ÁrTextbox.Text = reader["ar"].ToString();
            }
            reader.Close();
            conn.Close();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void InsertButton_Clicked(object sender, EventArgs e)
        {
            if (IdTextbox.Text != "")
            {
                MessageBox.Show("Az ID hozzárendelés autómatikus! Nem kell megadni!", "FELHÍVÁS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            if (GyártóTextbox.Text == "" || ÉvjáratTextbox.Text == "" || SzínTextbox.Text == "" || ÁrTextbox.Text == "")
            {
                MessageBox.Show("Az ID-n kívűl minden adatot meg kell adni!", "HIBA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO autok (id, gyarto, evjarat, szin, ar) VALUES (DEFAULT, @gyarto, @evjarat, @szin, @ar)";
            cmd.Parameters.Add("@gyarto", MySqlDbType.VarChar).Value = GyártóTextbox.Text;
            cmd.Parameters.Add("@evjarat", MySqlDbType.VarChar).Value = ÉvjáratTextbox.Text;
            cmd.Parameters.Add("@szin", MySqlDbType.VarString).Value = SzínTextbox.Text;
            cmd.Parameters.Add("@ar", MySqlDbType.Int32).Value = ÁrTextbox.Text;
            conn.Open();
            try
            {
                cmd.ExecuteNonQuery();
                ÖsszesSor++;
                AutóLista.Items.Add($"{ÖsszesSor}:{GyártóTextbox.Text}");
                MessageBox.Show("Az adatok bevitele sikeres volt!", "SIKERES MŰVELET", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Valamilyen hiba történt! ", "HIBA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally { conn.Close(); }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE autok SET gyarto = @gyarto, evjarat = @evjarat, szin = @szin, ar = @ar WHERE id = @id";
            cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = AutóLista.Items[AutóLista.SelectedIndex].ToString().Split(":")[0];
            cmd.Parameters.Add("@gyarto", MySqlDbType.VarChar).Value = GyártóTextbox.Text;
            cmd.Parameters.Add("@evjarat", MySqlDbType.VarChar).Value = ÉvjáratTextbox.Text;
            cmd.Parameters.Add("@szin", MySqlDbType.VarString).Value = SzínTextbox.Text;
            cmd.Parameters.Add("@ar", MySqlDbType.Int32).Value = ÁrTextbox.Text;
            if (AutóLista.Items[AutóLista.SelectedIndex].ToString().Split(":")[0] != IdTextbox.Text)
            {
                MessageBox.Show("Az ID nem egyezik meg a kiválaszott ID-val!", "HIBA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            conn.Open();
            try
            {
                cmd.ExecuteNonQuery();
                conn.Close();
                AutóLista.Items[AutóLista.SelectedIndex] = $"{IdTextbox.Text}:{GyártóTextbox.Text}";
                MessageBox.Show("Az adatok bevitele sikeres volt!", "SIKERES MŰVELET", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Valamilyen hiba történt!", "HIBA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally { conn.Close(); }
        }

        private void DeleteButon_Click(object sender, EventArgs e)
        {
            DialogResult option = MessageBox.Show("Biztos szeretné törölni?", "FELHIVÁS", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (option == DialogResult.Yes)
            {
                conn.Open();
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM autok WHERE id = @id";
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = AutóLista.Items[AutóLista.SelectedIndex].ToString().Split(":")[0];
                cmd.ExecuteNonQuery();
                AutóLista.Items.RemoveAt(AutóLista.SelectedIndex);
                conn.Close();
            }
            else { return; }
        }
    }
}