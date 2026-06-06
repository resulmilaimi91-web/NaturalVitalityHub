using System.Data;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using ArmepunesApp.Models;

namespace ArmepunesApp.Data;

public class DatabaseHelper
{
    private readonly string _connectionString;
    private readonly string _dbPath;

    public DatabaseHelper(string dbPath)
    {
        _dbPath = dbPath;
        _connectionString = $"Data Source={dbPath};Version=3;";
        InitializeDatabase();
    }

    public string GetDbPath() => _dbPath;

    public static string KrijoConnectionString(string dbPath) => $"Data Source={dbPath};Version=3;";

    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private void InitializeDatabase()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();

        string sql = @"
            CREATE TABLE IF NOT EXISTS Armet (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NumerSerial TEXT NOT NULL UNIQUE,
                Lloji TEXT NOT NULL,
                Marka TEXT NOT NULL,
                Modeli TEXT NOT NULL,
                Kalibri TEXT NOT NULL,
                Vendlindja TEXT,
                Statusi TEXT NOT NULL DEFAULT 'Ne Magazine',
                Shenime TEXT,
                DataRegjistrimit TEXT NOT NULL,
                NrInventari TEXT UNIQUE
            );

            CREATE TABLE IF NOT EXISTS Personeli (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Emri TEXT NOT NULL,
                Mbiemri TEXT NOT NULL,
                Grada TEXT NOT NULL,
                Njesia TEXT,
                NrLegjitimacioni TEXT UNIQUE,
                Telefon TEXT
            );

            CREATE TABLE IF NOT EXISTS Klientet (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Emri TEXT NOT NULL,
                Mbiemri TEXT NOT NULL,
                Adresa TEXT,
                Telefon TEXT,
                Email TEXT,
                NrLeternjoftimit TEXT UNIQUE,
                Shenime TEXT
            );

            CREATE TABLE IF NOT EXISTS Transaksionet (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ArmaId INTEGER NOT NULL,
                PersoneliId INTEGER NOT NULL,
                KlientiId INTEGER DEFAULT 0,
                Tipi TEXT NOT NULL CHECK(Tipi IN ('Hyrje','Dalje')),
                DataOra TEXT NOT NULL,
                Qellimi TEXT,
                PersoneliQeDorzoi TEXT,
                PersoneliQeMorri TEXT,
                Shenime TEXT,
                Municioni TEXT,
                FOREIGN KEY(ArmaId) REFERENCES Armet(Id),
                FOREIGN KEY(PersoneliId) REFERENCES Personeli(Id)
            );

