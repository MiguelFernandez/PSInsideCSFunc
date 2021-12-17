using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Management.Automation;
using System.Net;

namespace FunctionAppPS_Core5
{
    public static class Function1
    {
        [Function("Function1")]
        public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Function1");
            logger.LogInformation("C# HTTP trigger function processed a request.");

           

            var currentDir = Environment.CurrentDirectory;
            var modulesDir = $"{currentDir}\\Modules";
         
            var secret = "your app secret";
            var applicationId = "your app id";
            var tenantId = "your tenant id";
            var subscriptionId = "your sub id";
            
           
            logger.LogInformation($"Current directory : {currentDir}");
            logger.LogInformation($"Modules directory : {modulesDir}");

            //path were the file will be generated
            var filepath = @"c:/home/testfile.txt";
           
            try
            {
                using (var powershell = PowerShell.Create())
                {

                    powershell.AddScript($"Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass");
                    powershell.AddScript($"$password = ConvertTo-SecureString {secret} -AsPlainText -Force");
                    powershell.AddScript($"$credential = New-Object System.Management.Automation.PSCredential ('{applicationId}', $password )");
                    
                    var modulePathAzAccounts = $"{modulesDir}\\Az.Accounts";                    

                    powershell.AddScript($"Import-Module {modulePathAzAccounts}").Invoke();
                    
                    
                    var modulePathAzCompute = $"{modulesDir}\\Az.Compute";
                    powershell.AddScript($"Import-Module {modulePathAzCompute}").Invoke();

                    powershell.AddScript($"Connect-AzAccount -TenantId {tenantId} -SubscriptionId {subscriptionId} -Credential $credential -ServicePrincipal ");
                    powershell.AddScript("Get-AzComputeResourceSku |  Where-Object {$_.ResourceType -eq \"virtualMachines\"} | Out-File -FilePath " + filepath);
                    powershell.Invoke();

                    
                    if (powershell.HadErrors)
                    {
                        foreach (ErrorRecord error in powershell.Streams.Error.ReadAll())
                        {
                            logger.LogInformation(error.ToString());
                        }
                           
                    }

                    var response = req.CreateResponse(HttpStatusCode.OK);
                    response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                    response.WriteString("Function completed");
                    return response;
                }
            }
            catch (Exception ex)
            {
                
                logger.LogInformation(ex.Message);
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            
        }
    }
}
