using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Compartilhado;
using Compartilhado.Model;
using Newtonsoft.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Reservador;

public class Function
{
    private AmazonDynamoDBClient _amazonDynamoDBClient;
    public Function()
    {
        _amazonDynamoDBClient = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
    }

    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        if (evnt.Records.Count > 1) throw new InvalidOperationException("Somente uma Mensagem pode ser tratada por vez");
        var message = evnt.Records.FirstOrDefault();
        if (message == null) return;
        await ProcessMessageAsync(message, context);
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        var pedido = JsonConvert.DeserializeObject<Pedido>(message.Body);
        pedido.Status = StatusPedido.Reservado;
        foreach (var produto in pedido.Produtos)
        {
            try
            {
                await BaixarEstoque(produto.Id, produto.Quantidade);
                produto.Reservado = true;
                context.Logger.LogLine($"Produto baixado do estoque {produto.Id} - {produto.Nome}");

            }
            catch (ConditionalCheckFailedException ex)
            {
                pedido.JustificativaDeCancelamento = $"Produto Indisponivel no estoque {produto.Id} - {produto.Nome}";
                pedido.Cancelado = true;
                context.Logger.LogLine($"ERRO: Produto Indisponivel no estoque {produto.Id} - {produto.Nome}");
                break;
            }
        }
 

        if (pedido.Cancelado)
        {
            foreach (var produto in pedido.Produtos)
            {
                if (produto.Reservado)
                {
                    await DevolverAoEstoque(produto.Id, produto.Quantidade);
                }
            }

            await AmazonUtil.EnviarParaFila(EnumFilasSNS.falha, pedido);
            
        }
        else
        {
            await AmazonUtil.EnviarParaFila(EnumFilasSQS.reservado, pedido);
        }
        await pedido.SalvarAsync();

    }

    private Task DevolverAoEstoque(string id, int quantidade)
    {
        throw new NotImplementedException();
    }

    private Task BaixarEstoque(string id, int quantidade)
    {
        throw new NotImplementedException();
    }
}