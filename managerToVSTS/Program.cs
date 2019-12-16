using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace managerToVSTS
{

    public class Program
    {

        public static void Main()
        {
            // CONSTANTES
            Uri orgUrl = new Uri(@"https://dev.azure.com/gruposym/");
            String personalAccessToken = "l6fxys5kplhidykgwvaqkxhysusdza5pxe6fhgj4pomnsjpxfdza";

            VssConnection connection = new VssConnection(orgUrl, new VssBasicCredential(string.Empty, personalAccessToken));
            CreateTask(connection).Wait();

            //GetTask(connection, "NEFRODATA").Wait();

        }

        public static async Task CreateTask(VssConnection connection)
        {
            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();
            

            try
            {
                JsonPatchDocument patchDocument = new JsonPatchDocument();
                // título da solicitação
                patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = "titulo"
                }
                );

                // descrição da solicitação
                patchDocument.Add(
                   new JsonPatchOperation()
                   {
                       Operation = Operation.Add,
                       Path = "/fields/Custom.c474834f-4b08-42ac-aaa3-1bcf80e3ae66",
                       Value = "descricao"
                   }
               );
                // razão social do cliente
                patchDocument.Add(
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/Custom.ClienteLista",
                        Value = "XCLIENTE DE TESTE - NÃO APAGAR"
                    }
                );
                // responsável pelo cadastro
                patchDocument.Add(
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/Custom.Contato",
                        Value = "PAULO"
                    }
                );
                // protocolo
                patchDocument.Add(
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/Custom.ProtocoloManager",
                        Value = "999999"
                    }
                );
                // sistema/produto
                //https://dev.azure.com/gruposym/_apis/projects?api-version=5.1
                patchDocument.Add(
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/Custom.Produto",
                        Value = "NEFRODATA ACD"
                    }
                );
                //prioridade
                patchDocument.Add(
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/Custom.Prioridade",
                        Value = "2 - Médio"
                    }
                );

                WorkItem workitem = await witClient.CreateWorkItemAsync(patchDocument, "Nefrodata", "Erro");

            }
            catch (AggregateException aex)
            {
                VssServiceException vssex = aex.InnerException as VssServiceException;
                if (vssex != null)
                {
                    Console.WriteLine(vssex.Message);
                }
            }

        }

        public static async Task GetTask(VssConnection connection, string projeto)
        {
            WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            //WorkItem workitem = await witClient.GetWorkItemAsync(4621);

            List<QueryHierarchyItem> queryHierarchyItems = witClient.GetQueriesAsync(projeto, depth: 2).Result;

            QueryHierarchyItem myQueriesFolder = queryHierarchyItems.FirstOrDefault(qhi => qhi.Name.Equals("Shared Queries"));
            if (myQueriesFolder != null)
            {
                string queryName = "Integra";

                // See if our 'REST Sample' query already exists under 'My Queries' folder.
                QueryHierarchyItem newBugsQuery = null;
                if (myQueriesFolder.Children != null)
                {
                    newBugsQuery = myQueriesFolder.Children.FirstOrDefault(qhi => qhi.Name.Equals(queryName));
                }
                if (newBugsQuery == null)
                {
                    // if the 'REST Sample' query does not exist, create it.
                    newBugsQuery = new QueryHierarchyItem()
                    {
                        Name = queryName,
                        Wiql = "SELECT [System.Id],[System.WorkItemType],[System.Title],[System.AssignedTo],[System.State],[System.Tags] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.WorkItemType] = 'Bug' AND [System.State] = 'New'",
                        IsFolder = false
                    };
                    newBugsQuery = witClient.CreateQueryAsync(newBugsQuery, projeto, myQueriesFolder.Name).Result;
                }

                WorkItemQueryResult result = witClient.QueryByIdAsync(newBugsQuery.Id).Result;

                if (result.WorkItems.Any())
                {
                    int skip = 0;
                    const int batchSize = 100;
                    IEnumerable<WorkItemReference> workItemRefs;
                    do
                    {
                        workItemRefs = result.WorkItems.Skip(skip).Take(batchSize);
                        if (workItemRefs.Any())
                        {
                            // get details for each work item in the batch
                            List<WorkItem> workItems = witClient.GetWorkItemsAsync(workItemRefs.Select(wir => wir.Id)).Result;
                            foreach (WorkItem workItem in workItems)
                            {
                                var teste = workItem;
                            }
                        }
                        skip += batchSize;
                    }
                    while (workItemRefs.Count() == batchSize);
                }

            }
        }


    }
}
