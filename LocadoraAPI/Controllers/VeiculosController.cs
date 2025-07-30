// Controllers/VeiculosController.cs
using LocadoraAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace LocadoraAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VeiculosController : ControllerBase
    {
        private readonly string _connectionString = string.Empty;

        public VeiculosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/Veiculos
        [HttpGet]
        public IActionResult GetVeiculos()
        {
            var veiculos = new List<Veiculo>();
            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    string sql = "SELECT * FROM VEICULOS;";
                    using (var comando = new MySqlCommand(sql, conexao))
                    {
                        using (var reader = comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                veiculos.Add(new Veiculo
                                {
                                    IdVeiculo = reader.GetInt32("id_veiculo"),
                                    Placa = reader.GetString("placa"),
                                    Marca = reader.GetString("marca"),
                                    Modelo = reader.GetString("modelo"),
                                    Ano = reader.GetInt32("ano"),
                                    Cor = reader.GetString("cor"),
                                    PrecoDiaria = reader.GetDecimal("preco_diaria"),
                                    Disponibilidade = reader.GetString("disponibilidade")
                                });
                            }
                        }
                    }
                }
                return Ok(veiculos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao acessar o banco de dados: {ex.Message}");
            }
        }

        // GET: api/Veiculos/{id}
        [HttpGet("{id}")]
        public IActionResult GetVeiculoPorId(int id)
        {
            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    string sql = "SELECT * FROM VEICULOS WHERE id_veiculo = @id;";
                    using (var comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        using (var reader = comando.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var veiculo = new Veiculo
                                {
                                    IdVeiculo = reader.GetInt32("id_veiculo"),
                                    Placa = reader.GetString("placa"),
                                    Marca = reader.GetString("marca"),
                                    Modelo = reader.GetString("modelo"),
                                    Ano = reader.GetInt32("ano"),
                                    Cor = reader.GetString("cor"),
                                    PrecoDiaria = reader.GetDecimal("preco_diaria"),
                                    Disponibilidade = reader.GetString("disponibilidade")
                                };
                                return Ok(veiculo);
                            }
                        }
                    }
                }
                return NotFound("Veículo não encontrado.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao acessar o banco de dados: {ex.Message}");
            }
        }

        // POST: api/Veiculos
        [HttpPost]
        public IActionResult PostVeiculo([FromBody] Veiculo novoVeiculo)
        {
            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    // O comando SELECT LAST_INSERT_ID() é específico do MySQL para retornar o ID que foi auto-incrementado.
                    string sql = "INSERT INTO VEICULOS (placa, marca, modelo, ano, cor, preco_diaria, disponibilidade) VALUES (@placa, @marca, @modelo, @ano, @cor, @preco_diaria, @disponibilidade); SELECT LAST_INSERT_ID();";
                    using (var comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@placa", novoVeiculo.Placa);
                        comando.Parameters.AddWithValue("@marca", novoVeiculo.Marca);
                        comando.Parameters.AddWithValue("@modelo", novoVeiculo.Modelo);
                        comando.Parameters.AddWithValue("@ano", novoVeiculo.Ano);
                        comando.Parameters.AddWithValue("@cor", novoVeiculo.Cor);
                        comando.Parameters.AddWithValue("@preco_diaria", novoVeiculo.PrecoDiaria);
                        comando.Parameters.AddWithValue("@disponibilidade", novoVeiculo.Disponibilidade);

                        // ExecuteScalar é usado aqui para obter o primeiro valor da primeira linha do resultado (o ID do novo veículo).
                        novoVeiculo.IdVeiculo = Convert.ToInt32(comando.ExecuteScalar());

                        return CreatedAtAction(nameof(GetVeiculoPorId), new { id = novoVeiculo.IdVeiculo }, novoVeiculo);
                    }
                }
            }
            catch (MySqlException ex) when (ex.Number == 1062) // Código de erro para entrada duplicada (neste caso, a placa).
            {
                return Conflict($"Já existe um veículo com a placa {novoVeiculo.Placa}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao acessar o banco de dados: {ex.Message}");
            }
        }

        // PUT: api/Veiculos/{id}
        [HttpPut("{id}")]
        public IActionResult PutVeiculo(int id, [FromBody] Veiculo veiculoAtualizado)
        {
            if (id != veiculoAtualizado.IdVeiculo)
            {
                return BadRequest("O ID da URL não corresponde ao ID do corpo da requisição.");
            }

            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    string sql = "UPDATE VEICULOS SET placa = @placa, marca = @marca, modelo = @modelo, ano = @ano, cor = @cor, preco_diaria = @preco_diaria, disponibilidade = @disponibilidade WHERE id_veiculo = @id;";
                    using (var comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        comando.Parameters.AddWithValue("@placa", veiculoAtualizado.Placa);
                        comando.Parameters.AddWithValue("@marca", veiculoAtualizado.Marca);
                        comando.Parameters.AddWithValue("@modelo", veiculoAtualizado.Modelo);
                        comando.Parameters.AddWithValue("@ano", veiculoAtualizado.Ano);
                        comando.Parameters.AddWithValue("@cor", veiculoAtualizado.Cor);
                        comando.Parameters.AddWithValue("@preco_diaria", veiculoAtualizado.PrecoDiaria);
                        comando.Parameters.AddWithValue("@disponibilidade", veiculoAtualizado.Disponibilidade);

                        int linhasAfetadas = comando.ExecuteNonQuery();

                        if (linhasAfetadas > 0)
                        {
                            return NoContent();
                        }
                        else
                        {
                            return NotFound("Veículo não encontrado para atualização.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao acessar o banco de dados: {ex.Message}");
            }
        }

        // DELETE: api/Veiculos/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteVeiculo(int id)
        {
            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    string sql = "DELETE FROM VEICULOS WHERE id_veiculo = @id;";
                    using (var comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@id", id);

                        int linhasAfetadas = comando.ExecuteNonQuery();

                        if (linhasAfetadas > 0)
                        {
                            return NoContent();
                        }
                        else
                        {
                            return NotFound("Veículo não encontrado para exclusão.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao acessar o banco de dados: {ex.Message}");
            }
        }
    }
}