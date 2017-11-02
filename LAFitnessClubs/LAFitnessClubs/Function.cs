using Amazon.Lambda.Core;
using Amazon.APIGateway;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LAFitnessClubs
{
    public class Function
    {
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string FunctionHandler(ILambdaContext context)
        {
            string toReturn = new ClassesData().Execute(context);
            context.Logger.Log(toReturn);
            return toReturn;
        }      
    }
}
