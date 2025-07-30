// Models/RelatorioLocacaoAtiva.cs
namespace LocadoraAPI.Models
{
    public class RelatorioLocacaoAtiva
    {
        public int IdLocacao { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataDevolucaoPrevista { get; set; }
        public string CpfCliente { get; set; } = string.Empty;
        public string NomeCliente { get; set; } = string.Empty;
        public string PlacaVeiculo { get; set; } = string.Empty;
        public string MarcaVeiculo { get; set; } = string.Empty;
        public string ModeloVeiculo { get; set; } = string.Empty;
        public string NomeFuncionario { get; set; } = string.Empty;
    }
}