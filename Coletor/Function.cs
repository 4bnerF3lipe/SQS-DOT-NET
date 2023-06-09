using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.DynamoDBv2.Model;
using Compartilhado.Model;
using Compartilhado;
using System.Runtime.CompilerServices;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Coletor;

public class Function
{
    public async void FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {

        foreach (var record in dynamoEvent.Records)
        {
            if (record.EventName == "INSERT")
            {
                var pedido = record.Dynamodb.NewImage.ToObject<Pedido>();
                pedido.Status = StatusPedido.Coletado;
                try
                {
                    await ProcessarValorDoPedido(pedido);
                }
                catch (Exception ex)
                {
                    pedido.JustificativaDeCancelamento = ex.Message;
                    pedido.Cancelado = true;
                    //Adicionar a fila de falha

                }
                pedido.SalvarAsync();
                //salvar o pedido
            }
        }


    }

    private async Task ProcessarValorDoPedido(Pedido pedido)
    {
        foreach (var produto in pedido.Produtos)
        {
            var produtoDoEstoque = await ObterProdutoDoDynamoDBAsync(produto.Id);
            if (produtoDoEstoque == null) throw new InvalidOperationException($"Produto n�o encontrado na tabela estoque. {produto.Id}");

            produto.Valor = produtoDoEstoque.Valor;
            produto.Nome = produtoDoEstoque.Nome;
        }

        var valorTotal = pedido.Produtos.Sum(x => x.Valor * x.Quantidade);
        if (pedido.ValorTotal != 0 && pedido.ValorTotal != valorTotal)
            throw new InvalidOperationException($"O valor esperado do pedido � de R$ {pedido.ValorTotal} e o valor verdadeiro � R$ {valorTotal}");

        pedido.ValorTotal = valorTotal;
    }

    private async Task<Produto> ObterProdutoDoDynamoDBAsync(string id)
    {
        var client = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
        var request = new QueryRequest()
        {
            TableName = "estoque",
            KeyConditionExpression = "�d = :v_id",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { "v_id", new AttributeValue { S = id } } }
        };
        var response = await client.QueryAsync(request);
        var item = response.Items.FirstOrDefault();
        if (item == null) return null;
        return item.ToObject<Produto>();
    }
}