// Models/Manutencao.cs
namespace LocadoraAPI.Models
{
    public class Manutencao
    {
        public int IdManutencao { get; set; }
        public int IdVeiculoFk { get; set; }
        public int IdFuncionarioFk { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string Descricao { get; set; } = string.Empty;
    }
}