            CREATE TABLE IF NOT EXISTS Aksesoret (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TransaksioniId INTEGER NOT NULL,
                Emri TEXT NOT NULL,
                Sasia INTEGER NOT NULL DEFAULT 1,
                Shenime TEXT,
                FOREIGN KEY(TransaksioniId) REFERENCES Transaksionet(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Perdoruesit (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                Emri TEXT,
                Role TEXT NOT NULL DEFAULT 'User'
            );

            CREATE TABLE IF NOT EXISTS AuditLog (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DataOra TEXT NOT NULL,
                Perdoruesi TEXT NOT NULL,
                Veprimi TEXT NOT NULL,
                Detajet TEXT
            );

            CREATE TABLE IF NOT EXISTS Cilësimet (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL DEFAULT '',
                Description TEXT,
                Category TEXT NOT NULL DEFAULT 'General'
            );

            CREATE TABLE IF NOT EXISTS FormaTemplates (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Emri TEXT NOT NULL,
                Lloji TEXT NOT NULL DEFAULT 'Fleteleshim',
                Parametrat TEXT DEFAULT '{}',
                Aktive INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS SchemaVersion (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Version INTEGER NOT NULL,
                AppliedAt TEXT NOT NULL,
                Description TEXT
            );

            CREATE TABLE IF NOT EXISTS Municioni (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TransaksioniId INTEGER NOT NULL,
                Emri TEXT NOT NULL,
                Lloji TEXT,
                Kalibri TEXT,
                Sasia INTEGER NOT NULL DEFAULT 1,
                Njesia TEXT NOT NULL DEFAULT 'copë',
                Shenime TEXT,
                FOREIGN KEY(TransaksioniId) REFERENCES Transaksionet(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS UserPermissions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                Permission TEXT NOT NULL,
                FOREIGN KEY(UserId) REFERENCES Perdoruesit(Id) ON DELETE CASCADE,
                UNIQUE(UserId, Permission)
            );";

        using var cmd = new SQLiteCommand(sql, conn);
        cmd.ExecuteNonQuery();

        string indexesSql = @"
            CREATE INDEX IF NOT EXISTS IX_Armet_NumerSerial ON Armet(NumerSerial);
            CREATE INDEX IF NOT EXISTS IX_Armet_Statusi ON Armet(Statusi);
            CREATE INDEX IF NOT EXISTS IX_Transaksionet_DataOra ON Transaksionet(DataOra);
            CREATE INDEX IF NOT EXISTS IX_Transaksionet_ArmaId ON Transaksionet(ArmaId);
            CREATE INDEX IF NOT EXISTS IX_Transaksionet_Tipi ON Transaksionet(Tipi);
            CREATE INDEX IF NOT EXISTS IX_AuditLog_DataOra ON AuditLog(DataOra);
            CREATE INDEX IF NOT EXISTS IX_Klientet_Emri ON Klientet(Emri, Mbiemri);
            CREATE INDEX IF NOT EXISTS IX_Personeli_Emri ON Personeli(Emri, Mbiemri);
            CREATE INDEX IF NOT EXISTS IX_Municioni_TransaksioniId ON Municioni(TransaksioniId);";

        using var indexCmd = new SQLiteCommand(indexesSql, conn);
        indexCmd.ExecuteNonQuery();

        MigroDb(conn);
    }

    private void MigroDb(SQLiteConnection conn)
    {
        int currentVersion = 0;
        using (var cv = new SQLiteCommand("SELECT COALESCE(MAX(Version), 0) FROM SchemaVersion", conn))
        {
            var result = cv.ExecuteScalar();
            if (result != null) currentVersion = Convert.ToInt32(result);
        }

        if (currentVersion < 1)
        {
            bool hasKlientiId = false;
            using (var ci = new SQLiteCommand("PRAGMA table_info(Transaksionet)", conn))
            using (var r = ci.ExecuteReader())
                while (r.Read())
                    if (r["name"]?.ToString() == "KlientiId") hasKlientiId = true;
            if (!hasKlientiId)
            {
                using var a = new SQLiteCommand("ALTER TABLE Transaksionet ADD COLUMN KlientiId INTEGER DEFAULT 0", conn);
                a.ExecuteNonQuery();
            }
            try { using var ca = new SQLiteCommand("ALTER TABLE Transaksionet ADD COLUMN Municioni TEXT", conn); ca.ExecuteNonQuery(); } catch { }
            try { using var ca = new SQLiteCommand("ALTER TABLE Armet ADD COLUMN VitiProdhimit TEXT", conn); ca.ExecuteNonQuery(); } catch { }

            using var rm = new SQLiteCommand("INSERT INTO SchemaVersion (Version, AppliedAt, Description) VALUES (1, @a, 'Initial schema: added KlientiId, Municioni, VitiProdhimit columns')", conn);
            rm.Parameters.AddWithValue("@a", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            rm.ExecuteNonQuery();
            currentVersion = 1;
        }

        if (currentVersion < 2)
        {
            // Version 2: Municioni table is already created via CREATE TABLE IF NOT EXISTS
            using var rm = new SQLiteCommand("INSERT INTO SchemaVersion (Version, AppliedAt, Description) VALUES (2, @a, 'Added Municioni table for structured ammunition tracking')", conn);
            rm.Parameters.AddWithValue("@a", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            rm.ExecuteNonQuery();
            currentVersion = 2;
        }

        if (currentVersion < 3)
        {
            using var rm = new SQLiteCommand("UPDATE Armet SET Statusi='Tek Klienti' WHERE Statusi='Ne Perdorim'", conn);
            rm.ExecuteNonQuery();
            using var ins = new SQLiteCommand("INSERT INTO SchemaVersion (Version, AppliedAt, Description) VALUES (3, @a, 'Renamed status Ne Perdorim → Tek Klienti')", conn);
            ins.Parameters.AddWithValue("@a", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            ins.ExecuteNonQuery();
            currentVersion = 3;
        }

        // Seed data (only for fresh databases)
        using var chk = new SQLiteCommand("SELECT COUNT(*) FROM Perdoruesit", conn);
        if (Convert.ToInt32(chk.ExecuteScalar()) == 0)
        {
            using var ins = new SQLiteCommand("INSERT INTO Perdoruesit (Username,Password,Emri,Role) VALUES (@u,@p,@e,@r)", conn);
            ins.Parameters.AddWithValue("@u", "admin");
            ins.Parameters.AddWithValue("@p", HashPassword("admin123"));
            ins.Parameters.AddWithValue("@e", "Administratori");
            ins.Parameters.AddWithValue("@r", "Admin");
            ins.ExecuteNonQuery();
        }

        using var chkSettings = new SQLiteCommand("SELECT COUNT(*) FROM Cilësimet", conn);
        if (Convert.ToInt32(chkSettings.ExecuteScalar()) == 0)
        {
            var defaults = new (string key, string value, string desc, string cat)[]
            {
                ("app_name", "POLIGONI DRENI", "Emri i aplikacionit", "Appearance"),
                ("app_subtitle", "Qendra e Deponimit dhe Menaxhimit te Armeve", "Nentitulli", "Appearance"),
                ("auto_refresh_interval", "5000", "Intervali i rifreskimit (ms)", "Performance"),
                ("default_role", "User", "Roli i ri default", "Security"),
                ("print_copy_arkivi", "ARKIVI", "Emri i kopjes se arki", "Print"),
                ("print_copy_klienti", "KLIENTI", "Emri i kopjes se klientit", "Print"),
                ("backup_auto", "false", "Backup automatik", "Database"),
                ("backup_interval_days", "7", "Intervali backup (dite)", "Database"),
                ("update_url", "", "URL per kontrollin e azhornimeve (lini bosh per lokal)", "Updates"),
            };
            foreach (var d in defaults)
            {
                using var ins2 = new SQLiteCommand("INSERT INTO Cilësimet (Key, Value, Description, Category) VALUES (@k, @v, @d, @c)", conn);
                ins2.Parameters.AddWithValue("@k", d.key);
                ins2.Parameters.AddWithValue("@v", d.value);
                ins2.Parameters.AddWithValue("@d", d.desc);
                ins2.Parameters.AddWithValue("@c", d.cat);
                ins2.ExecuteNonQuery();
            }
        }

        using var chkTemplates = new SQLiteCommand("SELECT COUNT(*) FROM FormaTemplates", conn);
        if (Convert.ToInt32(chkTemplates.ExecuteScalar()) == 0)
        {
            var templates = new (string emri, string lloji, string parametrat, int aktive)[]
            {
                ("Standard - Fleteleshim 2 kopje", "Fleteleshim", @"{""kopje"":2,""stili"":""standard"",""titulli"":""ARKIVI/KLIENTI""}", 1),
                ("Thjeshtuar - Fleteleshim 1 kopje", "Fleteleshim", @"{""kopje"":1,""stili"":""thjeshtuar"",""titulli"":""KOPJE""}", 0),
                ("Fletpranim - Pranim Armesh", "Fletpranim", @"{""kopje"":2,""stili"":""standard"",""titulli"":""PRANUES/DOREZUES""}", 0),
                ("Raport i Detajuar", "Raport", @"{""kopje"":1,""stili"":""detajuar"",""titulli"":""RAPORT""}", 0),
            };
            foreach (var t in templates)
            {
                using var insT = new SQLiteCommand("INSERT INTO FormaTemplates (Emri, Lloji, Parametrat, Aktive) VALUES (@e, @l, @p, @a)", conn);
                insT.Parameters.AddWithValue("@e", t.emri);
                insT.Parameters.AddWithValue("@l", t.lloji);
                insT.Parameters.AddWithValue("@p", t.parametrat);
                insT.Parameters.AddWithValue("@a", t.aktive);
                insT.ExecuteNonQuery();
            }
        }
    }

    // ==================== ARMET ====================

    public DataTable MerrArmet()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var da = new SQLiteDataAdapter("SELECT * FROM Armet ORDER BY DataRegjistrimit DESC", conn);
        var dt = new DataTable(); da.Fill(dt); return dt;
    }

    public DataTable KerkoArmet(string filter)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("SELECT * FROM Armet WHERE NumerSerial LIKE @f OR Lloji LIKE @f OR Marka LIKE @f OR Modeli LIKE @f ORDER BY DataRegjistrimit DESC", conn);
        cmd.Parameters.AddWithValue("@f", $"%{filter}%");
        var dt = new DataTable(); new SQLiteDataAdapter(cmd).Fill(dt); return dt;
    }

    public bool ShtoArme(Arma arma)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("INSERT INTO Armet (NumerSerial,Lloji,Marka,Modeli,Kalibri,Vendlindja,Statusi,Shenime,DataRegjistrimit,NrInventari) VALUES (@ns,@l,@m,@mo,@k,@v,@s,@sh,@d,@ni)", conn);
        cmd.Parameters.AddWithValue("@ns", arma.NumerSerial); cmd.Parameters.AddWithValue("@l", arma.Lloji);
        cmd.Parameters.AddWithValue("@m", arma.Marka); cmd.Parameters.AddWithValue("@mo", arma.Modeli);
        cmd.Parameters.AddWithValue("@k", arma.Kalibri); cmd.Parameters.AddWithValue("@v", arma.Vendlindja);
        cmd.Parameters.AddWithValue("@s", arma.Statusi); cmd.Parameters.AddWithValue("@sh", arma.Shenime);
        cmd.Parameters.AddWithValue("@d", arma.DataRegjistrimit); cmd.Parameters.AddWithValue("@ni", arma.NrInventari);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool NdryshoArme(Arma arma)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("UPDATE Armet SET NumerSerial=@ns,Lloji=@l,Marka=@m,Modeli=@mo,Kalibri=@k,Vendlindja=@v,Statusi=@s,Shenime=@sh,NrInventari=@ni WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", arma.Id); cmd.Parameters.AddWithValue("@ns", arma.NumerSerial);
        cmd.Parameters.AddWithValue("@l", arma.Lloji); cmd.Parameters.AddWithValue("@m", arma.Marka);
        cmd.Parameters.AddWithValue("@mo", arma.Modeli); cmd.Parameters.AddWithValue("@k", arma.Kalibri);
        cmd.Parameters.AddWithValue("@v", arma.Vendlindja); cmd.Parameters.AddWithValue("@s", arma.Statusi);
        cmd.Parameters.AddWithValue("@sh", arma.Shenime); cmd.Parameters.AddWithValue("@ni", arma.NrInventari);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool FshiArme(int id, string perdoruesi = "")
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        string sn = "";
        using (var c = new SQLiteCommand("SELECT NumerSerial FROM Armet WHERE Id=@id", conn))
        { c.Parameters.AddWithValue("@id", id); sn = c.ExecuteScalar()?.ToString() ?? ""; }

        using var cmdDelT = new SQLiteCommand("DELETE FROM Transaksionet WHERE ArmaId=@aid", conn);
        cmdDelT.Parameters.AddWithValue("@aid", id);
        cmdDelT.ExecuteNonQuery();

        using var cmd = new SQLiteCommand("DELETE FROM Armet WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        var result = cmd.ExecuteNonQuery() > 0;
        if (result && !string.IsNullOrEmpty(perdoruesi))
            RegjistroAuditLog(perdoruesi, "Fshi Arme", $"Id: {id}, Seriali: {sn}");
        return result;
    }

     public Arma? MerrArmeById(int id)
     {
         using var conn = new SQLiteConnection(_connectionString);
         conn.Open();
         using var cmd = new SQLiteCommand("SELECT * FROM Armet WHERE Id=@id", conn);
         cmd.Parameters.AddWithValue("@id", id);
         using var rdr = cmd.ExecuteReader();
         if (rdr.Read())
             return new Arma
             {
                 Id = Convert.ToInt32(rdr["Id"]), NumerSerial = rdr["NumerSerial"]?.ToString() ?? "",
                 Lloji = rdr["Lloji"]?.ToString() ?? "", Marka = rdr["Marka"]?.ToString() ?? "",
                 Modeli = rdr["Modeli"]?.ToString() ?? "", Kalibri = rdr["Kalibri"]?.ToString() ?? "",
                 Vendlindja = rdr["Vendlindja"]?.ToString() ?? "", Statusi = rdr["Statusi"]?.ToString() ?? "",
                 Shenime = rdr["Shenime"]?.ToString() ?? "", DataRegjistrimit = rdr["DataRegjistrimit"]?.ToString() ?? "",
                 NrInventari = rdr["NrInventari"]?.ToString() ?? ""
             };
         return null;
     }

     public Arma? MerrArmeBySerial(string serial)
     {
         if (string.IsNullOrWhiteSpace(serial)) return null;
         using var conn = new SQLiteConnection(_connectionString);
         conn.Open();
         using var cmd = new SQLiteCommand("SELECT * FROM Armet WHERE NumerSerial = @serial", conn);
         cmd.Parameters.AddWithValue("@serial", serial.Trim());
         using var rdr = cmd.ExecuteReader();
         if (rdr.Read())
             return new Arma
             {
                 Id = Convert.ToInt32(rdr["Id"]), NumerSerial = rdr["NumerSerial"]?.ToString() ?? "",
                 Lloji = rdr["Lloji"]?.ToString() ?? "", Marka = rdr["Marka"]?.ToString() ?? "",
                 Modeli = rdr["Modeli"]?.ToString() ?? "", Kalibri = rdr["Kalibri"]?.ToString() ?? "",
                 Vendlindja = rdr["Vendlindja"]?.ToString() ?? "", Statusi = rdr["Statusi"]?.ToString() ?? "",
                 Shenime = rdr["Shenime"]?.ToString() ?? "", DataRegjistrimit = rdr["DataRegjistrimit"]?.ToString() ?? "",
                 NrInventari = rdr["NrInventari"]?.ToString() ?? ""
             };
         return null;
     }

    // ==================== PERSONELI ====================

    public DataTable MerrPersonelin()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        var dt = new DataTable(); new SQLiteDataAdapter("SELECT * FROM Personeli ORDER BY Emri, Mbiemri", conn).Fill(dt); return dt;
    }

    public bool ShtoPersonel(Personeli p)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("INSERT INTO Personeli (Emri,Mbiemri,Grada,Njesia,NrLegjitimacioni,Telefon) VALUES (@e,@m,@g,@n,@nl,@t)", conn);
        cmd.Parameters.AddWithValue("@e", p.Emri); cmd.Parameters.AddWithValue("@m", p.Mbiemri);
        cmd.Parameters.AddWithValue("@g", p.Grada); cmd.Parameters.AddWithValue("@n", p.Njesia);
        cmd.Parameters.AddWithValue("@nl", p.NrLegjitimacioni); cmd.Parameters.AddWithValue("@t", p.Telefon);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool NdryshoPersonel(Personeli p)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("UPDATE Personeli SET Emri=@e,Mbiemri=@m,Grada=@g,Njesia=@n,NrLegjitimacioni=@nl,Telefon=@t WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", p.Id); cmd.Parameters.AddWithValue("@e", p.Emri);
        cmd.Parameters.AddWithValue("@m", p.Mbiemri); cmd.Parameters.AddWithValue("@g", p.Grada);
        cmd.Parameters.AddWithValue("@n", p.Njesia); cmd.Parameters.AddWithValue("@nl", p.NrLegjitimacioni);
        cmd.Parameters.AddWithValue("@t", p.Telefon);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool FshiPersonel(int id, string perdoruesi = "")
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        string emri = "";
        using (var c = new SQLiteCommand("SELECT Emri || ' ' || Mbiemri FROM Personeli WHERE Id=@id", conn))
        { c.Parameters.AddWithValue("@id", id); emri = c.ExecuteScalar()?.ToString() ?? ""; }

        var armaIds = new List<int>();
        using (var cmdT = new SQLiteCommand("SELECT DISTINCT ArmaId FROM Transaksionet WHERE PersoneliId=@pid", conn))
        {
            cmdT.Parameters.AddWithValue("@pid", id);
            using var rdr = cmdT.ExecuteReader();
            while (rdr.Read())
                armaIds.Add(Convert.ToInt32(rdr["ArmaId"]));
        }

        using var cmdDel = new SQLiteCommand("DELETE FROM Transaksionet WHERE PersoneliId=@pid", conn);
        cmdDel.Parameters.AddWithValue("@pid", id);
        cmdDel.ExecuteNonQuery();

        foreach (var aid in armaIds)
        {
            string status = "Ne Magazine";
            using (var cmdS = new SQLiteCommand("SELECT Tipi FROM Transaksionet WHERE ArmaId=@aid ORDER BY DataOra DESC LIMIT 1", conn))
            {
                cmdS.Parameters.AddWithValue("@aid", aid);
                var tipiFundit = cmdS.ExecuteScalar()?.ToString();
                if (tipiFundit == "Dalje") status = "Tek Klienti";
            }
            using var cmdUpd = new SQLiteCommand("UPDATE Armet SET Statusi=@s WHERE Id=@aid", conn);
            cmdUpd.Parameters.AddWithValue("@s", status);
            cmdUpd.Parameters.AddWithValue("@aid", aid);
            cmdUpd.ExecuteNonQuery();
        }

        using var cmd = new SQLiteCommand("DELETE FROM Personeli WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        var result = cmd.ExecuteNonQuery() > 0;
        if (result && !string.IsNullOrEmpty(perdoruesi))
            RegjistroAuditLog(perdoruesi, "Fshi Personel", $"Id: {id}, Emri: {emri}, Arme te prekura: {armaIds.Count}");
        return result;
    }

    // ==================== KLIENTET ====================

    public DataTable MerrKlientet()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        var dt = new DataTable(); new SQLiteDataAdapter("SELECT * FROM Klientet ORDER BY Emri, Mbiemri", conn).Fill(dt); return dt;
    }

    public DataTable? MerrKlientByNID(string nrLeternjoftimit)
    {
        if (string.IsNullOrWhiteSpace(nrLeternjoftimit)) return null;
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("SELECT * FROM Klientet WHERE NrLeternjoftimit=@nl", conn);
        cmd.Parameters.AddWithValue("@nl", nrLeternjoftimit);
        var dt = new DataTable();
        new SQLiteDataAdapter(cmd).Fill(dt);
        return dt.Rows.Count > 0 ? dt : null;
    }

    public DataTable? MerrKlientByEmriMbiemri(string emri, string mbiemri)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("SELECT * FROM Klientet WHERE Emri=@e AND Mbiemri=@m", conn);
        cmd.Parameters.AddWithValue("@e", emri);
        cmd.Parameters.AddWithValue("@m", mbiemri);
        var dt = new DataTable();
        new SQLiteDataAdapter(cmd).Fill(dt);
        return dt.Rows.Count > 0 ? dt : null;
    }

    public bool ShtoKlient(Klienti k)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("INSERT INTO Klientet (Emri,Mbiemri,Adresa,Telefon,Email,NrLeternjoftimit,Shenime) VALUES (@e,@m,@a,@t,@em,@nl,@sh)", conn);
        cmd.Parameters.AddWithValue("@e", k.Emri); cmd.Parameters.AddWithValue("@m", k.Mbiemri);
        cmd.Parameters.AddWithValue("@a", k.Adresa ?? ""); cmd.Parameters.AddWithValue("@t", k.Telefon ?? "");
        cmd.Parameters.AddWithValue("@em", k.Email ?? ""); cmd.Parameters.AddWithValue("@nl", k.NrLeternjoftimit ?? "");
        cmd.Parameters.AddWithValue("@sh", k.Shenime ?? "");
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool NdryshoKlient(Klienti k)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("UPDATE Klientet SET Emri=@e,Mbiemri=@m,Adresa=@a,Telefon=@t,Email=@em,NrLeternjoftimit=@nl,Shenime=@sh WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", k.Id); cmd.Parameters.AddWithValue("@e", k.Emri);
        cmd.Parameters.AddWithValue("@m", k.Mbiemri); cmd.Parameters.AddWithValue("@a", k.Adresa ?? "");
        cmd.Parameters.AddWithValue("@t", k.Telefon ?? ""); cmd.Parameters.AddWithValue("@em", k.Email ?? "");
        cmd.Parameters.AddWithValue("@nl", k.NrLeternjoftimit ?? ""); cmd.Parameters.AddWithValue("@sh", k.Shenime ?? "");
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool FshiKlient(int id, string perdoruesi = "")
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        string emri = "";
        using (var c = new SQLiteCommand("SELECT Emri || ' ' || Mbiemri FROM Klientet WHERE Id=@id", conn))
        { c.Parameters.AddWithValue("@id", id); emri = c.ExecuteScalar()?.ToString() ?? ""; }

        var armaIds = new List<int>();
        using (var cmdT = new SQLiteCommand("SELECT DISTINCT ArmaId FROM Transaksionet WHERE KlientiId=@kid", conn))
        {
            cmdT.Parameters.AddWithValue("@kid", id);
            using var rdr = cmdT.ExecuteReader();
            while (rdr.Read())
                armaIds.Add(Convert.ToInt32(rdr["ArmaId"]));
        }

        using var cmdDel = new SQLiteCommand("DELETE FROM Transaksionet WHERE KlientiId=@kid", conn);
        cmdDel.Parameters.AddWithValue("@kid", id);
        cmdDel.ExecuteNonQuery();

        foreach (var aid in armaIds)
        {
            string status = "Ne Magazine";
            using (var cmdS = new SQLiteCommand("SELECT Tipi FROM Transaksionet WHERE ArmaId=@aid ORDER BY DataOra DESC LIMIT 1", conn))
            {
                cmdS.Parameters.AddWithValue("@aid", aid);
                var tipiFundit = cmdS.ExecuteScalar()?.ToString();
                if (tipiFundit == "Dalje") status = "Tek Klienti";
            }
            using var cmdUpd = new SQLiteCommand("UPDATE Armet SET Statusi=@s WHERE Id=@aid", conn);
            cmdUpd.Parameters.AddWithValue("@s", status);
            cmdUpd.Parameters.AddWithValue("@aid", aid);
            cmdUpd.ExecuteNonQuery();
        }

        using var cmd = new SQLiteCommand("DELETE FROM Klientet WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        var result = cmd.ExecuteNonQuery() > 0;
        if (result && !string.IsNullOrEmpty(perdoruesi))
            RegjistroAuditLog(perdoruesi, "Fshi Klient", $"Id: {id}, Emri: {emri}, Arme te prekura: {armaIds.Count}");
        return result;
    }

    // ==================== TRANSAKSIONET ====================

    public DataTable MerrTransaksionet()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        string sql = @"SELECT t.*, a.NumerSerial AS ArmaSerial, 
                       (p.Emri || ' ' || p.Mbiemri) AS PersoneliEmri,
                       COALESCE((SELECT k.Emri || ' ' || k.Mbiemri FROM Klientet k WHERE k.Id = t.KlientiId), '') AS KlientiEmri
                       FROM Transaksionet t
                       JOIN Armet a ON t.ArmaId = a.Id
                       JOIN Personeli p ON t.PersoneliId = p.Id
                       ORDER BY t.DataOra DESC";
        var dt = new DataTable(); new SQLiteDataAdapter(sql, conn).Fill(dt); return dt;
    }

