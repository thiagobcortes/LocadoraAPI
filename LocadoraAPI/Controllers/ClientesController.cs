// Controllers/ClientesController.cs
using LocadoraAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace LocadoraAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly string _connectionString = string.Empty;

        public ClientesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/Clientes (Listar todos)
        [HttpGet]
        public IActionResult GetClientes()
        {
            var clientes = new List<Cliente>();
            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    string sql = "SELECT cpf, nome, email, telefone FROM CLIENTES;";
                    using (var comando = new MySqlCommand(sql, conexao))
                    {
                        using (var reader = comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                clientes.Add(new Cliente
                                {
                                    Cpf = reader.GetString("cpf"),
                                    Nome = reader.GetString("nome"),
                                    Email = reader.GetString("email"),
                                    Telefone = reader.IsDBNull(reader.GetOrdinal("telefone")) ? null : reader.GetString("telefone")
                                });
                            }
                        }
                    }
                }
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao acessar o banco de dados: {ex.Message}");
            }
        }

        // GET: api/Clientes/{cpf} (Buscar por CPF)
        [HttpGet("{cpf}")]
        public IActionResult GetClientePorCpf(string cpf)
        {
            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    string sql = "SELECT cpf, nome, email, telefone FROM CLIENTES WHERE cpf = @cpf;";
                    using (var comando = new MySqlCommand(sql, conexao))
                    {
                        
                        comando.Parameters.AddWithValue("@cpf", cpf);

                        using (var reader = comando.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var cliente = new Cliente
                                {
                                    Cpf = reader.GetString("cpf"),
                                    Nome = reader.GetString("nome"),
                                    Email = reader.GetString("email"),
                                    Telefone = reader.IsDBNull(reader.GetOrdinal("telefone")) ? null : reader.GetString("telefone")
                                };
                                return Ok(cliente);
                            }
                        }
                    }
                }
                return NotFound("Cliente não encontrado."); // Retorna 404 se não achar o cliente
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao acessar o banco de dados: {ex.Message}");
            }
        }

        // POST: api/Clientes (Criar novo cliente)
        [HttpPost]
        public IActionResult PostCliente([FromBody] Cliente novoCliente)
        {
            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    string sql = "INSERT INTO CLIENTES (cpf, nome, email, telefone) VALUES (@cpf, @nome, @email, @telefone);";
                    using (var comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@cpf", novoCliente.Cpf);
                        comando.Parameters.AddWithValue("@nome", novoCliente.Nome);
                        comando.Parameters.AddWithValue("@email", novoCliente.Email);
                        comando.Parameters.AddWithValue("@telefone", novoCliente.Telefone);

                        int linhasAfetadas = comando.ExecuteNonQuery();

                        if (linhasAfetadas > 0)
                        {
                            // Retorna 201 Created com o cliente criado
                            return CreatedAtAction(nameof(GetClientePorCpf), new { cpf = novoCliente.Cpf }, novoCliente);
                        }
                    }
                }
                return BadRequest("Não foi possível criar o cliente.");
            }
            catch (MySqlException ex) when (ex.Number == 1062) // Código de erro para entrada duplicada
            {
                return Conflict($"Já existe um cliente com o CPF {novoCliente.Cpf} ou Email {novoCliente.Email}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao acessar o banco de dados: {ex.Message}");
            }
        }

        // PUT: api/Clientes/{cpf} (Atualizar cliente)
        [HttpPut("{cpf}")]
        public IActionResult PutCliente(string cpf, [FromBody] Cliente clienteAtualizado)
        {
            if (cpf != clienteAtualizado.Cpf)
            {
                return BadRequest("O CPF da URL não corresponde ao CPF do corpo da requisição.");
            }

            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    string sql = "UPDATE CLIENTES SET nome = @nome, email = @email, telefone = @telefone WHERE cpf = @cpf;";
                    using (var comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@nome", clienteAtualizado.Nome);
                        comando.Parameters.AddWithValue("@email", clienteAtualizado.Email);
                        comando.Parameters.AddWithValue("@telefone", clienteAtualizado.Telefone);
                        comando.Parameters.AddWithValue("@cpf", cpf);

                        int linhasAfetadas = comando.ExecuteNonQuery();

                        if (linhasAfetadas > 0)
                        {
                            return NoContent(); // Retorna 204 NoContent em caso de sucesso
                        }
                        else
                        {
                            return NotFound("Cliente não encontrado para atualização.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao acessar o banco de dados: {ex.Message}");
            }
        }

        // DELETE: api/Clientes/{cpf} (Deletar cliente)
        [HttpDelete("{cpf}")]
        public IActionResult DeleteCliente(string cpf)
        {
            try
            {
                using (var conexao = new MySqlConnection(_connectionString))
                {
                    conexao.Open();
                    string sql = "DELETE FROM CLIENTES WHERE cpf = @cpf;";
                    using (var comando = new MySqlCommand(sql, conexao))
                    {
                        comando.Parameters.AddWithValue("@cpf", cpf);

                        int linhasAfetadas = comando.ExecuteNonQuery();

                        if (linhasAfetadas > 0)
                        {
                            return NoContent(); // Retorna 204 NoContent em caso de sucesso
                        }
                        else
                        {
                            return NotFound("Cliente não encontrado para exclusão.");
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