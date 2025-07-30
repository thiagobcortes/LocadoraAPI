// Controllers/LocacoesController.cs
using LocadoraAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace LocadoraAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocacoesController : ControllerBase
    {
        private readonly string _connectionString = string.Empty;

        public LocacoesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // POST: api/Locacoes (Cria uma nova locação)
        [HttpPost]
        public IActionResult RegistrarLocacao([FromBody] LocacaoRequest dadosLocacao)
        {
            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    using (var comando = new MySqlCommand("sp_RegistrarLocacao", conexao))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        comando.Parameters.AddWithValue("p_cpf_cliente", dadosLocacao.CpfCliente);
                        comando.Parameters.AddWithValue("p_id_funcionario", dadosLocacao.IdFuncionario);
                        comando.Parameters.AddWithValue("p_id_veiculo", dadosLocacao.IdVeiculo);
                        comando.Parameters.AddWithValue("p_data_devolucao_prevista", dadosLocacao.DataDevolucaoPrevista);

                        var p_id_locacao_criada = new MySqlParameter("p_id_locacao_criada", MySqlDbType.Int32)
                        {
                            Direction = ParameterDirection.Output
                        };
                        comando.Parameters.Add(p_id_locacao_criada);

                        var p_mensagem_erro = new MySqlParameter("p_mensagem_erro", MySqlDbType.VarChar, 255)
                        {
                            Direction = ParameterDirection.Output
                        };
                        comando.Parameters.Add(p_mensagem_erro);

                        comando.ExecuteNonQuery();

                        if (p_mensagem_erro.Value != DBNull.Value && p_mensagem_erro.Value != null)
                        {
                            return BadRequest(p_mensagem_erro.Value.ToString());
                        }
                        else
                        {
                            var novaLocacaoId = Convert.ToInt32(p_id_locacao_criada.Value);
                            return StatusCode(201, new { id_locacao = novaLocacaoId });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado na API: {ex.Message}");
            }
        }


        // PUT: api/Locacoes/{id}/devolver (Finaliza uma locação)
        [HttpPut("{id}/devolver")]
        public IActionResult DevolverVeiculo(int id)
        {
            MySqlConnection conexao = new MySqlConnection(_connectionString);
            MySqlTransaction transaction = null;

            try
            {
                conexao.Open();

                // Evita devolução duplicada
                string sqlCheck = "SELECT data_devolucao_efetiva FROM LOCACOES WHERE id_locacao = @id_locacao;";
                using (var comandoCheck = new MySqlCommand(sqlCheck, conexao))
                {
                    comandoCheck.Parameters.AddWithValue("@id_locacao", id);
                    var resultCheck = comandoCheck.ExecuteScalar();
                    if (resultCheck != null && resultCheck != DBNull.Value)
                    {
                        return BadRequest("Esta locação já foi finalizada anteriormente.");
                    }
                }

                transaction = conexao.BeginTransaction();

                // Passo 1: Obter o ID do veículo associado
                int idVeiculo = 0;
                string sqlGetVeiculoId = "SELECT id_veiculo_fk FROM LOCACAO_VEICULOS WHERE id_locacao_fk = @id_locacao;";
                using (var comandoGetId = new MySqlCommand(sqlGetVeiculoId, conexao, transaction))
                {
                    comandoGetId.Parameters.AddWithValue("@id_locacao", id);
                    var result = comandoGetId.ExecuteScalar();
                    if (result != null)
                    {
                        idVeiculo = Convert.ToInt32(result);
                    }
                    else
                    {
                        return NotFound("Nenhuma locação encontrada com este ID.");
                    }
                }

                // Passo 2: Registrar a data de devolução
                string sqlUpdateData = "UPDATE LOCACOES SET data_devolucao_efetiva = NOW() WHERE id_locacao = @id_locacao;";
                using (var comandoUpdateData = new MySqlCommand(sqlUpdateData, conexao, transaction))
                {
                    comandoUpdateData.Parameters.AddWithValue("@id_locacao", id);
                    comandoUpdateData.ExecuteNonQuery();
                }

                // Passo 3: Calcular e salvar o preço total
                string sqlUpdatePreco = "UPDATE LOCACOES SET preco_total = fn_CalcularPrecoFinal(@id_locacao) WHERE id_locacao = @id_locacao;";
                using (var comandoUpdatePreco = new MySqlCommand(sqlUpdatePreco, conexao, transaction))
                {
                    comandoUpdatePreco.Parameters.AddWithValue("@id_locacao", id);
                    comandoUpdatePreco.ExecuteNonQuery();
                }

                // Passo 4: Atualizar a disponibilidade do veículo
                string sqlUpdateVeiculo = "UPDATE VEICULOS SET disponibilidade = 'Disponível' WHERE id_veiculo = @id_veiculo;";
                using (var comandoUpdateVeiculo = new MySqlCommand(sqlUpdateVeiculo, conexao, transaction))
                {
                    comandoUpdateVeiculo.Parameters.AddWithValue("@id_veiculo", idVeiculo);
                    comandoUpdateVeiculo.ExecuteNonQuery();
                }

                transaction.Commit();

                return Ok("Devolução registrada com sucesso. Preço final calculado e veículo liberado.");
            }
            catch (Exception ex)
            {
                try
                {
                    transaction?.Rollback();
                }
                catch (MySqlException mysqlEx)
                {
                    Console.WriteLine($"Erro ao fazer rollback: {mysqlEx.Message}");
                }

                return StatusCode(500, $"Erro ao processar devolução: {ex.Message}");
            }
            finally
            {
                conexao.Close();
            }
        }
    }
}