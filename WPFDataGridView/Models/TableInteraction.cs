using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Npgsql;
using NpgsqlTypes;

namespace WPFDataGridView.Models {
    public class TableInteraction<T> where T : ITable {
        private readonly string _connectionString = new NpgsqlConnectionStringBuilder {
            Host = Properties.Settings.Default.Host,
            Port = Properties.Settings.Default.Port,
            Database = Properties.Settings.Default.Name,
            Username = Environment.GetEnvironmentVariable("POSTGRES_USERNAME"),
            Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD"),
        }.ConnectionString;

        private readonly string _tableName;

        private readonly Column[] _columns;

        public TableInteraction(string tableName, Column[] columns) {
            _tableName = tableName;
            _columns = columns;
        }

        public void InitializeToTable(IList<T> tuples) {
            using (var connection = new NpgsqlConnection(_connectionString)) {
                connection.Open();

                var command = new NpgsqlCommand {
                    Connection = connection,
                    CommandText = $"SELECT {GetColumnsString(column => true)} FROM {_tableName};"
                };

                using (NpgsqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        var tuple = (T) Activator.CreateInstance(typeof(T));
                        foreach (Column column in _columns) {
                            tuple[column.Name] = reader[column.Name];
                        }

                        tuples.Add(tuple);
                    }
                }
            }
        }

        public void AddRowToTable(T tuple) {
            using (var connection = new NpgsqlConnection(_connectionString)) {
                connection.Open();

                var command = new NpgsqlCommand {
                    Connection = connection,
                    CommandText =
                        $"INSERT INTO {_tableName} ({GetColumnsString(column => !column.IsSerial)}) VALUES ({GetValuesTemplateForAdd()})" +
                        (_columns.Any(column => column.IsSerial)
                            ? $" RETURNING {GetColumnsString(column => column.IsSerial)};"
                            : ";")
                };

                foreach (Column column in _columns.Where(column => !column.IsSerial)) {
                    command.Parameters.AddWithValue("@" + column.Name, tuple[column.Name]);
                }

                using (NpgsqlDataReader reader = command.ExecuteReader()) {
                    reader.Read();
                    foreach (Column column in _columns.Where(column => column.IsSerial)) {
                        tuple[column.Name] = reader[column.Name];
                    }
                }
            }
        }

        public void UpdateRowToTable(T tuple) {
            using (var connection = new NpgsqlConnection(_connectionString)) {
                connection.Open();

                var command = new NpgsqlCommand {
                    Connection = connection,
                    CommandText =
                        $"UPDATE {_tableName} SET {GetSetsTemplateForUpdate()} WHERE {GetPrimaryKeyTemplate()};"
                };

                foreach (Column column in _columns) {
                    command.Parameters.AddWithValue("@" + column.Name, tuple[column.Name]);
                }
                foreach (Column column in _columns.Where(column => column.IsPrimaryKeyPart)) {
                    command.Parameters.AddWithValue("@" + column.Name + "_primary_key_part", tuple[column.Name, true]);
                }

                command.ExecuteNonQuery();
            }
        }

        public void DeleteRowFromTable(T tuple) {
            using (var connection = new NpgsqlConnection(_connectionString)) {
                connection.Open();

                var command = new NpgsqlCommand {
                    Connection = connection,
                    CommandText =
                        $"DELETE FROM {_tableName} WHERE {GetPrimaryKeyTemplate()};"
                };

                foreach (Column column in _columns.Where(column => column.IsPrimaryKeyPart)) {
                    command.Parameters.AddWithValue("@" + column.Name + "_primary_key_part", tuple[column.Name]);
                }

                command.ExecuteNonQuery();
            }
        }

        private string GetColumnsString(Func<Column, bool> wherePredicate) {
            return _columns.Where(wherePredicate).Select(column => column.Name)
                           .Aggregate((source, columnName) => source + ", " + columnName);
        }


        private string GetValuesTemplateForAdd() {
            return _columns.Where(column => !column.IsSerial)
                           .Select(column => column.Name)
                           .Aggregate(string.Empty, (source, columnName) => source + ", @" + columnName)
                           .Substring(1);
        }

        private string GetSetsTemplateForUpdate() {
            return _columns.Select(column => column.Name)
                           .Aggregate(string.Empty,
                                      (source, columnName) => source + $", {columnName} = @{columnName}")
                           .Substring(1);
        }

        private string GetPrimaryKeyTemplate() {
            return _columns.Where(column => column.IsPrimaryKeyPart).Select(column => column.Name)
                           .Aggregate(string.Empty,
                                      (source, columnName) =>
                                          source + $", {columnName} = @{columnName}_primary_key_part")
                           .Substring(1);
        }
    }
}