    public DataTable KerkoTransaksionet(string filter)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand(@"SELECT t.*, a.NumerSerial AS ArmaSerial, 
                       (p.Emri || ' ' || p.Mbiemri) AS PersoneliEmri,
                       COALESCE((SELECT k.Emri || ' ' || k.Mbiemri FROM Klientet k WHERE k.Id = t.KlientiId), '') AS KlientiEmri
                       FROM Transaksionet t
                       JOIN Armet a ON t.ArmaId = a.Id
                       JOIN Personeli p ON t.PersoneliId = p.Id
                       WHERE a.NumerSerial LIKE @f OR p.Emri LIKE @f OR p.Mbiemri LIKE @f OR t.Tipi LIKE @f
                       ORDER BY t.DataOra DESC", conn);
        cmd.Parameters.AddWithValue("@f", $"%{filter}%");
        var dt = new DataTable(); new SQLiteDataAdapter(cmd).Fill(dt); return dt;
    }

    public DataTable MerrTransaksionetBySerial(string serial)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand(@"SELECT t.*, a.NumerSerial AS ArmaSerial,
                       (p.Emri || ' ' || p.Mbiemri) AS PersoneliEmri,
                       COALESCE((SELECT k.Emri || ' ' || k.Mbiemri FROM Klientet k WHERE k.Id = t.KlientiId), '') AS KlientiEmri
                       FROM Transaksionet t
                       JOIN Armet a ON t.ArmaId = a.Id
                       JOIN Personeli p ON t.PersoneliId = p.Id
                       WHERE a.NumerSerial = @s
                       ORDER BY t.DataOra DESC", conn);
        cmd.Parameters.AddWithValue("@s", serial);
        var dt = new DataTable(); new SQLiteDataAdapter(cmd).Fill(dt); return dt;
    }

