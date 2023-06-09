using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Compartilhado.Model
{
    public enum StatusPedido
    {
        Coletado,
        Pago,
        Faturado
    }
    [DynamoDBTable("Pedidos")]
    public class Pedido
    {
        public string? Id { get; set; }

        public decimal ValorTotal { get; set; }

        public DateTime DataDeCriacao { get; set; }

        public List<Produto>? Produtos { get; set; }

        public Cliente? Cliente { get; set; }

        public Pagamento? Pagamento { get; set; }

        public string? JustificativaDeCancelamento { get; set; }

        [JsonConverter(typeof(StringConverter))]
        public StatusPedido Status { get; set; }

        public bool Cancelado { get; set; }


    }
}
