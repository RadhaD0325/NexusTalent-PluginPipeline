using System;
using Microsoft.Xrm.Sdk;

namespace NexusTalent.PluginSet
{
    public class CandidateFieldValidationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            ITracingService tracing =
                (ITracingService)serviceProvider.GetService(
                    typeof(ITracingService));

            tracing.Trace(
                "CandidateFieldValidationPlugin started. " +
                "Message={0}, Stage={1}, Mode={2}, Depth={3}, CorrelationId={4}",
                context.MessageName,
                context.Stage,
                context.Mode,
                context.Depth,
                context.CorrelationId);

            try
            {
                if (!string.Equals(
                    context.PrimaryEntityName,
                    "nex_candidate",
                    StringComparison.OrdinalIgnoreCase))
                {
                    tracing.Trace("Unexpected table. Exiting.");
                    return;
                }

                if (!context.InputParameters.Contains("Target") ||
                    !(context.InputParameters["Target"] is Entity target))
                {
                    tracing.Trace("Target is not an Entity. Exiting.");
                    return;
                }

                tracing.Trace(
                    "Target ID={0}; Attributes={1}",
                    target.Id,
                    target.Attributes.Count);

                string candidateName = null;
                string email = null;

                if (target.Contains("nex_candidatename"))
                {
                    candidateName =
                        target.GetAttributeValue<string>(
                            "nex_candidatename");
                }

                if (target.Contains("nex_email"))
                {
                    email = target.GetAttributeValue<string>("nex_email");
                }

                if (context.MessageName == "Create")
                {
                    if (string.IsNullOrWhiteSpace(candidateName))
                    {
                        throw new InvalidPluginExecutionException(
                            "Candidate Name is required.");
                    }

                    if (!string.IsNullOrWhiteSpace(email) &&
                        !IsValidEmail(email))
                    {
                        throw new InvalidPluginExecutionException(
                            "Enter a valid email address.");
                    }
                }

                if (context.MessageName == "Update")
                {
                    // On Update, only supplied attributes are available
                    // in Target. Register a Pre-Image to validate the
                    // complete record rather than only changed fields.
                    Entity preImage = null;

                    if (context.PreEntityImages.Contains("PreImage"))
                    {
                        preImage =
                            context.PreEntityImages["PreImage"];
                    }

                    string finalName = target.Contains("nex_candidatename")
                        ? candidateName
                        : preImage?.GetAttributeValue<string>(
                            "nex_candidatename");

                    string finalEmail = target.Contains("nex_email")
                        ? email
                        : preImage?.GetAttributeValue<string>("nex_email");

                    if (string.IsNullOrWhiteSpace(finalName))
                    {
                        throw new InvalidPluginExecutionException(
                            "Candidate Name is required.");
                    }

                    if (!string.IsNullOrWhiteSpace(finalEmail) &&
                        !IsValidEmail(finalEmail))
                    {
                        throw new InvalidPluginExecutionException(
                            "Enter a valid email address.");
                    }
                }

                tracing.Trace(
                    "Candidate validation completed successfully.");
            }
            catch (InvalidPluginExecutionException ex)
            {
                tracing.Trace(
                    "Validation rejected the request: {0}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                tracing.Trace(
                    "Unexpected validation error: {0}", ex.ToString());

                throw new InvalidPluginExecutionException(
                    "Candidate validation failed. Please contact support.");
            }
            finally
            {
                tracing.Trace(
                    "CandidateFieldValidationPlugin finished. CorrelationId={0}",
                    context.CorrelationId);
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var address =
                    new System.Net.Mail.MailAddress(email);

                return string.Equals(
                    address.Address,
                    email,
                    StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}