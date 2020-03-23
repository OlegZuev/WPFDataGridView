using System;
using System.Collections.Generic;
using Npgsql;

namespace WPFDataGridView.Models {
    public class DatabaseInteraction {
        private static readonly string _connectionString = new NpgsqlConnectionStringBuilder {
            Host = Properties.Settings.Default.Host,
            Port = Properties.Settings.Default.Port,
            Database = Properties.Settings.Default.Name,
            Username = Environment.GetEnvironmentVariable("POSTGRES_USERNAME"),
            Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD"),
        }.ConnectionString;

        public static void InitializeToBooks(IList<Book> books) {
            using (var connection = new NpgsqlConnection(_connectionString)) {
                connection.Open();

                var command = new NpgsqlCommand {
                    Connection = connection,
                    CommandText = "SELECT isbn, publisher_id, name, release_date, age_restriction FROM books;"
                };

                // using (NpgsqlDataReader reader = command.ExecuteReader()) {
                //     while (reader.Read()) {
                //         books.Add(new ITable().CreateFromDatabase(reader));
                //     }
                // }
            }
        }
    }
}