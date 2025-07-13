using System.ComponentModel.DataAnnotations;

namespace CaseItau.API.Model
{
    public class MovimentacaoPatrimonio
    {
        [Required(ErrorMessage = "Operação é obrigatória")]
        [RegularExpression("^(ADD|SUB)$", ErrorMessage = "Operação deve ser ADD ou SUB")]
        public string Operation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Valor é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal Value { get; set; }
    }
}
