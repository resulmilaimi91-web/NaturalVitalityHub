using System.Data;
using System.Data.SQLite;
using ArmepunesApp.Data;

namespace ArmepunesApp.Services;

public static class BackupHelper
{
    public static string GetBackupDir()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ArmepunesApp", "Backup");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return dir;
    }

    public static string EksportoKlientNeDb(string klientiEmri, int klientiId, DatabaseHelper db)
    {
        var backupDir = GetBackupDir();
        var klientiDir = Path.Combine(backupDir, "Klientet", SanitizeFileName(klientiEmri));
        if (!Directory.Exists(klientiDir)) Directory.CreateDirectory(klientiDir);

        var dbPath = Path.Combine(klientiDir, $"TeDhenat_{SanitizeFileName(klientiEmri)}.sqlite");

        using var conn = new SQLiteConnection(DatabaseHelper.KrijoConnectionString(dbPath));
        conn.Open();

        string sql = @"
            CREATE TABLE IF NOT EXISTS Klienti (
                Id INTEGER PRIMARY KEY,
                Emri TEXT, Mbiemri TEXT, Adresa TEXT, Telefon TEXT,
                Email TEXT, NrLeternjoftimit TEXT, Shenime TEXT
            );
            CREATE TABLE IF NOT EXISTS Armet (
                Id INTEGER PRIMARY KEY,
                NumerSerial TEXT, Lloji TEXT, Marka TEXT, Modeli TEXT,
                Kalibri TEXT, Statusi TEXT, NrInventari TEXT
            );
            CREATE TABLE IF NOT EXISTS Transaksionet (
                Id INTEGER PRIMARY KEY,
                ArmaId INTEGER, PersoneliId INTEGER, KlientiId INTEGER,
                Tipi TEXT, DataOra TEXT, Qellimi TEXT,
                PersoneliQeDorzoi TEXT, PersoneliQeMorri TEXT, Shenime TEXT,
                ArmaSerial TEXT, PersoneliEmri TEXT
            );
            CREATE TABLE IF NOT EXISTS Aksesoret (
                Id INTEGER PRIMARY KEY,
                TransaksioniId INTEGER, Emri TEXT, Sasia INTEGER, Shenime TEXT
            );";
        using var cmdCreate = new SQLiteCommand(sql, conn);
        cmdCreate.ExecuteNonQuery();

        // Insert client data
        using (var cmd = new SQLiteCommand("DELETE FROM Klienti", conn))
            cmd.ExecuteNonQuery();
        var klientet = db.MerrKlientet();
        var klRow = klientet.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["Id"]) == klientiId);
        if (klRow != null)
        {
            using var cmd = new SQLiteCommand(
                "INSERT INTO Klienti (Id, Emri, Mbiemri, Adresa, Telefon, Email, NrLeternjoftimit, Shenime) VALUES (@id,@e,@m,@a,@t,@em,@nl,@sh)", conn);
            cmd.Parameters.AddWithValue("@id", klRow["Id"]);
            cmd.Parameters.AddWithValue("@e", klRow["Emri"]?.ToString() ?? "");
            cmd.Parameters.AddWithValue("@m", klRow["Mbiemri"]?.ToString() ?? "");
            cmd.Parameters.AddWithValue("@a", klRow["Adresa"]?.ToString() ?? "");
            cmd.Parameters.AddWithValue("@t", klRow["Telefon"]?.ToString() ?? "");
            cmd.Parameters.AddWithValue("@em", klRow["Email"]?.ToString() ?? "");
            cmd.Parameters.AddWithValue("@nl", klRow["NrLeternjoftimit"]?.ToString() ?? "");
            cmd.Parameters.AddWithValue("@sh", klRow["Shenime"]?.ToString() ?? "");
            cmd.ExecuteNonQuery();
        }

        // Load all transactions for this client
        using var cmdDel = new SQLiteCommand("DELETE FROM Transaksionet", conn);
        cmdDel.ExecuteNonQuery();
        using var cmdDel2 = new SQLiteCommand("DELETE FROM Aksesoret", conn);
        cmdDel2.ExecuteNonQuery();
        using var cmdDel3 = new SQLiteCommand("DELETE FROM Armet", conn);
        cmdDel3.ExecuteNonQuery();

        var trans = db.MerrTransaksionet();
        var klRows = trans.AsEnumerable()
            .Where(r => (r["KlientiEmri"]?.ToString() ?? "").IndexOf(klientiEmri, StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();

        var armSerialet = new HashSet<string>();
        foreach (var r in klRows)
        {
            var serial = r["ArmaSerial"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(serial)) armSerialet.Add(serial);

            using var cmd = new SQLiteCommand(
                "INSERT INTO Transaksionet (Id, ArmaId, PersoneliId, KlientiId, Tipi, DataOra, Qellimi, PersoneliQeDorzoi, PersoneliQeMorri, Shenime, ArmaSerial, PersoneliEmri) VALUES (@id,@ai,@pi,@ki,@tip,@do,@q,@pd,@pm,@sh,@as,@pe)", conn);
            cmd.Parameters.AddWithValue("@id", r["Id"]);
            cmd.Parameters.AddWithValue("@ai", r["ArmaId"]);
            cmd.Parameters.AddWithValue("@pi", r["PersoneliId"]);
            cmd.Parameters.AddWithValue("@ki", r["KlientiId"]);
            cmd.Parameters.AddWithValue("@tip", r["Tipi"]);
            cmd.Parameters.AddWithValue("@do", r["DataOra"]);
            cmd.Parameters.AddWithValue("@q", r["Qellimi"]);
            cmd.Parameters.AddWithValue("@pd", r["PersoneliQeDorzoi"]);
            cmd.Parameters.AddWithValue("@pm", r["PersoneliQeMorri"]);
            cmd.Parameters.AddWithValue("@sh", r["Shenime"]);
            cmd.Parameters.AddWithValue("@as", r["ArmaSerial"]);
            cmd.Parameters.AddWithValue("@pe", r["PersoneliEmri"]);
            cmd.ExecuteNonQuery();

            // Copy accessories
            var tid = r["Id"];
            if (tid != null && tid != DBNull.Value)
            {
                var aksDt = db.MerrAksesoretByTransaksionId(Convert.ToInt32(tid));
                foreach (DataRow ak in aksDt.Rows)
                {
                    using var ca = new SQLiteCommand("INSERT INTO Aksesoret (TransaksioniId, Emri, Sasia, Shenime) VALUES (@ti,@e,@s,@sh)", conn);
                    ca.Parameters.AddWithValue("@ti", ak["TransaksioniId"]);
                    ca.Parameters.AddWithValue("@e", ak["Emri"]);
                    ca.Parameters.AddWithValue("@s", ak["Sasia"]);
                    ca.Parameters.AddWithValue("@sh", ak["Shenime"] ?? "");
                    ca.ExecuteNonQuery();
                }
            }
        }

        // Save related weapons
        var armet = db.MerrArmet();
        foreach (var serial in armSerialet)
        {
            var ar = armet.AsEnumerable().FirstOrDefault(a => (a["NumerSerial"]?.ToString() ?? "") == serial);
            if (ar == null) continue;
            using var cmd = new SQLiteCommand(
                "INSERT INTO Armet (Id, NumerSerial, Lloji, Marka, Modeli, Kalibri, Statusi, NrInventari) VALUES (@id,@ns,@l,@m,@mo,@k,@s,@ni)", conn);
            cmd.Parameters.AddWithValue("@id", ar["Id"]);
            cmd.Parameters.AddWithValue("@ns", ar["NumerSerial"]);
            cmd.Parameters.AddWithValue("@l", ar["Lloji"]);
            cmd.Parameters.AddWithValue("@m", ar["Marka"]);
            cmd.Parameters.AddWithValue("@mo", ar["Modeli"]);
            cmd.Parameters.AddWithValue("@k", ar["Kalibri"]);
            cmd.Parameters.AddWithValue("@s", ar["Statusi"]);
            cmd.Parameters.AddWithValue("@ni", ar["NrInventari"] ?? "");
            cmd.ExecuteNonQuery();
        }

        return dbPath;
    }

    public static string KrijoBackupFull(DatabaseHelper db, string mainDbPath)
    {
        var backupDir = GetBackupDir();
        var dir = Path.Combine(backupDir, "FullBackup");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var backupPath = Path.Combine(dir, $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.sqlite");

        using var sourceConn = new SQLiteConnection(DatabaseHelper.KrijoConnectionString(mainDbPath));
        sourceConn.Open();

        using var destConn = new SQLiteConnection(DatabaseHelper.KrijoConnectionString(backupPath));
        destConn.Open();

        sourceConn.BackupDatabase(destConn, "main", "main", -1, null, 0);

        return backupPath;
    }

    public static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sb = new System.Text.StringBuilder(name);
        foreach (char c in invalid)
            sb.Replace(c, '_');
        return sb.ToString().Trim(' ', '_');
    }
}
