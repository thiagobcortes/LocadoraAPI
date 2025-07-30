// Models/LocacaoRequest.cs

// Classe auxiliar para representar os dados da requisição de locação

namespace LocadoraAPI.Models
{
    public class LocacaoRequest
    {
        public string CpfCliente { get; set; } = string.Empty;
        public int IdFuncionario { get; set; }
        public int IdVeiculo { get; set; }
        public DateTime DataDevolucaoPrevista { get; set; }
    }
}