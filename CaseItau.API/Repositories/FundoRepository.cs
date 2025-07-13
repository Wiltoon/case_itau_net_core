using CaseItau.API.Model;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CaseItau.API.Repositories
{
    public class FundoRepository : IFundoRepository
    {
        private readonly string _connectionString;

        public FundoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Data Source=dbCaseItau.s3db";
        }

        public async Task<IEnumerable<Fundo>> GetAllAsync()
        {
            var fundos = new List<Fundo>();
            
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT F.CODIGO, F.NOME, F.CNPJ, F.CODIGO_TIPO, F.PATRIMONIO, T.NOME AS NOME_TIPO 
                FROM FUNDO F 
                INNER JOIN TIPO_FUNDO T ON T.CODIGO = F.CODIGO_TIPO";
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                fundos.Add(MapFromReader(reader));
            }
            
            return fundos;
        }

        public async Task<Fundo?> GetByCodigoAsync(string codigo)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT F.CODIGO, F.NOME, F.CNPJ, F.CODIGO_TIPO, F.PATRIMONIO, T.NOME AS NOME_TIPO 
                FROM FUNDO F 
                INNER JOIN TIPO_FUNDO T ON T.CODIGO = F.CODIGO_TIPO 
                WHERE F.CODIGO = @codigo";
            
            command.Parameters.AddWithValue("@codigo", codigo);
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }
            
            return null;
        }

        public async Task CreateAsync(Fundo fundo)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO FUNDO (CODIGO, NOME, CNPJ, CODIGO_TIPO, PATRIMONIO) 
                VALUES (@codigo, @nome, @cnpj, @codigoTipo, @patrimonio)";
            
            AddParameters(command, fundo);
            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(string codigo, Fundo fundo)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE FUNDO 
                SET NOME = @nome, CNPJ = @cnpj, CODIGO_TIPO = @codigoTipo, PATRIMONIO = @patrimonio 
                WHERE CODIGO = @codigo";
            
            command.Parameters.AddWithValue("@codigo", codigo);
            AddParameters(command, fundo);
            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(string codigo)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM FUNDO WHERE CODIGO = @codigo";
            command.Parameters.AddWithValue("@codigo", codigo);
            
            await command.ExecuteNonQueryAsync();
        }

        public async Task MovimentarPatrimonioAsync(string codigo, decimal valor)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE FUNDO 
                SET PATRIMONIO = COALESCE(PATRIMONIO, 0) + @valor 
                WHERE CODIGO = @codigo";
            
            command.Parameters.AddWithValue("@codigo", codigo);
            command.Parameters.AddWithValue("@valor", valor);
            
            await command.ExecuteNonQueryAsync();
        }

        private static Fundo MapFromReader(SqliteDataReader reader)
        {
            return new Fundo
            {
                Codigo = reader["CODIGO"].ToString() ?? string.Empty,
                Nome = reader["NOME"].ToString() ?? string.Empty,
                Cnpj = reader["CNPJ"].ToString() ?? string.Empty,
                CodigoTipo = Convert.ToInt32(reader["CODIGO_TIPO"]),
                Patrimonio = reader["PATRIMONIO"] != DBNull.Value ? Convert.ToDecimal(reader["PATRIMONIO"]) : null,
                NomeTipo = reader["NOME_TIPO"].ToString() ?? string.Empty
            };
        }

        private static void AddParameters(SqliteCommand command, Fundo fundo)
        {
            command.Parameters.AddWithValue("@codigo", fundo.Codigo);
            command.Parameters.AddWithValue("@nome", fundo.Nome);
            command.Parameters.AddWithValue("@cnpj", fundo.Cnpj);
            command.Parameters.AddWithValue("@codigoTipo", fundo.CodigoTipo);
            command.Parameters.AddWithValue("@patrimonio", (object?)fundo.Patrimonio ?? DBNull.Value);
        }
    }
}
