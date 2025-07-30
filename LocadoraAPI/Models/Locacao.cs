// Models/Locacao.cs
namespace LocadoraAPI.Models
{
    public class Locacao
    {
        public int IdLocacao { get; set; }
        public string CpfClienteFk { get; set; } = string.Empty;
        public int IdFuncionarioFk { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataDevolucaoPrevista { get; set; }
        public DateTime? DataDevolucaoEfetiva { get; set; }
        public decimal? PrecoTotal { get; set; }
    }
}