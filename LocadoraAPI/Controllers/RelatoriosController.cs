// Controllers/RelatoriosController.cs
using LocadoraAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace LocadoraAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelatoriosController : ControllerBase
    {
        private readonly string _connectionString = string.Empty;

        public RelatoriosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/Relatorios/LocacoesAtivas
        [HttpGet("LocacoesAtivas")]
        public IActionResult GetRelatorioLocacoesAtivas()
        {
            var relatorio = new List<RelatorioLocacaoAtiva>();
            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    
                    string sql = "SELECT * FROM vw_RelatorioLocacoesAtivas;";
                    using (var comando = new MySqlCommand(sql, conexao))
                    {
                        using (var reader = comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                relatorio.Add(new RelatorioLocacaoAtiva
                                {
                                    IdLocacao = reader.GetInt32("id_locacao"),
                                    DataInicio = reader.GetDateTime("data_inicio"),
                                    DataDevolucaoPrevista = reader.GetDateTime("data_devolucao_prevista"),
                                    CpfCliente = reader.GetString("cpf_cliente"),
                                    NomeCliente = reader.GetString("nome_cliente"),
                                    PlacaVeiculo = reader.GetString("placa_veiculo"),
                                    MarcaVeiculo = reader.GetString("marca_veiculo"),
                                    ModeloVeiculo = reader.GetString("modelo_veiculo"),
                                    NomeFuncionario = reader.GetString("nome_funcionario")
                                });
                            }
                        }
                    }
                }
                return Ok(relatorio);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao acessar o banco de dados: {ex.Message}");
            }
        }
    }
}