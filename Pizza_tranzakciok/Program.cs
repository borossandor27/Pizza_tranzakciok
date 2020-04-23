using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Pizza_tranzakciok
{
    class Program
    {
        static MySqlConnection conn = null;
        static MySqlCommand sql = null;
        static MySqlTransaction trans = null;

        static readonly int MAX_FUTAR = 5;
        static readonly int MAX_PIZZA = 5;
        static readonly int MAX_VEVO = 7;

        static List<Rendeles> rendelesek = new List<Rendeles>();
        static void Main(string[] args)
        {
            MySqlConnectionStringBuilder sb = new MySqlConnectionStringBuilder();
            sb.Server = "localhost";
            sb.Database = "pizza2";
            sb.UserID = "root";
            sb.Password = "";
            conn = new MySqlConnection(sb.ToString());
            conn.Open();
            sql = conn.CreateCommand();

            Uj_rendelesek();

            foreach (Rendeles item in rendelesek)
            {
                try
                {
                    trans = conn.BeginTransaction();
                    Program.sql.CommandText = "INSERT INTO `prendeles` (`razon`, `vazon`, `fazon`, `datum`) VALUES (NULL, @vazon, @fazon, @datum); ";
                    Program.sql.Parameters.Clear();
                    Program.sql.Parameters.AddWithValue("@vazon", item.vazon);
                    Program.sql.Parameters.AddWithValue("@fazon", item.fazon);
                    Program.sql.Parameters.AddWithValue("@datum", item.datum);
                    Program.sql.ExecuteNonQuery();
                    item.razon = Convert.ToInt32(Program.sql.LastInsertedId);
                    //item.razon = 55;
                    //-- A tételek rögzítése
                    foreach (Tetel tetel in item.tetelek)
                    {
                        Program.sql.CommandText = "INSERT INTO `ptetel` (`razon`, `pazon`, `db`) VALUES (@razon, @pazon, @db); ";
                        Program.sql.Parameters.Clear();
                        Program.sql.Parameters.AddWithValue("@razon", item.razon);
                        Program.sql.Parameters.AddWithValue("@pazon", tetel.pazon);
                        Program.sql.Parameters.AddWithValue("@db", tetel.db);
                        Program.sql.ExecuteNonQuery();
                    }

                    trans.Commit(); //-- Az adatbázisműveletek rögzítése
                    Console.WriteLine("Az adatok rögzítése MINDKÉT TÁBLÁBAN sikeres!");
                }
                catch (MySqlException ex)
                {
                    trans.Rollback(); //-- A megkezdett műveletek elvetése MINDKÉT TÁBLÁBAN!
                    Console.WriteLine(ex.Message + "\n\nAz adatok rögzítése sikertelen!");
                    Console.ReadKey();
                    //return; //-- Program vége!
                }
            }

            Console.WriteLine("\nProgram vége!");
            Console.ReadKey();
        }
        static void Vevo_lista()
        {
            sql.CommandText = "SELECT `vazon`,`vnev`,`vcim` FROM `pvevo` ";
            using (MySqlDataReader dr=sql.ExecuteReader())
            {
                Console.WriteLine("\nVevők:");
                while (dr.Read())
                {
                    Console.WriteLine($"\t{dr.GetInt32("vazon")},\t{dr.GetString("vnev")}, \t{dr.GetString("vcim")}");
                }
            }
        }        
        static void Rendeles_lista_beolvas()
        {
            sql.CommandText = "SELECT `vazon`,`vnev`,`vcim` FROM `pvevo` ";
            using (MySqlDataReader dr=sql.ExecuteReader())
            {
                Console.WriteLine("\nVevők:");
                while (dr.Read())
                {
                    Console.WriteLine($"\t{dr.GetInt32("vazon")},\t{dr.GetString("vnev")}, \t{dr.GetString("vcim")}");
                }
            }
        }

        static void Uj_rendelesek()
        {
            Random r = new Random();
            int rendelesszam = r.Next(5) + 3; //-- Előállítandó rendelések száma
            for (int i = 0; i < rendelesszam; i++)
            {
                Rendeles uj = new Rendeles();
                uj.fazon = r.Next(MAX_FUTAR) + 1;
                uj.vazon = r.Next(MAX_VEVO) + 1;
                int tetelszam = r.Next(4) + 1;
                List<Tetel> tetelek = new List<Tetel>();
                for (int j = 0; j < tetelszam; j++)
                {
                    Tetel uj_tetel = new Tetel();
                    uj_tetel.pazon = r.Next(MAX_PIZZA) + 1;
                    uj_tetel.db = r.Next(2) + 1;
                    tetelek.Add(uj_tetel);
                }
                uj.tetelek = tetelek;
                rendelesek.Add(uj);
            }
        }
    }
}
