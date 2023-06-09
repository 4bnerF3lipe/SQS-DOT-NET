using Amazon;
using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.SQS.Model.Internal.MarshallTransformations;
using Compartilhado.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Compartilhado
{
    public static class AmazonUtil
    {
        public static async Task SalvarAsync(this Pedido pedido)
        {
            var client = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
            var context = new DynamoDBContext(client) ;
            await context.SaveAsync(pedido);
        }
    
        public static T ToObject<T>(this Dictionary<string, AttributeValue> dictionary)
        {
            var doc = Document.FromAttributeMap(dictionary);
            var client = new AmazonDynamoDBClient(RegionEndpoint.SAEast1);
            var context = new DynamoDBContext(client);
            return context.FromDocument<T>(doc);
        }

        public static async Task EnviarParaFila(EnumFilasSQS fila, Pedido pedido)
        {
            var json = JsonConvert.SerializeObject(pedido);
            var client = new AmazonSQSClient(RegionEndpoint.SAEast1);
            var request = new SendMessageRequest()
            {
                MessageBody = json,
                QueueUrl = $"https://mxlzl3n0l1.execute-api.sa-east-1.amazonaws.com/{fila}"
            };
            await client.SendMessageAsync(request);
        }

        public static async Task EnviarParaFila(EnumFilasSNS fila, Pedido pedido)
        {
            //Implementar
            await Task.CompletedTask;
        }
    }


}
