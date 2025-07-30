// Models/Veiculo.cs
namespace LocadoraAPI.Models
{
    public class Veiculo
    {
        public int IdVeiculo { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Ano { get; set; }
        public string Cor { get; set; } = string.Empty;
        public decimal PrecoDiaria { get; set; }
        public string Disponibilidade { get; set; } = string.Empty;
    }
}