    public int RegjistroTransaksion(Transaksioni t, string perdoruesi = "")
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            string sql = @"INSERT INTO Transaksionet (ArmaId, PersoneliId, KlientiId, Tipi, DataOra, 
                           Qellimi, PersoneliQeDorzoi, PersoneliQeMorri, Shenime, Municioni)
                           VALUES (@ai, @pi, @ki, @tip, @do, @q, @pd, @pm, @sh, @m)";
            using var cmd = new SQLiteCommand(sql, conn, tran);
            cmd.Parameters.AddWithValue("@ai", t.ArmaId);
            cmd.Parameters.AddWithValue("@pi", t.PersoneliId);
            cmd.Parameters.AddWithValue("@ki", t.KlientiId);
            cmd.Parameters.AddWithValue("@tip", t.Tipi);
            cmd.Parameters.AddWithValue("@do", t.DataOra);
            cmd.Parameters.AddWithValue("@q", t.Qellimi);
            cmd.Parameters.AddWithValue("@pd", t.PersoneliQeDorzoi);
            cmd.Parameters.AddWithValue("@pm", t.PersoneliQeMorri);
            cmd.Parameters.AddWithValue("@sh", t.Shenime);
            cmd.Parameters.AddWithValue("@m", t.Municioni);
            cmd.ExecuteNonQuery();

            long transId = conn.LastInsertRowId;

            foreach (var aks in t.Aksesoret)
            {
                using var ca = new SQLiteCommand("INSERT INTO Aksesoret (TransaksioniId, Emri, Sasia, Shenime) VALUES (@ti, @e, @s, @sh)", conn, tran);
                ca.Parameters.AddWithValue("@ti", transId);
                ca.Parameters.AddWithValue("@e", aks.Emri);
                ca.Parameters.AddWithValue("@s", aks.Sasia);
                ca.Parameters.AddWithValue("@sh", aks.Shenime ?? "");
                ca.ExecuteNonQuery();
            }

            foreach (var mun in t.Municionet)
            {
                using var cm = new SQLiteCommand("INSERT INTO Municioni (TransaksioniId, Emri, Lloji, Kalibri, Sasia, Njesia, Shenime) VALUES (@ti, @e, @l, @k, @s, @nj, @sh)", conn, tran);
                cm.Parameters.AddWithValue("@ti", transId);
                cm.Parameters.AddWithValue("@e", mun.Emri);
                cm.Parameters.AddWithValue("@l", mun.Lloji ?? "");
                cm.Parameters.AddWithValue("@k", mun.Kalibri ?? "");
                cm.Parameters.AddWithValue("@s", mun.Sasia);
                cm.Parameters.AddWithValue("@nj", mun.Njesia);
                cm.Parameters.AddWithValue("@sh", mun.Shenime ?? "");
                cm.ExecuteNonQuery();
            }

            string updateStatus = t.Tipi == "Dalje"
                ? "UPDATE Armet SET Statusi='Ne Perdorim' WHERE Id=@aid"
                : "UPDATE Armet SET Statusi='Ne Magazine' WHERE Id=@aid";
            using var cmd2 = new SQLiteCommand(updateStatus, conn, tran);
            cmd2.Parameters.AddWithValue("@aid", t.ArmaId);
            cmd2.ExecuteNonQuery();

            tran.Commit();

            if (!string.IsNullOrEmpty(perdoruesi))
            {
                var detajet = $"Tipi: {t.Tipi}, ArmaId: {t.ArmaId}, Seriali: {t.ArmaSerial}, Data: {t.DataOra}";
                if (t.Aksesoret.Count > 0)
                    detajet += ", Aksesoret: " + string.Join(", ", t.Aksesoret.Select(a => a.Emri));
                RegjistroAuditLog(perdoruesi, $"Regjistro {t.Tipi}", detajet);
            }

            return (int)transId;
        }
        catch { tran.Rollback(); throw; }
    }

    public bool FshiTransaksion(int id, string perdoruesi = "")
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();

        // Merr te dhenat e transaksionit perpara fshirjes
        string tipi = "";
        int armaId = 0;
        using (var cmdGet = new SQLiteCommand("SELECT Tipi, ArmaId FROM Transaksionet WHERE Id=@id", conn))
        {
            cmdGet.Parameters.AddWithValue("@id", id);
            using var rdr = cmdGet.ExecuteReader();
            if (rdr.Read())
            {
                tipi = rdr["Tipi"]?.ToString() ?? "";
                armaId = Convert.ToInt32(rdr["ArmaId"]);
            }
        }

        if (string.IsNullOrEmpty(tipi) || armaId == 0)
            return false;

        using var cmd = new SQLiteCommand("DELETE FROM Transaksionet WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        var result = cmd.ExecuteNonQuery() > 0;

        if (result)
        {
            // Rivendos statusin e armes: nese transaksioni ishte Dalje -> kthe ne Magazine, anasjelltas
            string newStatus = tipi == "Dalje" ? "Ne Magazine" : "Tek Klienti";
            using var cmdStatus = new SQLiteCommand("UPDATE Armet SET Statusi=@s WHERE Id=@aid", conn);
            cmdStatus.Parameters.AddWithValue("@s", newStatus);
            cmdStatus.Parameters.AddWithValue("@aid", armaId);
            cmdStatus.ExecuteNonQuery();

            if (!string.IsNullOrEmpty(perdoruesi))
                RegjistroAuditLog(perdoruesi, "Fshi Transaksion", $"TransaksioniId: {id}, ArmaId: {armaId}, Statusi ri: {newStatus}");
        }
        return result;
    }

    public bool NdryshoTipinTransaksionit(int id, string tipiRi, string perdoruesi = "")
    {
        if (tipiRi != "Hyrje" && tipiRi != "Dalje") return false;

        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();

        string tipiVjeter = "";
        int armaId = 0;
        using (var cmdGet = new SQLiteCommand("SELECT Tipi, ArmaId FROM Transaksionet WHERE Id=@id", conn))
        {
            cmdGet.Parameters.AddWithValue("@id", id);
            using var rdr = cmdGet.ExecuteReader();
            if (rdr.Read())
            {
                tipiVjeter = rdr["Tipi"]?.ToString() ?? "";
                armaId = Convert.ToInt32(rdr["ArmaId"]);
            }
        }

        if (string.IsNullOrEmpty(tipiVjeter) || tipiVjeter == tipiRi || armaId == 0)
            return false;

        using var cmdUpd = new SQLiteCommand("UPDATE Transaksionet SET Tipi=@tipiRi WHERE Id=@id", conn);
        cmdUpd.Parameters.AddWithValue("@tipiRi", tipiRi);
        cmdUpd.Parameters.AddWithValue("@id", id);
        cmdUpd.ExecuteNonQuery();

        // Ndrysho statusin e armes sipas tipit te ri
        string statusRi = tipiRi == "Hyrje" ? "Ne Magazine" : "Tek Klienti";
        using var cmdStatus = new SQLiteCommand("UPDATE Armet SET Statusi=@s WHERE Id=@aid", conn);
        cmdStatus.Parameters.AddWithValue("@s", statusRi);
        cmdStatus.Parameters.AddWithValue("@aid", armaId);
        cmdStatus.ExecuteNonQuery();

        if (!string.IsNullOrEmpty(perdoruesi))
            RegjistroAuditLog(perdoruesi, "Ndrysho Tip Transaksioni",
                $"TransaksioniId: {id}, ArmaId: {armaId}, Nga '{tipiVjeter}' ne '{tipiRi}'");

        return true;
    }

    public DataRow? MerrTransaksionById(int id)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        string sql = @"SELECT t.*, a.NumerSerial AS ArmaSerial, a.Lloji AS ArmaLloji, 
                       a.Marka AS ArmaMarka, a.Modeli AS ArmaModeli, a.Kalibri AS ArmaKalibri,
                       a.NrInventari AS ArmaNrInventari,
                       (p.Emri || ' ' || p.Mbiemri) AS PersoneliEmri,
                       p.Emri AS PersoneliEmriVetem, p.Mbiemri AS PersoneliMbiemri, 
                       p.Grada AS PersoneliGrada, p.Njesia AS PersoneliNjesia,
                       p.NrLegjitimacioni AS PersoneliLegjitimacioni,
                       COALESCE((SELECT k.Emri || ' ' || k.Mbiemri FROM Klientet k WHERE k.Id = t.KlientiId), '') AS KlientiEmri
                       FROM Transaksionet t
                       JOIN Armet a ON t.ArmaId = a.Id
                       JOIN Personeli p ON t.PersoneliId = p.Id
                       WHERE t.Id = @id";
        using var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        var dt = new DataTable(); new SQLiteDataAdapter(cmd).Fill(dt);
        return dt.Rows.Count > 0 ? dt.Rows[0] : null;
    }

    public DataTable MerrAksesoretByTransaksionId(int transaksioniId)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        var dt = new DataTable();
        using var cmd = new SQLiteCommand("SELECT * FROM Aksesoret WHERE TransaksioniId=@id ORDER BY Id", conn);
        cmd.Parameters.AddWithValue("@id", transaksioniId);
        new SQLiteDataAdapter(cmd).Fill(dt);
        return dt;
    }

    public DataTable MerrAksesoretFunditHyrjePerArmen(int armaId)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        string sql = @"SELECT a.* FROM Aksesoret a
            JOIN Transaksionet t ON t.Id = a.TransaksioniId
            WHERE t.ArmaId = @aid AND t.Tipi = 'Hyrje'
            AND t.Id = (SELECT MAX(t2.Id) FROM Transaksionet t2 WHERE t2.ArmaId = @aid2 AND t2.Tipi = 'Hyrje')
            ORDER BY a.Id";
        using var cmd = new SQLiteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@aid", armaId);
        cmd.Parameters.AddWithValue("@aid2", armaId);
        var dt = new DataTable(); new SQLiteDataAdapter(cmd).Fill(dt);
        return dt;
    }

    public DataTable MerrMunicionetByTransaksionId(int transaksioniId)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        var dt = new DataTable();
        using var cmd = new SQLiteCommand("SELECT * FROM Municioni WHERE TransaksioniId=@id ORDER BY Id", conn);
        cmd.Parameters.AddWithValue("@id", transaksioniId);
        new SQLiteDataAdapter(cmd).Fill(dt);
        return dt;
    }

    public bool ShtoMunicion(Municioni m)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("INSERT INTO Municioni (TransaksioniId, Emri, Lloji, Kalibri, Sasia, Njesia, Shenime) VALUES (@ti, @e, @l, @k, @s, @nj, @sh)", conn);
        cmd.Parameters.AddWithValue("@ti", m.TransaksioniId);
        cmd.Parameters.AddWithValue("@e", m.Emri);
        cmd.Parameters.AddWithValue("@l", m.Lloji ?? "");
        cmd.Parameters.AddWithValue("@k", m.Kalibri ?? "");
        cmd.Parameters.AddWithValue("@s", m.Sasia);
        cmd.Parameters.AddWithValue("@nj", m.Njesia);
        cmd.Parameters.AddWithValue("@sh", m.Shenime ?? "");
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool FshiMunicionetByTransaksionId(int transaksioniId)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("DELETE FROM Municioni WHERE TransaksioniId=@id", conn);
        cmd.Parameters.AddWithValue("@id", transaksioniId);
        cmd.ExecuteNonQuery();
        return true;
    }

    public int MerrKlientiIdFunditHyrjePerArmen(int armaId)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand(
            "SELECT KlientiId FROM Transaksionet WHERE ArmaId = @aid AND Tipi = 'Hyrje' ORDER BY Id DESC LIMIT 1", conn);
        cmd.Parameters.AddWithValue("@aid", armaId);
        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public DataTable MerrArmetNeMagazine()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        var dt = new DataTable();
        new SQLiteDataAdapter("SELECT * FROM Armet WHERE Statusi='Ne Magazine' ORDER BY NumerSerial", conn).Fill(dt);
        return dt;
    }

    public DataTable MerrArmetTekKlienti()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        var dt = new DataTable();
        string sql = @"
            SELECT a.NumerSerial AS Seriali, a.Marka, a.Modeli, a.Lloji, a.Kalibri, a.NrInventari,
                   COALESCE(k.Emri || ' ' || k.Mbiemri, '') AS Klienti,
                   COALESCE(p.Emri || ' ' || p.Mbiemri, '') AS Stafi,
                   t.DataOra AS DataDaljes,
                   t.Qellimi
            FROM Armet a
            LEFT JOIN Transaksionet t ON t.ArmaId = a.Id AND t.Tipi = 'Dalje'
            LEFT JOIN Klientet k ON k.Id = t.KlientiId
            LEFT JOIN Personeli p ON p.Id = t.PersoneliId
            WHERE a.Statusi = 'Tek Klienti'
            AND t.Id = (
                SELECT MAX(t2.Id) FROM Transaksionet t2
                WHERE t2.ArmaId = a.Id AND t2.Tipi = 'Dalje'
            )
            ORDER BY a.NumerSerial";
        new SQLiteDataAdapter(sql, conn).Fill(dt);
        return dt;
    }

    public DataTable MerrGjendjenDeponimit()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        string sql = @"
            SELECT a.Id AS ArmaId, a.NumerSerial AS Seriali, a.Marka, a.Modeli, a.Lloji, a.Kalibri,
                   a.NrInventari,
                   COALESCE(k.Emri || ' ' || k.Mbiemri, '') AS Klienti,
                   COALESCE(p.Emri || ' ' || p.Mbiemri, '') AS Stafi,
                   t.DataOra AS DataHyrjes,
                   t.Qellimi,
                   a.Statusi
            FROM Armet a
            LEFT JOIN Transaksionet t ON t.ArmaId = a.Id AND t.Tipi = 'Hyrje'
            LEFT JOIN Klientet k ON k.Id = t.KlientiId
            LEFT JOIN Personeli p ON p.Id = t.PersoneliId
            WHERE a.Statusi = 'Ne Magazine'
            AND t.Id = (
                SELECT MAX(t2.Id) FROM Transaksionet t2
                WHERE t2.ArmaId = a.Id AND t2.Tipi = 'Hyrje'
            )
            ORDER BY a.NumerSerial";
        var dt = new DataTable(); new SQLiteDataAdapter(sql, conn).Fill(dt); return dt;
    }

    // ==================== PERDORUESIT ====================

    public bool VerifikoPerdoruesin(string username, string password, out Perdoruesi? perdoruesi)
    {
        perdoruesi = null;
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();

        // First try with hashed password
        var hashedInput = HashPassword(password);
        using var cmdHash = new SQLiteCommand("SELECT * FROM Perdoruesit WHERE Username=@u AND Password=@p", conn);
        cmdHash.Parameters.AddWithValue("@u", username);
        cmdHash.Parameters.AddWithValue("@p", hashedInput);
        using var rdrHash = cmdHash.ExecuteReader();
        if (rdrHash.Read())
        {
            perdoruesi = LexoPerdorues(rdrHash);
            return true;
        }
        rdrHash.Close();

        // Fallback: try plaintext for old passwords (migration path)
        using var cmdPlain = new SQLiteCommand("SELECT * FROM Perdoruesit WHERE Username=@u AND Password=@p", conn);
        cmdPlain.Parameters.AddWithValue("@u", username);
        cmdPlain.Parameters.AddWithValue("@p", password);
        using var rdrPlain = cmdPlain.ExecuteReader();
        if (rdrPlain.Read())
        {
            perdoruesi = LexoPerdorues(rdrPlain);
            rdrPlain.Close();

            // Migrate: update to hashed password
            using var updateCmd = new SQLiteCommand("UPDATE Perdoruesit SET Password=@p WHERE Id=@id", conn);
            updateCmd.Parameters.AddWithValue("@p", hashedInput);
            updateCmd.Parameters.AddWithValue("@id", perdoruesi.Id);
            updateCmd.ExecuteNonQuery();
            return true;
        }
        return false;
    }

    private static Perdoruesi LexoPerdorues(SQLiteDataReader rdr)
    {
        return new Perdoruesi
        {
            Id = Convert.ToInt32(rdr["Id"]),
            Username = rdr["Username"]?.ToString() ?? "",
            Password = rdr["Password"]?.ToString() ?? "",
            Emri = rdr["Emri"]?.ToString() ?? "",
            Role = rdr["Role"]?.ToString() ?? "User"
        };
    }

    public DataTable MerrPerdoruesit()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        var dt = new DataTable();
        new SQLiteDataAdapter("SELECT Id, Username, Password, Emri, Role FROM Perdoruesit ORDER BY Emri", conn).Fill(dt);
        return dt;
    }

    public bool ShtoPerdorues(Perdoruesi p)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("INSERT INTO Perdoruesit (Username,Password,Emri,Role) VALUES (@u,@p,@e,@r)", conn);
        cmd.Parameters.AddWithValue("@u", p.Username);
        cmd.Parameters.AddWithValue("@p", HashPassword(p.Password));
        cmd.Parameters.AddWithValue("@e", p.Emri);
        cmd.Parameters.AddWithValue("@r", p.Role);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool NdryshoPerdorues(Perdoruesi p)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("UPDATE Perdoruesit SET Username=@u, Password=@p, Emri=@e, Role=@r WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", p.Id);
        cmd.Parameters.AddWithValue("@u", p.Username);
        cmd.Parameters.AddWithValue("@p", HashPassword(p.Password));
        cmd.Parameters.AddWithValue("@e", p.Emri);
        cmd.Parameters.AddWithValue("@r", p.Role);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool FshiPerdorues(int id, string perdoruesi = "")
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        string emri = "";
        using (var c = new SQLiteCommand("SELECT Username FROM Perdoruesit WHERE Id=@id", conn))
        { c.Parameters.AddWithValue("@id", id); emri = c.ExecuteScalar()?.ToString() ?? ""; }
        using var cmd = new SQLiteCommand("DELETE FROM Perdoruesit WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        var result = cmd.ExecuteNonQuery() > 0;
        if (result && !string.IsNullOrEmpty(perdoruesi))
            RegjistroAuditLog(perdoruesi, "Fshi Perdorues", $"Id: {id}, Username: {emri}");
        return result;
    }

    // ==================== AUDIT LOG ====================

    public void RegjistroAuditLog(string perdoruesi, string veprimi, string detajet)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("INSERT INTO AuditLog (DataOra, Perdoruesi, Veprimi, Detajet) VALUES (@do, @p, @v, @d)", conn);
        cmd.Parameters.AddWithValue("@do", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("@p", perdoruesi);
        cmd.Parameters.AddWithValue("@v", veprimi);
        cmd.Parameters.AddWithValue("@d", detajet);
        cmd.ExecuteNonQuery();
    }

    public DataTable MerrAuditLog(int limit = 500)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        var dt = new DataTable();
        new SQLiteDataAdapter($"SELECT Id, DataOra, Perdoruesi, Veprimi, Detajet FROM AuditLog ORDER BY DataOra DESC LIMIT {limit}", conn).Fill(dt);
        return dt;
    }

    public DataTable KerkoAuditLog(string filter)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("SELECT Id, DataOra, Perdoruesi, Veprimi, Detajet FROM AuditLog WHERE Perdoruesi LIKE @f OR Veprimi LIKE @f OR Detajet LIKE @f ORDER BY DataOra DESC LIMIT 500", conn);
        cmd.Parameters.AddWithValue("@f", $"%{filter}%");
        var dt = new DataTable(); new SQLiteDataAdapter(cmd).Fill(dt); return dt;
    }

    // ==================== CILËSIMET (SETTINGS) ====================

    public DataTable MerrCilësimet()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        var dt = new DataTable();
        new SQLiteDataAdapter("SELECT Key, Value, Description, Category FROM Cilësimet ORDER BY Category, Key", conn).Fill(dt);
        return dt;
    }

    public string MerrCilësimin(string key, string defaultValue = "")
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("SELECT Value FROM Cilësimet WHERE Key=@k", conn);
        cmd.Parameters.AddWithValue("@k", key);
        var result = cmd.ExecuteScalar();
        return result?.ToString() ?? defaultValue;
    }

    public bool RuajCilësimin(string key, string value)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("INSERT OR REPLACE INTO Cilësimet (Key, Value) VALUES (@k, @v)", conn);
        cmd.Parameters.AddWithValue("@k", key);
        cmd.Parameters.AddWithValue("@v", value);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool FshiCilësimin(string key)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("DELETE FROM Cilësimet WHERE Key=@k", conn);
        cmd.Parameters.AddWithValue("@k", key);
        return cmd.ExecuteNonQuery() > 0;
    }

    public DataTable MerrTemplates()
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        var dt = new DataTable();
        new SQLiteDataAdapter("SELECT Id, Emri, Lloji, Parametrat, Aktive FROM FormaTemplates ORDER BY Lloji, Emri", conn).Fill(dt);
        return dt;
    }

    public int MerrTemplateAktivId(string lloji = "Fleteleshim")
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("SELECT Id FROM FormaTemplates WHERE Lloji=@l AND Aktive=1 LIMIT 1", conn);
        cmd.Parameters.AddWithValue("@l", lloji);
        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public string MerrTemplateParametrat(int id)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("SELECT Parametrat FROM FormaTemplates WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteScalar()?.ToString() ?? "{}";
    }

    public int ShtoTemplate(string emri, string lloji, string parametrat)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("INSERT INTO FormaTemplates (Emri, Lloji, Parametrat, Aktive) VALUES (@e, @l, @p, 0)", conn);
        cmd.Parameters.AddWithValue("@e", emri);
        cmd.Parameters.AddWithValue("@l", lloji);
        cmd.Parameters.AddWithValue("@p", parametrat);
        cmd.ExecuteNonQuery();
        return (int)conn.LastInsertRowId;
    }

    public bool FshiTemplate(int id)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("DELETE FROM FormaTemplates WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool NdryshoParametratTemplate(int id, string parametrat)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("UPDATE FormaTemplates SET Parametrat=@p WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@p", parametrat);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool AktivizoTemplate(int id, string lloji)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();
        // Deactivate all templates of this type
        using var deact = new SQLiteCommand("UPDATE FormaTemplates SET Aktive=0 WHERE Lloji=@l", conn);
        deact.Parameters.AddWithValue("@l", lloji);
        deact.ExecuteNonQuery();
        // Activate selected
        using var act = new SQLiteCommand("UPDATE FormaTemplates SET Aktive=1 WHERE Id=@id", conn);
        act.Parameters.AddWithValue("@id", id);
        act.ExecuteNonQuery();
        tx.Commit();
        return true;
    }

    public bool BackupDb(string destinationPath)
    {
        try
        {
            if (System.IO.File.Exists(destinationPath))
                System.IO.File.Delete(destinationPath);
            System.IO.File.Copy(_dbPath, destinationPath);
            return true;
        }
        catch { return false; }
    }

    public long MerrMadhesineDb()
    {
        try { return new System.IO.FileInfo(_dbPath).Length; }
        catch { return 0; }
    }

    public int MerrSchemaVersion()
    {
        try
        {
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using var cmd = new SQLiteCommand("SELECT COALESCE(MAX(Version), 0) FROM SchemaVersion", conn);
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch { return 0; }
    }

    public bool KaLeje(int userId, string permission)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("SELECT COUNT(*) FROM UserPermissions WHERE UserId=@uid AND Permission=@p", conn);
        cmd.Parameters.AddWithValue("@uid", userId);
        cmd.Parameters.AddWithValue("@p", permission);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public void ShtoLeje(int userId, string permission)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("INSERT OR IGNORE INTO UserPermissions(UserId, Permission) VALUES(@uid, @p)", conn);
        cmd.Parameters.AddWithValue("@uid", userId);
        cmd.Parameters.AddWithValue("@p", permission);
        cmd.ExecuteNonQuery();
    }

    public void LargoLeje(int userId, string permission)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("DELETE FROM UserPermissions WHERE UserId=@uid AND Permission=@p", conn);
        cmd.Parameters.AddWithValue("@uid", userId);
        cmd.Parameters.AddWithValue("@p", permission);
        cmd.ExecuteNonQuery();
    }

    public List<string> MerrLejet(int userId)
    {
        using var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        using var cmd = new SQLiteCommand("SELECT Permission FROM UserPermissions WHERE UserId=@uid", conn);
        cmd.Parameters.AddWithValue("@uid", userId);
        var list = new List<string>();
        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
            list.Add(rdr["Permission"]?.ToString() ?? "");
        return list;
    }

    public string[] MerrTeGjithaLejet()
    {
        return new[]
        {
            "Regjistro Transaksion", "Fshi Transaksion",
            "Shto/Ndrysho Arme", "Shto/Ndrysho Personel", "Shto/Ndrysho Klient",
            "Fshi Arme", "Fshi Personel", "Fshi Klient",
            "Printo Fleteleshim", "Listo Fleteleshime", "Liste Deponimi",
            "Raporto", "Eksporto",
            "Forma A4 Template",
            "Admin Panel", "Update Aplikacionit"
        };
    }
}
