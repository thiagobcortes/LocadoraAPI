// Controllers/ManutencoesController.cs
using LocadoraAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace LocadoraAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManutencoesController : ControllerBase
    {
        private readonly string _connectionString = string.Empty;

        public ManutencoesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/Manutencoes (Listar todas as manutenções)
        [HttpGet]
        public IActionResult GetManutencoes()
        {
            var manutencoes = new List<Manutencao>();
            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    string sql = "SELECT * FROM MANUTENCOES;";
                    using (var comando = new MySqlCommand(sql, conexao))
                    {
                        using (var reader = comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                manutencoes.Add(new Manutencao
                                {
                                    IdManutencao = reader.GetInt32("id_manutencao"),
                                    IdVeiculoFk = reader.GetInt32("id_veiculo_fk"),
                                    IdFuncionarioFk = reader.GetInt32("id_funcionario_fk"),
                                    DataInicio = reader.GetDateTime("data_inicio"),
                                    DataFim = reader.IsDBNull(reader.GetOrdinal("data_fim")) ? null : reader.GetDateTime("data_fim"),
                                    Descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? null : reader.GetString("descricao")
                                });
                            }
                        }
                    }
                }
                return Ok(manutencoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao acessar o banco de dados: {ex.Message}");
            }
        }

        // POST: api/Manutencoes (Inicia uma nova manutenção)
        [HttpPost]
        public IActionResult IniciarManutencao([FromBody] ManutencaoRequest dadosManutencao)
        {
            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    string statusAtual = "";
                    string sqlCheck = "SELECT disponibilidade FROM VEICULOS WHERE id_veiculo = @id_veiculo;";
                    using (var comandoCheck = new MySqlCommand(sqlCheck, conexao))
                    {
                        comandoCheck.Parameters.AddWithValue("@id_veiculo", dadosManutencao.IdVeiculo);
                        var result = comandoCheck.ExecuteScalar();
                        if (result != null) { statusAtual = result.ToString(); }
                        else { return NotFound($"Veículo com ID {dadosManutencao.IdVeiculo} não encontrado."); }
                    }

                    if (statusAtual != "Disponível")
                    {
                        return Conflict($"Veículo não está 'Disponível' para iniciar uma manutenção. Status atual: {statusAtual}");
                    }

                    string sql = "INSERT INTO MANUTENCOES (id_veiculo_fk, id_funcionario_fk, data_inicio, descricao) VALUES (@id_veiculo, @id_funcionario, @data_inicio, @descricao);";
                    using (var comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@id_veiculo", dadosManutencao.IdVeiculo);
                        comando.Parameters.AddWithValue("@id_funcionario", dadosManutencao.IdFuncionario);
                        comando.Parameters.AddWithValue("@data_inicio", DateTime.Now);
                        comando.Parameters.AddWithValue("@descricao", dadosManutencao.Descricao);
                        comando.ExecuteNonQuery();
                        return StatusCode(201, "Manutenção iniciada. O trigger atualizou o status do veículo para 'Indisponível'.");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao acessar o banco de dados: {ex.Message}");
            }
        }

        // PUT: api/Manutencoes/{id}/finalizar
        [HttpPut("{id}/finalizar")]
        public IActionResult FinalizarManutencao(int id)
        {
            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    int idVeiculo = 0;
                    string sqlGetVeiculo = "SELECT id_veiculo_fk FROM MANUTENCOES WHERE id_manutencao = @id_manutencao;";
                    using (var comandoGet = new MySqlCommand(sqlGetVeiculo, conexao))
                    {
                        comandoGet.Parameters.AddWithValue("@id_manutencao", id);
                        var result = comandoGet.ExecuteScalar();
                        if (result != null) { idVeiculo = Convert.ToInt32(result); }
                        else { return NotFound("Registro de manutenção não encontrado."); }
                    }

                    string sqlUpdateManutencao = "UPDATE MANUTENCOES SET data_fim = NOW() WHERE id_manutencao = @id_manutencao;";
                    using (var comandoUpdate = new MySqlCommand(sqlUpdateManutencao, conexao))
                    {
                        comandoUpdate.Parameters.AddWithValue("@id_manutencao", id);
                        comandoUpdate.ExecuteNonQuery();
                    }

                    string sqlUpdateVeiculo = @"
                        UPDATE VEICULOS v
                        LEFT JOIN (
                            SELECT lv.id_veiculo_fk
                            FROM LOCACOES l
                            JOIN LOCACAO_VEICULOS lv ON l.id_locacao = lv.id_locacao_fk
                            WHERE l.data_devolucao_efetiva IS NULL
                        ) AS locacoes_ativas ON v.id_veiculo = locacoes_ativas.id_veiculo_fk
                        SET v.disponibilidade = 'Disponível'
                        WHERE v.id_veiculo = @id_veiculo AND locacoes_ativas.id_veiculo_fk IS NULL;";

                    using (var comandoUpdateVeiculo = new MySqlCommand(sqlUpdateVeiculo, conexao))
                    {
                        comandoUpdateVeiculo.Parameters.AddWithValue("@id_veiculo", idVeiculo);
                        comandoUpdateVeiculo.ExecuteNonQuery();
                    }

                    return Ok("Manutenção finalizada e veículo liberado.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao acessar o banco de dados: {ex.Message}");
            }
        }
    }

}