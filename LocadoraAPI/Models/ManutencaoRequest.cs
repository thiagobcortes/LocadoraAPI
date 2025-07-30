// Models/ManutencaoRequest.cs
namespace LocadoraAPI.Models
{
    public class ManutencaoRequest
    {
        public int IdVeiculo { get; set; }
        public int IdFuncionario { get; set; }
        public string? Descricao { get; set; }
    }
}