
using System;
using System.Globalization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace NexusTalent.PluginSet
{
    public class CandidateAutoNumberPlugin : IPlugin
    {
        private const string EntityName = "nex_candidate";
        private const string CodeColumn = "nex_candidatecode";
        private const string Prefix = "CAN-";

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            var tracing = (ITracingService)
                serviceProvider.GetService(typeof(ITracingService));

            tracing.Trace(
                "AUTONUM-V2 START | Message={0} | Stage={1} | CorrelationId={2}",
                context.MessageName,
                context.Stage,
                context.CorrelationId);

            try
            {
                if (context.MessageName != "Create" ||
                    context.PrimaryEntityName != EntityName)
                {
                    tracing.Trace("AUTONUM-V2: Not a Candidate Create. Exiting.");
                    return;
                }

                if (!context.InputParameters.Contains("Target") ||
                    !(context.InputParameters["Target"] is Entity target))
                {
                    throw new InvalidPluginExecutionException(
                        "Candidate AutoNumber: Target record was not found.");
                }

                // Only generate a code when one has not already been supplied.
                string suppliedCode =
                    target.GetAttributeValue<string>(CodeColumn);

                if (!string.IsNullOrWhiteSpace(suppliedCode))
                {
                    tracing.Trace(
                        "AUTONUM-V2: A Candidate Code was supplied. Skipping.");
                    return;
                }

                var factory = (IOrganizationServiceFactory)
                    serviceProvider.GetService(
                        typeof(IOrganizationServiceFactory));

                var service =
                    factory.CreateOrganizationService(context.UserId);

                int highestNumber = 0;
                int scannedRecords = 0;

                var query = new QueryExpression(EntityName)
                {
                    ColumnSet = new ColumnSet(CodeColumn),
                    PageInfo = new PagingInfo
                    {
                        Count = 5000,
                        PageNumber = 1
                    }
                };

                query.Criteria.AddCondition(
                    CodeColumn, ConditionOperator.NotNull);

                while (true)
                {
                    EntityCollection results = service.RetrieveMultiple(query);

                    foreach (Entity record in results.Entities)
                    {
                        scannedRecords++;

                        string existingCode =
                            record.GetAttributeValue<string>(CodeColumn);

                        if (string.IsNullOrWhiteSpace(existingCode) ||
                            !existingCode.StartsWith(
                                Prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            // Ignore legacy values such as "yuyuyu".
                            continue;
                        }

                        string suffix = existingCode.Substring(Prefix.Length);

                        if (int.TryParse(
                            suffix,
                            NumberStyles.None,
                            CultureInfo.InvariantCulture,
                            out int number) &&
                            number > highestNumber)
                        {
                            highestNumber = number;
                        }
                    }

                    if (!results.MoreRecords)
                    {
                        break;
                    }

                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = results.PagingCookie;
                }

                tracing.Trace(
                    "AUTONUM-V2: Scanned={0}, HighestNumber={1}",
                    scannedRecords, highestNumber);

                int nextNumber = checked(highestNumber + 1);

                string newCode = Prefix + nextNumber.ToString(
                    "D6", CultureInfo.InvariantCulture);

                target[CodeColumn] = newCode;

                tracing.Trace(
                    "AUTONUM-V2: Generated Candidate Code={0}", newCode);
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracing.Trace("AUTONUM-V2 ERROR: {0}", ex.ToString());

                throw new InvalidPluginExecutionException(
                    "Unable to generate the Candidate Code. " +
                    "Please check the plug-in trace log.");
            }
            finally
            {
                tracing.Trace(
                    "AUTONUM-V2 END | CorrelationId={0}",
                    context.CorrelationId);
            }
        }
    }
}
