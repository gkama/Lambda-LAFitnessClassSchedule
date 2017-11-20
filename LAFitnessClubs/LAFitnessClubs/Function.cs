using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Collections.Generic;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LAFitnessClubs
{
    public class Function
    {
        private static string states = "|AL|AK|AS|AZ|AR|CA|CO|CT|DE|DC|FM|FL|GA|GU|HI|ID|IL|IN|IA|KS|KY|LA|ME|MH|MD|MA|MI|MN|MS|MO|MT|NE|NV|NH|NJ|NM|NY|NC|ND|MP|OH|OK|OR|PW|PA|PR|RI|SC|SD|TN|TX|UT|VT|VI|VA|WA|WV|WI|WY|";
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public response FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            //State & Zip
            string state = request.QueryStringParameters["state"].ToUpper();
            string zipCode = request.QueryStringParameters["zipcode"];

            if (state.Length == 2 && states.IndexOf(state) > 0)
            {
                string toReturn = new ClassesData().Execute(context, state, zipCode);
                context.Logger.Log(toReturn);
                var response = new response()
                {
                    statusCode = "200",
                    headers = new Dictionary<string, string>() { { "Access-Control-Allow-Origin", "*" } },
                    body = toReturn
                };
                return response;
            }
            else
            {
                var response = new response()
                {
                    statusCode = "400",
                    headers = new Dictionary<string, string>() { { "Access-Control-Allow-Origin", "*" } },
                    body = "Not found"
                };
                return response;
            }
        }
        public class response
        {
            public string statusCode { get; set; }
            public Dictionary<string, string> headers { get; set; }
            public string body { get; set; }
        }
    }
}
